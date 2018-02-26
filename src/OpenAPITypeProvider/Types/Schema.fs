module OpenAPITypeProvider.Types.Schema

open ProviderImplementation.ProvidedTypes
open System.Reflection
open OpenAPITypeProvider.Types.Helpers
open OpenAPITypeProvider.Parser
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
        ProvidedTypeBuilder.MakeGenericType (typedefof<Option<_>>, [typ])
        //let obj = Array.CreateInstance(typ, 1)
        //obj.GetType()

let some (typ:Type) arg =
    let unionType = typedefof<option<_>>.MakeGenericType typ
    let meth = unionType.GetMethod("Some")
    Microsoft.FSharp.Quotations.Expr.Call(meth, [arg])

let rec getMembers asm ns (parent:ProvidedTypeDefinition) name (schema:Schema) =
    match schema with
    | Boolean -> schema |> getType |> makeTypedProperty (fun _ -> <@@ () @@>) name :> MemberInfo
    | Object (props, required) ->
        let ob = ProvidedTypeDefinition(asm, ns, name, None, hideObjectMethods = true, nonNullable = true)
        props 
        |> Map.map (getMembers asm ns parent) 
        |> Map.iter (fun _ mem -> ob.AddMember(mem))
        ob |> addAsProperty name "TODO" parent
        ob :> MemberInfo    
    | Integer _ -> schema |> getType |> makeTypedProperty (fun _ -> <@@ () @@>) name :> MemberInfo
    | Number _ -> schema |> getType |> makeTypedProperty (fun _ -> <@@ () @@>) name :> MemberInfo
    | String _ -> schema |> getType |> makeTypedProperty (fun _ -> <@@ () @@>) name :> MemberInfo
    | Array _ -> 
        let v = some typeof<string> (Microsoft.FSharp.Quotations.Expr.Value("x"))
        //let values = Option<string>.Some("x")
        schema |> getType |> makeTypedProperty (fun _ -> v ) name :> MemberInfo    