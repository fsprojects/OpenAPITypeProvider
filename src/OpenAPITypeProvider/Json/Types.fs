namespace OpenAPITypeProvider.Json.Types

open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Parser
open OpenAPITypeProvider.Specification
open Newtonsoft.Json.Linq

type ObjectValue(d:(string * obj) list) =
    let mutable data = d |> Map |> System.Collections.Generic.Dictionary
    new(json, schema) =
        let schema = schema |> Serialization.deserialize<Schema>
        let v = json |> Newtonsoft.Json.Linq.JToken.Parse |> parseForSchema ObjectValue typeof<ObjectValue> schema :?> ObjectValue
        ObjectValue(v.Data)
    member __.SetValue(x, y) = data.[x] <- y
    member __.GetValue x = if data.ContainsKey x then data.[x] else null
    member __.Data = data |> Seq.map (|KeyValue|) |> Seq.toList
    member this.ToJToken() = 
        let rec getObj (value:ObjectValue) =
            let obj = JObject()
            value.Data 
            |> Seq.iter (fun (k,v) -> 
                if v = null then
                    ()
                else if v.GetType() = typeof<ObjectValue> then
                    obj.[k] <- getObj (v :?> ObjectValue)
                else 
                    obj.[k] <- JToken.FromObject(v, Serialization.serializer)
            )
            obj
        this |> getObj
        

type SimpleValue(value) =
    new(json, strSchema) =
        let schema = strSchema |> Serialization.deserialize<Schema>
        let v = json |> Newtonsoft.Json.Linq.JToken.Parse |> parseForSchema ObjectValue typeof<ObjectValue> schema
        SimpleValue(v)
    member __.ToJToken() = JToken.FromObject(value, Serialization.serializer)
    member __.Value = value
        