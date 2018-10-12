namespace OpenAPITypeProvider

open System
open Newtonsoft.Json.Linq
open Newtonsoft.Json
open OpenAPIParser.Version3.Specification

module Inference =

    let rec getComplexType (getSchemaFun: Schema -> Type) schema = 
        match schema with
        | Boolean -> typeof<bool>
        | Integer IntFormat.Int32 -> typeof<int>
        | Integer IntFormat.Int64 -> typeof<int64>
        | Number NumberFormat.Float -> typeof<float32>
        | Number NumberFormat.Double -> typeof<double>
        | String StringFormat.String 
        | String StringFormat.Password 
        | String StringFormat.Binary -> typeof<string>
        | String StringFormat.Byte -> typeof<byte>
        | String StringFormat.Date 
        | String StringFormat.DateTime -> typeof<DateTime>
        | String StringFormat.UUID -> typeof<Guid>
        | String (StringFormat.Enum _) -> schema |> getSchemaFun
        | Array schema -> 
            let typ = schema |> getComplexType getSchemaFun
            typedefof<List<_>>.MakeGenericType([|typ|])
        | Object _ -> schema |> getSchemaFun

module Reflection =

    type ReflectiveListBuilder = 
        static member BuildList<'a> (args: obj list) = 
            [ for a in args do yield a :?> 'a ] 
        static member BuildTypedList lType (args: obj list) = 
            typeof<ReflectiveListBuilder>
                .GetMethod("BuildList")
                .MakeGenericMethod([|lType|])
                .Invoke(null, [|args|])

    let some (typ:Type) arg =
        let unionType = typedefof<option<_>>.MakeGenericType typ
        let meth = unionType.GetMethod("Some")
        meth.Invoke(null, [|arg|])

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

module Parser =

    let private checkRequiredProperties (req:string list) (jObject:JObject) =
        let props = jObject.Properties() |> Seq.toList
        let propertyExist name = props |> List.exists (fun x -> x.Name = name && x.Value.Type <> JTokenType.Null)
        req |> List.iter (fun p ->
            if propertyExist p |> not then raise <| FormatException (sprintf "Property '%s' is required by schema definition, but not present in JSON or is null" p)
        )

    let defaultDateFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK"

    let private isOneOfAllowed (allowedValues:string list) (value:string) =
        match allowedValues |> List.tryFind ((=) value) with
        | Some v -> value
        | None -> FormatException (sprintf "Invalid value %s - Enum must contain one of allowed values: %A" value allowedValues) |> raise

    let rec parseForSchema createObj defaultTyp (schema:Schema) (json:JToken) =
        match schema with
        | Boolean -> json.Value<bool>() |> box
        | Integer Int32 -> json.Value<int32>() |> box
        | Integer Int64 -> json.Value<int64>() |> box
        | Number NumberFormat.Double -> json.Value<double>() |> box
        | Number NumberFormat.Float -> json.Value<float32>() |> box
        | String StringFormat.String 
        | String StringFormat.Binary 
        | String StringFormat.Password -> json.Value<string>() |> box
        | String StringFormat.Byte -> json.Value<byte>() |> box
        | String StringFormat.DateTime
        | String StringFormat.Date -> json.Value<DateTime>() |> box
        | String StringFormat.UUID -> json.Value<string>() |> Guid |> box
        | String (StringFormat.Enum values) -> json.Value<string>() |> isOneOfAllowed values |> box
        | Array itemsSchema ->
            let jArray = json :?> JArray
            let items = [ for x in jArray do yield parseForSchema createObj defaultTyp itemsSchema x ]
            let typ = itemsSchema |> Inference.getComplexType (fun _ -> defaultTyp)
            Reflection.ReflectiveListBuilder.BuildTypedList typ items |> box
        | Object (props, required) ->
            let jObject = json :?> JObject
            jObject |> checkRequiredProperties required
            props 
            |> Map.map (fun name schema -> 
                if required |> List.contains name then
                    parseForSchema createObj defaultTyp schema (jObject.[name]) |> Some
                else if jObject.ContainsKey name then
                    let typ = schema |> Inference.getComplexType (fun _ -> defaultTyp)
                    if jObject.[name].Type = JTokenType.Null then None
                    else
                        parseForSchema createObj defaultTyp schema (jObject.[name]) 
                        |> Reflection.some typ
                        |> Some
                else None
            )
            |> Map.map (fun _ v -> defaultArg v null)
            |> Map.toList
            |> createObj
            |> box
    

module internal Serialization =

    open System
    open Newtonsoft.Json
    open Microsoft.FSharp.Reflection

    type OptionConverter() =
        inherit JsonConverter()
    
        override __.CanConvert(t) = 
            t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>

        override __.WriteJson(writer, value, serializer) =
            let value = 
                if isNull value then null
                else 
                    let _,fields = FSharpValue.GetUnionFields(value, value.GetType())
                    fields.[0]  
            serializer.Serialize(writer, value)

        override __.ReadJson(reader, t, _, serializer) =        
            let innerType = t.GetGenericArguments().[0]
            let innerType = 
                if innerType.IsValueType then (typedefof<Nullable<_>>).MakeGenericType([|innerType|])
                else innerType        
            let value = serializer.Deserialize(reader, innerType)
            let cases = FSharpType.GetUnionCases(t)
            if isNull value then FSharpValue.MakeUnion(cases.[0], [||])
            else FSharpValue.MakeUnion(cases.[1], [|value|])


    let getDefaultSettings() =
        let settings = JsonSerializerSettings()
        settings.Converters.Add(OptionConverter())
        settings.NullValueHandling <- NullValueHandling.Ignore
        settings

    let private settings = getDefaultSettings()

    let getSerializer() = JsonSerializer.Create(settings)
    let serialize obj = JsonConvert.SerializeObject(obj, settings)
    let deserialize<'a> json = JsonConvert.DeserializeObject<'a>(json, settings)

    let parseToJToken dateFormat json =
        settings.DateFormatString <- dateFormat
        JsonConvert.DeserializeObject<JToken>(json, settings)

type ObjectValue(d:(string * obj) list) =
    let data = d |> Map |> System.Collections.Generic.Dictionary

    let toJToken nullHandling (o:obj) =
        if o.GetType() = typeof<ObjectValue> then
            (o :?> ObjectValue).ToJToken(nullHandling)
        else
            JToken.FromObject(o, Serialization.getSerializer())

    let arrayToJToken nullHandling (o:obj) =
        let values = o :?> List<obj>
        let arr = JArray()
        values |> List.iter (toJToken nullHandling >> arr.Add)
        arr :> JToken
    
    let arrayObjectValueToJToken nullHandling (o:obj) =
        let values = o :?> List<ObjectValue>
        let arr = JArray()
        values |> List.iter (toJToken nullHandling >> arr.Add)
        arr :> JToken

    static member Parse(json, strSchema, dateFormat) =
        let schema = strSchema |> Serialization.deserialize<Schema>
        json |> Serialization.parseToJToken dateFormat |> Parser.parseForSchema ObjectValue typeof<ObjectValue> schema :?> ObjectValue

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
            else if v.GetType() = typeof<Option<List<ObjectValue>>> then
                match v |> Reflection.getOptionValue with
                | null -> setEmpty obj k
                | v -> obj.[k] <- v |> arrayObjectValueToJToken nullValueHandling
            else if v.GetType() = typeof<List<ObjectValue>> then
                obj.[k] <- v |> arrayObjectValueToJToken nullValueHandling
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
        let v = json |> Serialization.parseToJToken dateFormat |> Parser.parseForSchema ObjectValue typeof<ObjectValue> schema
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


// Put the TypeProviderAssemblyAttribute in the runtime DLL, pointing to the design-time DLL
[<assembly:CompilerServices.TypeProviderAssembly("OpenAPITypeProvider.DesignTime.dll")>]
do ()
