module OpenAPITypeProvider.Types.Response

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Types

let createType ctx findOrCreateSchemaFn name (response:Response) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    response.Content 
    |> Map.map (fun _ media -> media)
    |> Map.map (MediaType.createResponseType ctx findOrCreateSchemaFn)
    |> Map.iter (fun name t ->
        t |> typ.AddMember
        ProvidedProperty(name, t, (fun _ -> <@@ obj() @@>)) |> typ.AddMember
    )

    typ