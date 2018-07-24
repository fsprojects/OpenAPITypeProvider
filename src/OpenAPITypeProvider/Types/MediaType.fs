module internal OpenAPITypeProvider.Types.MediaType

open OpenAPITypeProvider
open ProviderImplementation.ProvidedTypes
open OpenAPIParser.Version3.Specification
open OpenAPITypeProvider.Json
open Newtonsoft.Json

let createRequestType ctx findOrCreateSchemaFn name (media:MediaType) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    let schemaType = findOrCreateSchemaFn name media.Schema
    let strSchema = media.Schema |> Serialization.serialize

    // Parse
    ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>)], schemaType.Type, (fun args -> 
        let format = Json.Parser.defaultDateFormat
        match media.Schema with
        | Object _ ->
            <@@ 
                let json = %%args.[1] : string
                ObjectValue.Parse(json,strSchema, format)
            @@>
        | _ -> 
            <@@ 
                let json = %%args.[1] : string
                SimpleValue.Parse(json,strSchema, format)
            @@>
        ))
        |> (fun x -> x.AddXmlDoc "Parses JSON string to strongly typed request"; x)
        |> typ.AddMember
    
    // Parse
    ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>);ProvidedParameter("dateFormatString", typeof<string>)], schemaType.Type, (fun args -> 
        match media.Schema with
        | Object _ ->
            <@@ 
                let json = %%args.[1] : string
                let format = %%args.[2] : string
                ObjectValue.Parse(json,strSchema, format)
            @@>
        | _ -> 
            <@@ 
                let json = %%args.[1] : string
                let format = %%args.[2] : string
                SimpleValue.Parse(json,strSchema, format)
            @@>
        ))
        |> (fun x -> x.AddXmlDoc "Parses JSON string to strongly typed request with custom DateFormatString"; x)
        |> typ.AddMember
    typ

let createResponseType ctx findOrCreateSchemaFn name (media:MediaType) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    let schemaTyp = findOrCreateSchemaFn name media.Schema

    // ToJson
    ProvidedMethod("ToJson", [ProvidedParameter(name, schemaTyp.Type)], typeof<string>, (fun args -> 
        match media.Schema with
        | Object _ ->
            <@@ 
                let o = %%args.[1] : ObjectValue
                o.ToJson()
            @@>
        | _ -> 
            <@@ 
                let o = %%args.[1] : SimpleValue
                o.ToJson()
            @@>
        ))
        |> (fun x -> x.AddXmlDoc "Converts strongly typed value to JSON string"; x)
        |> typ.AddMember
    
    // ToJson
    ProvidedMethod("ToJson", [ProvidedParameter(name, schemaTyp.Type);ProvidedParameter("formatting", typeof<Formatting>)], typeof<string>, (fun args -> 
        match media.Schema with
        | Object _ ->
            <@@ 
                let o = %%args.[1] : ObjectValue
                let f = %%args.[2] : Formatting
                o.ToJson(f)
            @@>
        | _ -> 
            <@@ 
                let o = %%args.[1] : SimpleValue
                let f = %%args.[2] : Formatting
                o.ToJson(f)
            @@>
        ))
        |> (fun x -> x.AddXmlDoc "Converts strongly typed value to JSON string"; x)
        |> typ.AddMember
    
    // ToJson
    ProvidedMethod("ToJson", [ProvidedParameter(name, schemaTyp.Type);ProvidedParameter("settings", typeof<JsonSerializerSettings>);ProvidedParameter("formatting", typeof<Formatting>)], typeof<string>, (fun args -> 
        match media.Schema with
        | Object _ ->
            <@@ 
                let o = %%args.[1] : ObjectValue
                let s = %%args.[2] : JsonSerializerSettings
                let f = %%args.[3] : Formatting
                o.ToJson(s,f)
            @@>
        | _ -> 
            <@@ 
                let o = %%args.[1] : SimpleValue
                let s = %%args.[2] : JsonSerializerSettings
                let f = %%args.[3] : Formatting
                o.ToJson(s,f)
            @@>
        ))
        |> (fun x -> x.AddXmlDoc "Converts strongly typed value to JSON string"; x)
        |> typ.AddMember
    
    // ToJToken
    ProvidedMethod("ToJToken", [ProvidedParameter(name, schemaTyp.Type)], typeof<Linq.JToken>, (fun args -> 
        match media.Schema with
        | Object _ ->
            <@@ 
                let o = %%args.[1] : ObjectValue
                o.ToJToken()
            @@>
        | _ -> 
            <@@ 
                let o = %%args.[1] : SimpleValue
                o.ToJToken()
            @@>
        ))
        |> (fun x -> x.AddXmlDoc "Converts strongly typed value to Newtonsoft JToken"; x)
        |> typ.AddMember
    
    match media.Schema with
    | Object _ ->
        // ToJToken
        ProvidedMethod("ToJToken", [ProvidedParameter(name, schemaTyp.Type);ProvidedParameter("nullValueHandling", typeof<NullValueHandling>)], typeof<Linq.JToken>, (fun args -> 
                <@@ 
                    let o = %%args.[1] : ObjectValue
                    let h = %%args.[2] : NullValueHandling
                    o.ToJToken(h)
                @@>
            ))
            |> (fun x -> x.AddXmlDoc "Converts strongly typed value to Newtonsoft JToken"; x)
            |> typ.AddMember
    | _ -> ()
    
    typ