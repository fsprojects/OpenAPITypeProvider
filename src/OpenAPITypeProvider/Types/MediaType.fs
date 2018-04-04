module OpenAPITypeProvider.Types.MediaType

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations


let createType ctx findOrCreateSchemaFn name (media:MediaType) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    let _,schemaTyp = findOrCreateSchemaFn name media.Schema
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
        |> typ.AddMember
    
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
        |> typ.AddMember
    
    

    typ
