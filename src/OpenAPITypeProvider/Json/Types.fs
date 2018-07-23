namespace OpenAPITypeProvider.Json.Types

open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Parser
open OpenAPIParser.Version3.Specification
open Newtonsoft.Json.Linq

module private Reflection =
    let getOptionValue (o:obj) =
        match o with
        | null -> null
        | v ->
            match v.GetType().GetProperty("Value") with
            | null -> null
            | prop -> prop.GetValue(o, null )
    
    let isOption<'a> (o:obj) =
        match o |> getOptionValue with
        | null -> false
        | v -> v.GetType() = typeof<'a>

type ObjectValue(d:(string * obj) list) =
    let mutable data = d |> Map |> System.Collections.Generic.Dictionary
    
    let toJToken (o:obj) =
        if o.GetType() = typeof<ObjectValue> then
            (o :?> ObjectValue).ToJToken()
        else
            JToken.FromObject(o, Serialization.getSerializer())

    let arrayToJToken (o:obj) =
        let values = o :?> List<_>
        let arr = JArray()
        values |> List.iter (toJToken >> arr.Add)
        arr :> JToken

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
            if v |> isNull then
                obj.[k] <- JValue.CreateNull()
            else if v |> Reflection.isOption<ObjectValue> then
                match v |> Reflection.getOptionValue with
                | null -> obj.[k] <- JValue.CreateNull()
                | v -> obj.[k] <- v |> toJToken
            else if v.GetType() = typeof<Option<List<obj>>> then
                match v |> Reflection.getOptionValue with
                | null -> obj.[k] <- JValue.CreateNull()
                | v -> obj.[k] <- v |> arrayToJToken
            else if v.GetType() = typeof<List<obj>> then
                obj.[k] <- v |> arrayToJToken
            else 
                obj.[k] <- v |> toJToken
        )
        obj :> JToken

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