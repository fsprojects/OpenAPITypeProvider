namespace OpenAPITypeProvider.Json.Types

open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Parser
open OpenAPITypeProvider.Specification

type SimpleValue(json, schema, existingTypes) =
    let schema = schema |> Serialization.deserialize<Schema>
    let value = json |> Newtonsoft.Json.Linq.JToken.Parse |> parseForSchema existingTypes schema
    member __.Value = value