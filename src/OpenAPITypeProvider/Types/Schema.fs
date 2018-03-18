module OpenAPITypeProvider.Types.Schema

open ProviderImplementation.ProvidedTypes
open System.Reflection
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open System
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations

let asOption (typ:#Type) = typedefof<option<_>>.MakeGenericType typ

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

type ObjectWrapper = {
    Schema : Schema
    Name : string
    PascalName : string
    IsOptional : bool
    Children : ObjectWrapper list

}

type ValueType =
    | Object of ProvidedTypeDefinition
    | Property of ProvidedProperty

    
let rec createObjectType asm ns (isOptional:bool) originalName (schema:Schema) existingObjects =
    let name = Names.pascalName originalName
    match schema with
    | ObjectSchema (props, required) ->
        let isOptional n = required |> List.contains n |> not
        
        let typ = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
        
        let foldFn acc n s =
            match createObjectType asm ns (isOptional n) n s acc with
            | ValueType.Object newType ->
                typ.AddMember(newType)
                ProvidedProperty(Names.pascalName n, newType, fun args -> 
                    <@@  
                        let objectValue = ((%%args.[0] : obj) :?> ObjectValue)
                        objectValue.GetValue(n)
                    @@>) |> typ.AddMember
                (s, newType) :: acc
            | ValueType.Property prop ->
                typ.AddMember(prop)
                acc

        // properties
        let existingObjects = props |> Map.fold foldFn existingObjects
        
        //let findType n s = 
        //    match existingObjects |> Map |> Map.tryFind s with
        //    | Some e -> e :> MemberInfo
        //    | None -> 
        //        match createObjectType asm ns (isOptional n) n s existingObjects with
        //        | ValueType.Object o -> o :> MemberInfo
        //        | ValueType.Property p -> p :> MemberInfo

        // constructor
        let getCtorParam (name,typ) =
            if name |> isOptional then
                ProvidedParameter(name, (typ |> asOption), false, null)
            else ProvidedParameter(name, typ)

        let ctorParams = 
            props 
            |> Map.toList 
            |> List.map (fun (n,s) -> n, s |> Inference.getType (Map existingObjects))
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
        

        // static method Parse
        let strSchema = schema |> Serialization.serialize
        
        ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>)], typ, (fun args -> 
            <@@ 
                let json = %%args.Head : string
                ObjectValue(json, strSchema, Map.empty)
            @@>), isStatic = true)
            |> typ.AddMember

        // ToJToken method
        ProvidedMethod("ToJToken", [], typeof<Newtonsoft.Json.Linq.JToken>, (fun args -> 
            let t = args.[0]
            <@@ 
                let objectValue = ((%%t : obj ):?> ObjectValue)
                objectValue.Data |> dict |> Newtonsoft.Json.Linq.JObject.FromObject
            @@>))
            |> typ.AddMember

        typ |> ValueType.Object
    
    | _ ->
        let typ = schema |> Inference.getType (Map existingObjects) |> (fun x -> if isOptional then x |> asOption else x)
        ProvidedProperty(name, typ, (fun args -> 
            let t = args.[0]
            <@@  
                let objectValue = ((%%t : obj) :?> ObjectValue)
                objectValue.GetValue(originalName)
            @@>)) |> ValueType.Property

let getObjectType asm ns name schema =
    match schema with
    | ObjectSchema _ -> 
        [] 
        |> createObjectType asm ns false name schema
        |> function
            | ValueType.Object o -> Some o
            | ValueType.Property _ -> None
    | _ -> None
