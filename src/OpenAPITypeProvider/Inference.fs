module OpenAPITypeProvider.Inference

open System
open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider.Specification

let private getIntType = function
    | IntFormat.Int32 -> typeof<int>
    | IntFormat.Int64 -> typeof<int64>

let private getNumberType = function
    | NumberFormat.Float -> typeof<float32>
    | NumberFormat.Double -> typeof<double>

let private getStringType = function
    | StringFormat.String | StringFormat.Password | StringFormat.Binary -> typeof<string>
    | StringFormat.Byte -> typeof<byte>
    | StringFormat.Date | StringFormat.DateTime -> typeof<DateTime>

let getPropertyType schema =
    match schema with
    | Boolean -> typeof<bool>
    | Integer format -> format |> getIntType
    | Number format -> format |> getNumberType
    | String format -> format |> getStringType

let rec getType (existingTypes:(Schema * ProvidedTypeDefinition) list) schema = 
    match schema with
    | Boolean -> typeof<bool>
    | Integer format -> format |> getIntType
    | Number format -> format |> getNumberType
    | String format -> format |> getStringType
    | Array schema -> 
        let typ = schema |> getType existingTypes
        ProvidedTypeBuilder.MakeGenericType (typedefof<List<_>>, [typ])
    | Object _ -> 
        existingTypes
        |> Map
        |> Map.find schema :> Type