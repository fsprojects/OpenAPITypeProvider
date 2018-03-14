module OpenAPITypeProvider.Types.Schema

open ProviderImplementation.ProvidedTypes
open System.Reflection
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open System
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types

let some (typ:Type) arg =
    let unionType = typedefof<option<_>>.MakeGenericType typ
    let meth = unionType.GetMethod("Some")
    Microsoft.FSharp.Quotations.Expr.Call(meth, [arg])

let private onlyNonObject (schema:Schema) =
    match schema with
    | Schema.Boolean 
    | Schema.Array _
    | Schema.Integer _
    | Schema.Number _
    | Schema.String _ -> Some schema
    | Schema.Object _ -> None

let private createRootNonObjectType asm ns name (schema:Schema) =
    
    let typ = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    let schemaType = schema |> Inference.getType Map.empty
    let strSchema = schema |> Serialization.serialize

    // add static method Parse
    ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>)], typ, (fun args -> 
        <@@ 
            let json = %%args.Head : string
            SimpleValue(json, strSchema, Map.empty)
        @@>), isStatic = true)
        |> typ.AddMember
        
    // add Value(s) property
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

    // add ToJToken method
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
    schema
    |> onlyNonObject
    |> Option.map (createRootNonObjectType asm ns name)
    

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
