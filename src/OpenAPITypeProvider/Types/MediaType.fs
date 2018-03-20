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
    
    let found,schemaTyp = findOrCreateSchemaFn name media.Schema
    let strSchema = media.Schema |> Serialization.serialize

    // static method Parse
    ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>)], schemaTyp, (fun args -> 
        <@@ 
            let json = %%args.[1] : string
            ObjectValue.Parse(json,strSchema)
        @@>))
        |> typ.AddMember

    typ
