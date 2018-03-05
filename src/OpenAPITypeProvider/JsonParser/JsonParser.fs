module OpenAPITypeProvider.JsonParser.JsonParser

open OpenAPITypeProvider.Specification
open Newtonsoft.Json.Linq

let tryCast castFn = 
    try
        castFn() |> ignore
        true
    with _ -> false


let isSchemaAndJTokenCompatible (json:JToken) (schema:Schema) =
    match schema with
    | Integer Int32 -> tryCast (fun _ -> json.Value<int32>())
    | Integer Int64 -> tryCast (fun _ -> json.Value<int64>())
    | String StringFormat.String 
    | String StringFormat.Binary 
    | String StringFormat.Password
        -> tryCast (fun _ -> json.Value<string>())
    | String StringFormat.Byte -> tryCast (fun _ -> json.Value<byte>())
    | String StringFormat.DateTime
    | String StringFormat.Date  
        -> tryCast (fun _ -> json.Value<string>())
        

// let parseJToken (json:JToken) (schema:Schema) =
//     match schema with
//     | Integer format ->
//         match format with
//         | IntFormat.Int32 -> json.Value() :?> int