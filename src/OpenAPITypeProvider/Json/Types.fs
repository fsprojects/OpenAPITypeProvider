namespace OpenAPITypeProvider.Json.Types

open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Parser
open OpenAPITypeProvider.Specification
open System.Collections.Generic

type SimpleValue(value) =
    
    new(json, schema, existingTypes) =
        let schema = schema |> Serialization.deserialize<Schema>
        let v = json |> Newtonsoft.Json.Linq.JToken.Parse |> parseForSchema existingTypes schema
        SimpleValue(v)

    member __.Value = value

type ObjectValue(d:(string * obj) list) =
    let mutable data = d |> Map |> Dictionary
    
    new(json, schema, existingTypes) =
        let schema = schema |> Serialization.deserialize<Schema>
        let v = json |> Newtonsoft.Json.Linq.JToken.Parse |> parseForSchema existingTypes schema :?> (string * obj) list
        ObjectValue(v)
    
    member __.SetValue(x, y) = data.[x] <- y
    member __.GetValue x = if data.ContainsKey x then data.[x] else null