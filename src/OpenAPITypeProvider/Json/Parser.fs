module internal OpenAPITypeProvider.Json.Parser

open System
open OpenAPIParser.Version3.Specification
open Newtonsoft.Json.Linq
open OpenAPITypeProvider

let private checkRequiredProperties (req:string list) (jObject:JObject) =
    let props = jObject.Properties() |> Seq.toList
    let propertyExist name = props |> List.exists (fun x -> x.Name = name && x.Value.Type <> JTokenType.Null)
    req |> List.iter (fun p ->
        if propertyExist p |> not then raise <| FormatException (sprintf "Property '%s' is required by schema definition, but not present in JSON or is null" p)
    )

type ReflectiveListBuilder = 
    static member BuildList<'a> (args: obj list) = 
        [ for a in args do yield a :?> 'a ] 
    static member BuildTypedList lType (args: obj list) = 
        typeof<ReflectiveListBuilder>
            .GetMethod("BuildList")
            .MakeGenericMethod([|lType|])
            .Invoke(null, [|args|])

let private some (typ:Type) arg =
    let unionType = typedefof<option<_>>.MakeGenericType typ
    let meth = unionType.GetMethod("Some")
    meth.Invoke(null, [|arg|])

let rec parseForSchema createObj (schema:Schema) (json:JToken) =
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
        let items = [ for x in jArray do yield parseForSchema createObj itemsSchema x ]
        let typ = itemsSchema |> Inference.getComplexType (fun _ -> typeof<obj>)
        ReflectiveListBuilder.BuildTypedList typ items |> box
    | Object (props, required) ->
        let jObject = json :?> JObject
        jObject |> checkRequiredProperties required
        props 
        |> Map.map (fun name schema -> 
            if required |> List.contains name then
                parseForSchema createObj schema (jObject.[name]) |> Some
            else if jObject.ContainsKey name then
                let typ = schema |> Inference.getComplexType (fun _ -> typeof<obj>)
                if jObject.[name].Type = JTokenType.Null then None
                else
                    parseForSchema createObj schema (jObject.[name]) 
                    |> some typ
                    |> Some
            else None
        )
        |> Map.filter (fun _ v -> v.IsSome)
        |> Map.map (fun _ v -> v.Value)
        |> Map.toList
        |> createObj
        |> box