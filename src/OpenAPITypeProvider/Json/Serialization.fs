[<RequireQualifiedAccess>]
module internal OpenAPITypeProvider.Json.Serialization

open System
open Newtonsoft.Json
open Microsoft.FSharp.Reflection
open Newtonsoft.Json.Linq

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

let settings = JsonSerializerSettings()
settings.NullValueHandling <- NullValueHandling.Ignore
settings.Converters.Add(OptionConverter())

let getSerializer() = JsonSerializer.Create(settings)
let serialize obj = JsonConvert.SerializeObject(obj, settings)
let deserialize<'a> json = JsonConvert.DeserializeObject<'a>(json, settings)

let toJToken dateFormat json =
    settings.DateFormatString <- dateFormat
    JsonConvert.DeserializeObject<JToken>(json, settings)