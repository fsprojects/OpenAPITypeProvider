namespace OpenAPIProvider

open OpenAPIProvider.JsonParser.JsonParser




type JsonValue (json,schema) =
    let value = json |> Newtonsoft.Json.Linq.JToken.Parse |> parseForSchema schema
    member __.RawValue = value
    static member Parse (json, schema) = JsonValue(json, schema)