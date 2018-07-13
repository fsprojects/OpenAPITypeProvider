namespace OpenAPITypeProvider.Json.Types

open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Parser
open OpenAPIParser.Version3.Specification
open Newtonsoft.Json.Linq


type ObjectValue(d:(string * obj) list) =
    let mutable data = d |> Map |> System.Collections.Generic.Dictionary
    
    static member Parse(json, schema, dateFormat) =
        let schema = schema |> Serialization.deserialize<Schema>
        let v = json |> Serialization.toJToken dateFormat |> parseForSchema ObjectValue schema :?> ObjectValue
        ObjectValue(v.GetData())

    member __.SetValue(x, y) = data.[x] <- y
    member __.GetValue x = if data.ContainsKey x then data.[x] else null
    member __.GetData() = data |> Seq.map (|KeyValue|) |> Seq.toList
    member this.ToJToken() = 
        let obj = JObject()
        this.GetData() 
        |> Seq.iter (fun (k,v) -> 
            if v = null then
                ()
            else if v.GetType() = typeof<ObjectValue> then
                obj.[k] <- (v :?> ObjectValue).ToJToken()
            else if v.GetType() = typeof<List<obj>> then
                let values = v :?> List<_>
                let arr = JArray()
                values |> List.map (fun (x:obj) -> x :?> ObjectValue) |> List.iter (fun x -> x.ToJToken() |> arr.Add)
                obj.[k] <- arr :> JToken
            else 
                obj.[k] <- JToken.FromObject(v, Serialization.getSerializer())
        )
        obj

type SimpleValue(value:obj) =
    static member Parse(json, strSchema, dateFormat) =
        let schema = strSchema |> Serialization.deserialize<Schema>
        let v = json |> Serialization.toJToken dateFormat |> parseForSchema ObjectValue schema
        SimpleValue(v)
    member __.ToJToken() = 
        let valueType = value.GetType()
        if valueType = typeof<List<ObjectValue>> then
            let values = value :?> List<ObjectValue>
            let arr = JArray()
            values |> List.iter (fun x -> x.ToJToken() |> arr.Add)
            arr :> JToken
        else if valueType = typeof<List<obj>> then
            let values = value :?> List<obj>
            let arr = JArray()
            values |> List.iter (fun x -> (x :?> ObjectValue).ToJToken() |> arr.Add)
            arr :> JToken
        else
            JToken.FromObject(value, Serialization.getSerializer())
    member __.Value = 
        if value.GetType() = typeof<List<ObjectValue>> then
            let values = value :?> List<ObjectValue>
            values |> List.map (fun x -> box x) |> box
        else
            value