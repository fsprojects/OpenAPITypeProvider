namespace OpenAPITypeProvider.Json.Types

open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Parser
open OpenAPITypeProvider.Specification

type SimpleValue(value) =
    
    new(json, schema, existingTypes) =
        let schema = schema |> Serialization.deserialize<Schema>
        let v = json |> Newtonsoft.Json.Linq.JToken.Parse |> parseForSchema existingTypes schema
        SimpleValue(v)

    member __.Value = value