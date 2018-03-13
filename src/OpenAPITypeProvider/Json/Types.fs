namespace OpenAPITypeProvider.Json.Types

open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Parser
open OpenAPITypeProvider.Specification
open Newtonsoft.Json.Linq

type SimpleValue(json, schema) =
    let schema = schema |> Serialization.deserialize<Schema>
    let value = json |> Newtonsoft.Json.Linq.JToken.Parse |> parseForSchema schema
    member __.Value = value