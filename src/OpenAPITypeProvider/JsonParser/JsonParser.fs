module OpenAPITypeProvider.JsonParser.JsonParser

open System
open OpenAPITypeProvider.Specification
open Newtonsoft.Json.Linq

let parseForSchema (schema:Schema) (json:JToken) =
    match schema with
    | Integer Int32 -> json.Value<int32>() |> box
    | Integer Int64 -> json.Value<int64>() |> box
    | Number NumberFormat.Double -> json.Value<double>() |> box
    | Number NumberFormat.Float -> json.Value<float32>() |> box
    | String StringFormat.String 
    | String StringFormat.Binary 
    | String StringFormat.Password -> json.Value<string>() |> box
    | String StringFormat.Byte -> json.Value<byte>() |> box
    | String StringFormat.DateTime
    | String StringFormat.Date -> json.Value<DateTime>() |> box

// let parseJToken (json:JToken) (schema:Schema) =
//     match schema with
//     | Integer format ->
//         match format with
//         | IntFormat.Int32 -> json.Value() :?> int