namespace OpenAPITypeProvider

open Newtonsoft.Json.Linq
open OpenAPITypeProvider.Json
open OpenAPIParser.Version3.Specification
open Newtonsoft.Json


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

type ObjectValue(d:(string * obj) list, dateFormat) =
    let mutable data = d |> Map |> System.Collections.Generic.Dictionary

    let toJToken nullHandling (o:obj) =
        if o.GetType() = typeof<ObjectValue> then
            (o :?> ObjectValue).ToJToken(nullHandling)
        else
            JToken.FromObject(o, Serialization.getSerializer())

    let arrayToJToken nullHandling (o:obj) =
        let values = o :?> List<_>
        let arr = JArray()
        values |> List.iter (toJToken nullHandling >> arr.Add)
        arr :> JToken

    static member Parse(json, schema, dateFormat) =
        let schema = schema |> Serialization.deserialize<Schema>
        json |> Serialization.parseToJToken dateFormat |> OpenAPITypeProvider.Json.Parser.parseForSchema (fun x -> ObjectValue(x, dateFormat)) schema :?> ObjectValue

    member  __.GetValue x = if data.ContainsKey x then data.[x] else null

    member this.ToJson((settings:JsonSerializerSettings), (formatting:Formatting)) = 
        let jToken = this.ToJToken(settings.NullValueHandling)
        JsonConvert.SerializeObject(jToken, formatting, settings)
        
    member this.ToJson formatting = 
        let settings = Serialization.getDefaultSettings()
        settings.DateFormatString <- dateFormat
        this.ToJson(settings, formatting)

    member this.ToJson () = this.ToJson(Newtonsoft.Json.Formatting.None)
    member this.ToJToken () = this.ToJToken(NullValueHandling.Ignore)
    member __.ToJToken (nullHandling:NullValueHandling) =
        let obj = JObject()
        let setEmpty (o:JObject) key =
            match nullHandling with
            | NullValueHandling.Include -> o.[key] <- JValue.CreateNull()
            | _ -> ()
        data
        |> Seq.map (|KeyValue|)
        |> Seq.toList
        |> List.iter (fun (k,v) -> 
            if v |> isNull then setEmpty obj k
            else if v |> Reflection.isOption<ObjectValue> then
                match v |> Reflection.getOptionValue with
                | null -> setEmpty obj k
                | v -> obj.[k] <- v |> toJToken nullHandling
            else if v.GetType() = typeof<Option<List<obj>>> then
                match v |> Reflection.getOptionValue with
                | null -> setEmpty obj k
                | v -> obj.[k] <- v |> arrayToJToken nullHandling
            else if v.GetType() = typeof<List<obj>> then
                obj.[k] <- v |> arrayToJToken nullHandling
            else 
                obj.[k] <- v |> toJToken nullHandling
        )
        obj :> JToken

type SimpleValue(value:obj) =
    static member Parse(json, strSchema, dateFormat) =
        let schema = strSchema |> Serialization.deserialize<Schema>
        let v = json |> Serialization.parseToJToken dateFormat |> OpenAPITypeProvider.Json.Parser.parseForSchema (fun x -> ObjectValue(x, dateFormat)) schema
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
    member internal __.Value = 
        if value.GetType() = typeof<List<ObjectValue>> then
            let values = value :?> List<ObjectValue>
            values |> List.map box |> box
        else
            value