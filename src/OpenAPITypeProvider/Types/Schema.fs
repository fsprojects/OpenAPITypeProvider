module OpenAPITypeProvider.Types.Schema

open ProviderImplementation.ProvidedTypes
open System.Reflection
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open System
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations

let asOption (typ:Type) = typedefof<option<_>>.MakeGenericType typ
    

let private (|NonObjectSchema|_|) (schema:Schema) =
    match schema with
    | Schema.Boolean 
    | Schema.Array _
    | Schema.Integer _
    | Schema.Number _
    | Schema.String _ -> Some ()
    | Schema.Object _ -> None

let private (|ObjectSchema|_|) (schema:Schema) =
    match schema with
    | Schema.Object (props, required) -> Some (props, required)
    | Schema.Boolean 
    | Schema.Array _
    | Schema.Integer _
    | Schema.Number _
    | Schema.String _ -> None

let private createNonObjectType asm ns name (schema:Schema) =
    
    let typ = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    let schemaType = schema |> Inference.getType Map.empty
    let strSchema = schema |> Serialization.serialize

    // constructor
    ProvidedConstructor([ProvidedParameter("value", schemaType)], (fun args ->
        let value = Expr.Coerce(args.Head, typeof<obj>)
        <@@
            SimpleValue(%%value)
        @@>)) |> typ.AddMember

    // static method Parse
    ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>)], typ, (fun args -> 
        <@@ 
            let json = %%args.Head : string
            SimpleValue(json, strSchema, Map.empty)
        @@>), isStatic = true)
        |> typ.AddMember
        
    // Value(s) property
    let valueMethodName = 
        match schema with
        | Schema.Array _ -> "Values"
        | _ -> "Value"

    ProvidedProperty(valueMethodName, schemaType, (fun args -> 
        let t = args.[0]
        <@@  
            let simpleValue = ((%%t : obj) :?> SimpleValue)
            simpleValue.Value
        @@>)) |> typ.AddMember

    // ToJToken method
    ProvidedMethod("ToJToken", [], typeof<Newtonsoft.Json.Linq.JToken>, (fun args -> 
        let t = args.[0]
        <@@ 
            let simpleValue = ((%%t : obj ):?> SimpleValue)
            simpleValue.Value |> Newtonsoft.Json.Linq.JToken.FromObject
        @@>))
        |> typ.AddMember
    typ

let getNonObjectType asm ns name schema =
    let name = Names.pascalName name
    match schema with
    | NonObjectSchema -> schema |> createNonObjectType asm ns name |> Some
    | _ -> None
    
let rec createObjectType asm ns (isOptional:bool) originalName (schema:Schema) =
    let name = Names.pascalName originalName
    match schema with
    | ObjectSchema (props, required) ->
        let strSchema = schema |> Serialization.serialize
        let isOptional n = required |> List.contains n |> not
        
        let typ = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
        
        // constructor
        let getCtorParam (name,typ) =
            if name |> isOptional then
                ProvidedParameter(name, (typ |> asOption), false, null)
            else ProvidedParameter(name, typ)

        let ctorParams = 
            props 
            |> Map.toList 
            |> List.map (fun (n,v) -> n, (v |> Inference.getType Map.empty))
            |> List.sortBy (fun (n,_) -> isOptional n)
            |> List.map getCtorParam 
        
        ProvidedConstructor(ctorParams, (fun args -> 
            let values =
                Expr.NewArray(typeof<string * obj>,
                    args
                    |> Seq.toList
                    |> List.mapi(fun i v -> Expr.NewTuple [ Expr.Value ctorParams.[i].Name ; Expr.Coerce(v, typeof<obj>) ] ))
                
            <@@  
            let v = (%%values : (string * obj) array) |> Array.toList 
            ObjectValue(v)
            @@>)) |> typ.AddMember


        // properties
        props 
        |> Map.map (fun n s -> createObjectType asm ns (isOptional n) n s) 
        |> Map.iter (fun _ mem -> typ.AddMember(mem))

        // static method Parse
        ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>)], typ, (fun args -> 
            <@@ 
                let json = %%args.Head : string
                ObjectValue(json, strSchema, Map.empty)
            @@>), isStatic = true)
            |> typ.AddMember

        typ :> MemberInfo
    
    | _ ->
        let typ = schema |> Inference.getType Map.empty |> (fun x -> if isOptional then x |> asOption else x)
        ProvidedProperty(name, typ, (fun args -> 
            let t = args.[0]
            <@@  
                let objectValue = ((%%t : obj) :?> ObjectValue)
                objectValue.GetValue(originalName)
            @@>)) :> MemberInfo

let getObjectType asm ns name schema =
    match schema with
    | ObjectSchema _ -> schema |> createObjectType asm ns false name |> Some
    | _ -> None
