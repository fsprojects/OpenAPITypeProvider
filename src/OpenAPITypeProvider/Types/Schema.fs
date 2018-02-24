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
    | StringFormat.Byte -> typeof<byte> //makeProperty<byte> (fun _ -> <@@ () @@>) name :> MemberInfo
    | StringFormat.Binary -> typeof<int> // (fun _ -> <@@ () @@>) name :> MemberInfo
    | StringFormat.Date | StringFormat.DateTime -> typeof<DateTime>
    
let rec getType = function
    | Boolean -> typeof<bool>
    | Integer format -> format |> getIntType
    | Number format -> format |> getNumberType
    | String format -> format |> getStringType
    | Array schema -> 
        let typ = schema |> getType
        let obj = Array.CreateInstance(typ, 1)
        obj.GetType()

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
    | Array _ -> schema |> getType |> makeTypedProperty (fun _ -> <@@ () @@>) name :> MemberInfo    