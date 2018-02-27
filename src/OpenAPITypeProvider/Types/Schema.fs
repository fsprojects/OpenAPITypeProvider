module OpenAPITypeProvider.Types.Schema

open ProviderImplementation.ProvidedTypes
open System.Reflection
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open System

let getIntType = function
    | IntFormat.Int32 -> typeof<int>
    | IntFormat.Int64 -> typeof<int64>

let getNumberType = function
    | NumberFormat.Float -> typeof<float>
    | NumberFormat.Double -> typeof<double>

let getStringType = function
    | StringFormat.String | StringFormat.Password -> typeof<string>
    | StringFormat.Byte -> typeof<byte>
    | StringFormat.Binary -> typeof<int>
    | StringFormat.Date | StringFormat.DateTime -> typeof<DateTime>
    
let rec getType = function
    | Boolean -> typeof<bool>
    | Integer format -> format |> getIntType
    | Number format -> format |> getNumberType
    | String format -> format |> getStringType
    | Array schema -> 
        let typ = schema |> getType
        ProvidedTypeBuilder.MakeGenericType (typedefof<List<_>>, [typ])

let some (typ:Type) arg =
    let unionType = typedefof<option<_>>.MakeGenericType typ
    let meth = unionType.GetMethod("Some")
    Microsoft.FSharp.Quotations.Expr.Call(meth, [arg])

let rec getMembers asm ns (parent:ProvidedTypeDefinition) name (schema:Schema) =
    let name = Names.pascalName name
    match schema with
    | Boolean -> 
        let typ = schema |> getType
        ProvidedProperty(name, typ, (fun _ -> <@@ () @@>)) :> MemberInfo
    | Object (props, required) ->
        let typ = ProvidedTypeDefinition(asm, ns, name, None, hideObjectMethods = true, nonNullable = true)

        let p = 
            props 
            |> Map.toList 
            |> List.map (fun (n,v) -> (Names.pascalName n), (getType v)) 
            |> List.map (fun (n,v) -> ProvidedParameter(n, v))
        
        let ctor = ProvidedConstructor(p, (fun _ -> <@@ () @@>))
        typ.AddMember(ctor)
        
        let mth = ProvidedMethod("Parse", [ProvidedParameter("Json", typeof<string>)], typ, (fun _ -> <@@ () @@>), isStatic = true)
        typ.AddMember(mth)

        props 
        |> Map.map (getMembers asm ns parent) 
        |> Map.iter (fun _ mem -> typ.AddMember(mem))
        
        typ |> parent.AddMember
        ProvidedProperty(name, typ, (fun _ -> <@@ typ @@>)) |> parent.AddMember
        typ :> MemberInfo    
    | Integer _ -> 
        let typ = schema |> getType 
        ProvidedProperty(name, typ, (fun _ -> <@@ () @@>)) :> MemberInfo
    | Number _ ->
        let typ = schema |> getType 
        ProvidedProperty(name, typ, (fun _ -> <@@ () @@>)) :> MemberInfo
    | String _ ->
        let typ = schema |> getType 
        ProvidedProperty(name, typ, (fun _ -> <@@ () @@>)) :> MemberInfo
    | Array _ -> 
        //let v = some typeof<string> (Microsoft.FSharp.Quotations.Expr.Value("x"))
        //let values = Option<string>.Some("x")
        let typ = schema |> getType 
        ProvidedProperty(name, typ, (fun _ -> <@@ () @@>)) :> MemberInfo
