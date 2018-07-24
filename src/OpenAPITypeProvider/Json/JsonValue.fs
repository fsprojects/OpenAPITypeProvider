namespace OpenAPITypeProvider

open Newtonsoft.Json.Linq
open OpenAPITypeProvider.Json
open OpenAPIParser.Version3.Specification
open Newtonsoft.Json

type ObjectValue(d:(string * obj) list) =
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

    static member Parse(json, strSchema, dateFormat) =
        let schema = strSchema |> Serialization.deserialize<Schema>
        json |> Serialization.parseToJToken dateFormat |> OpenAPITypeProvider.Json.Parser.parseForSchema ObjectValue schema :?> ObjectValue

    member  __.GetValue x = if data.ContainsKey x then data.[x] else null

    /// Converts strongly typed value to JSON string
    member this.ToJson((settings:JsonSerializerSettings), (formatting:Formatting)) = 
        let jToken = this.ToJToken(settings.NullValueHandling)
        JsonConvert.SerializeObject(jToken, formatting, settings)

    /// Converts strongly typed value to JSON string        
    member this.ToJson formatting = 
        let settings = Serialization.getDefaultSettings()
        this.ToJson(settings, formatting)

    /// Converts strongly typed value to JSON string
    member this.ToJson () = this.ToJson(Newtonsoft.Json.Formatting.None)
    
    /// Converts strongly typed value to Newtonsoft JToken
    member __.ToJToken (nullValueHandling:NullValueHandling) =
        let obj = JObject()
        let setEmpty (o:JObject) key =
            match nullValueHandling with
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
                | v -> obj.[k] <- v |> toJToken nullValueHandling
            else if v.GetType() = typeof<Option<List<obj>>> then
                match v |> Reflection.getOptionValue with
                | null -> setEmpty obj k
                | v -> obj.[k] <- v |> arrayToJToken nullValueHandling
            else if v.GetType() = typeof<List<obj>> then
                obj.[k] <- v |> arrayToJToken nullValueHandling
            else 
                obj.[k] <- v |> toJToken nullValueHandling
        )
        obj :> JToken

    /// Converts strongly typed value to Newtonsoft JToken    
    member this.ToJToken () = this.ToJToken(NullValueHandling.Ignore)

type SimpleValue(value:obj) =
    static member Parse(json, strSchema, dateFormat) =
        let schema = strSchema |> Serialization.deserialize<Schema>
        let v = json |> Serialization.parseToJToken dateFormat |> OpenAPITypeProvider.Json.Parser.parseForSchema  ObjectValue schema
        SimpleValue(v)

    /// Converts strongly typed value to Newtonsoft JToken    
    member __.ToJToken () = 
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
    
    /// Converts strongly typed value to JSON string
    member this.ToJson((settings:JsonSerializerSettings), (formatting:Formatting)) = 
        let jToken = this.ToJToken()
        JsonConvert.SerializeObject(jToken, formatting, settings)

    /// Converts strongly typed value to JSON string
    member this.ToJson formatting = 
        let settings = Serialization.getDefaultSettings()
        this.ToJson(settings, formatting)

    /// Converts strongly typed value to JSON string
    member this.ToJson () = this.ToJson(Newtonsoft.Json.Formatting.None)
    member __.GetValue = value
        