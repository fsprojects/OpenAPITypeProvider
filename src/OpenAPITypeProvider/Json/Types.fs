namespace OpenAPITypeProvider.Json.Types

open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Parser
open OpenAPITypeProvider.Specification
open System.Collections.Generic
open Newtonsoft.Json.Linq

type ObjectValue(d:(string * obj) list) =
    
    let mutable data = d |> Map |> Dictionary
    new(json, schema, existingTypes) =
        let schema = schema |> Serialization.deserialize<Schema>
        let v = json |> Newtonsoft.Json.Linq.JToken.Parse |> parseForSchema ObjectValue existingTypes schema :?> ObjectValue
        ObjectValue(v.Data)
    member __.SetValue(x, y) = data.[x] <- y
    member __.GetValue x = if data.ContainsKey x then data.[x] else null
    member __.Data = data |> Seq.map (|KeyValue|) |> Seq.toList
    member this.ToJToken() = 
        let rec getObj (value:ObjectValue) =
            let obj = JObject()
            value.Data 
            |> Seq.iter (fun (k,v) -> 
                if v.GetType() = typeof<ObjectValue> then
                    obj.[k] <- getObj (v :?> ObjectValue)
                else 
                    obj.[k] <- JToken.FromObject(v, Serialization.serializer)
            )
            obj
        this |> getObj
        

type SimpleValue(value) =
    
    new(json, schema, existingTypes) =
        let schema = schema |> Serialization.deserialize<Schema>
        let v = json |> Newtonsoft.Json.Linq.JToken.Parse |> parseForSchema ObjectValue existingTypes schema
        SimpleValue(v)
    member __.ToJToken() = JToken.FromObject(value, Serialization.serializer)
    member __.Value = value