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

let private createRootNonObjectType asm ns name (schema:Schema) =
    
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

let getRootNonObjectType asm ns name schema =
    let name = Names.pascalName name
    match schema with
    | NonObjectSchema -> schema |> createRootNonObjectType asm ns name |> Some
    | _ -> None
    
let rec createRootObjectType asm ns name (schema:Schema) =
    let name = Names.pascalName name
    match schema with
    | ObjectSchema (props, required) ->
        
        let typ = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
        
        // constructor
        let p = 
            props 
            |> Map.toList 
            |> List.map (fun (n,v) -> (Names.pascalName n), (v |> Inference.getType Map.empty)) 
            |> List.map (fun (n,v) -> ProvidedParameter(n, asOption v))
        
        ProvidedConstructor(p, (fun args -> 
            let values =
                Expr.NewArray(typeof<string * obj>,
                    args
                    |> Seq.toList
                    |> List.mapi(fun i v -> Expr.NewTuple [ Expr.Value p.[i].Name ; Expr.Coerce(v, typeof<obj>) ] ))
                
            <@@  
            let v = (%%values : (string * obj) array) |> Array.toList
            ObjectValue(v)
            @@>)) |> typ.AddMember

        // properties
        props 
         |> Map.map (createRootObjectType asm ns) 
         |> Map.iter (fun _ mem -> typ.AddMember(mem))

        typ :> MemberInfo
    | _ ->
        let typ = schema |> Inference.getType Map.empty |> asOption
        ProvidedProperty(name, typ, (fun args -> 
            let t = args.[0]
            <@@  
            let objectValue = ((%%t : obj) :?> ObjectValue)
            objectValue.GetValue(name)
            @@>)) :> MemberInfo

let getRootObjectType asm ns name schema =
    let name = Names.pascalName name
    match schema with
    | ObjectSchema _ -> schema |> createRootObjectType asm ns name |> Some
    | _ -> None

// let rec getMembers asm ns (parent:ProvidedTypeDefinition) name (schema:Schema) =
//     let name = Names.pascalName name
//     match schema with
//     | Boolean -> 
//         let typ = schema |> getType
//         ProvidedProperty(name, typ, (fun _ -> <@@ () @@>)) :> MemberInfo
//     | Object (props, required) ->
//         let typ = ProvidedTypeDefinition(asm, ns, name, None, hideObjectMethods = true, nonNullable = true, isErased = true)

//         let p = 
//             props 
//             |> Map.toList 
//             |> List.map (fun (n,v) -> (Names.pascalName n), (getType v)) 
//             |> List.map (fun (n,v) -> ProvidedParameter(n, v))
        
//         let ctor = ProvidedConstructor(p, (fun _ -> <@@ () @@>))
//         typ.AddMember(ctor)
        
//         let mth = ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>)], typ, (fun _ -> <@@ () @@>), isStatic = true)
//         typ.AddMember(mth)

//         props 
//         |> Map.map (getMembers asm ns parent) 
//         |> Map.iter (fun _ mem -> typ.AddMember(mem))
        
//         typ |> parent.AddMember
//         ProvidedProperty(name, typ, (fun _ -> <@@ typ @@>)) |> parent.AddMember
//         typ :> MemberInfo    
//     | Integer _ -> 
//         let typ = schema |> getType 
//         ProvidedProperty(name, typ, (fun _ -> <@@ () @@>)) :> MemberInfo
//     | Number _ ->
//         let typ = schema |> getType 
//         ProvidedProperty(name, typ, (fun _ -> <@@ () @@>)) :> MemberInfo
//     | String _ ->
//         let typ = schema |> getType 
//         ProvidedProperty(name, typ, (fun _ -> <@@ () @@>)) :> MemberInfo
//     | Array _ -> 
//         //let v = some typeof<string> (Microsoft.FSharp.Quotations.Expr.Value("x"))
//         //let values = Option<string>.Some("x")
//         let typ = schema |> getType 
//         ProvidedProperty(name, typ, (fun _ -> <@@ () @@>)) :> MemberInfo
