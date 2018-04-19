module OpenAPITypeProvider.Types.MediaType

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations

let createRequestType ctx findOrCreateSchemaFn name (media:MediaType) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, (name + "REQ"), Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    let schemaTyp = findOrCreateSchemaFn name media.Schema
    let strSchema = media.Schema |> Serialization.serialize

    // Parse
    ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>)], schemaTyp, (fun args -> 
        match media.Schema with
        | Object _ ->
            <@@ 
                let json = %%args.[1] : string
                ObjectValue.Parse(json,strSchema)
            @@>
        | _ -> 
            <@@ 
                let json = %%args.[1] : string
                SimpleValue.Parse(json,strSchema)
            @@>
        ))
        |> (fun x -> x.AddXmlDoc "Parses JSON string to strongly typed request"; x)
        |> typ.AddMember
    typ

let createResponseType ctx findOrCreateSchemaFn name (media:MediaType) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, (name + "RES"), Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    let schemaTyp = findOrCreateSchemaFn name media.Schema

    // ToJToken
    ProvidedMethod("ToJToken", [ProvidedParameter(name, schemaTyp)], typeof<Newtonsoft.Json.Linq.JToken>, (fun args -> 
        match media.Schema with
        | Object _ ->
            <@@ 
                let o = %%Expr.Coerce(args.[1], typeof<ObjectValue>) : ObjectValue
                o.ToJToken()
            @@>
        | _ -> 
            <@@ 
                let o = %%Expr.Coerce(args.[1], typeof<SimpleValue>) : SimpleValue
                o.ToJToken()
            @@>
        ))
        |> (fun x -> x.AddXmlDoc "Converts strongly typed response to JToken"; x)
        |> typ.AddMember
    typ