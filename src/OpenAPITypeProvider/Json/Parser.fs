module OpenAPITypeProvider.Json.Parser

open System
open OpenAPITypeProvider.Specification
open Newtonsoft.Json.Linq
open OpenAPITypeProvider.Types

let checkRequiredProperties (req:string list) (jObject:JObject) =
    let propertyExist name = jObject.Properties() |> Seq.exists (fun x -> x.Name = name)
    req |> List.iter (fun p ->
        if propertyExist p |> not then failwith (sprintf "Property %s is required by schema, but no present in JObject" p)
    )

type ReflectiveListBuilder = 
    static member BuildList<'a> (args: obj list) = 
        [ for a in args do yield a :?> 'a ] 
    static member BuildTypedList lType (args: obj list) = 
        typeof<ReflectiveListBuilder>
            .GetMethod("BuildList")
            .MakeGenericMethod([|lType|])
            .Invoke(null, [|args|])

let rec parseForSchema (schema:Schema) (json:JToken) =
    match schema with
    | Boolean -> json.Value<bool>() |> box
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
    | Array itemsSchema ->
        let jArray = json :?> JArray
        let items = [ for x in jArray do yield parseForSchema itemsSchema x ]
        let typ = itemsSchema |> Inference.getType
        ReflectiveListBuilder.BuildTypedList typ items |> box
    | Object (props, required) ->
        let jObject = json :?> JObject
        jObject |> checkRequiredProperties required
        props |> Map.map (fun name schema -> 
            parseForSchema schema (jObject.[name])
        ) :> obj

