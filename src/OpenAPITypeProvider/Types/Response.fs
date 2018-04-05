module OpenAPITypeProvider.Types.Response

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations


let createType ctx findOrCreateSchemaFn name (response:Response) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    // description
    let desc = response.Description
    ProvidedProperty("Description", typeof<string>, (fun _ -> <@@ desc @@>)) |> typ.AddMember

    response.Content 
    |> Map.map (fun _ media -> media)
    |> Map.map (MediaType.createType ctx findOrCreateSchemaFn)
    |> Map.iter (fun name t ->
        t |> typ.AddMember
        ProvidedProperty(name, t, (fun _ -> <@@ obj() @@>)) |> typ.AddMember
    )

    typ

