namespace OpenAPITypeProvider.Json.Types

open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Parser
open OpenAPIParser.Version3.Specification
open Newtonsoft.Json.Linq

type ObjectValue(d:(string * obj) list) =
    let mutable data = d |> Map |> System.Collections.Generic.Dictionary
    
    static member Parse(json, schema) =
        let schema = schema |> Serialization.deserialize<Schema>
        let v = json |> Newtonsoft.Json.Linq.JToken.Parse |> parseForSchema ObjectValue typeof<ObjectValue> schema :?> ObjectValue
        ObjectValue(v.GetData())

    member __.SetValue(x, y) = data.[x] <- y
    member __.GetValue x = if data.ContainsKey x then data.[x] else null
    member __.GetData() = data |> Seq.map (|KeyValue|) |> Seq.toList
    member this.ToJToken() = 
        let rec getObj (value:ObjectValue) =
            let obj = JObject()
            value.GetData() 
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

type SimpleValue(value:obj) =
    static member Parse(json, strSchema) =
        let schema = strSchema |> Serialization.deserialize<Schema>
        let v = json |> Newtonsoft.Json.Linq.JToken.Parse |> parseForSchema ObjectValue typeof<ObjectValue> schema
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
            JToken.FromObject(value, Serialization.serializer)
    member __.Value = 
        if value.GetType() = typeof<List<ObjectValue>> then
            let values = value :?> List<ObjectValue>
            values |> List.map (fun x -> box x) |> box
        else
            value