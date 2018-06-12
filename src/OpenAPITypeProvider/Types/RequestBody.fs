module OpenAPITypeProvider.Types.RequestBody

open ProviderImplementation.ProvidedTypes
open OpenAPIParser.Version3.Specification
open OpenAPITypeProvider.Types

let createType ctx findOrCreateSchemaFn name (par:RequestBody) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    par.Content 
    |> Map.map (fun _ media -> media)
    |> Map.map (MediaType.createRequestType ctx findOrCreateSchemaFn)
    |> Map.iter (fun name t ->
        t |> typ.AddMember
        ProvidedProperty(name, t, (fun _ -> <@@ obj() @@>)) |> typ.AddMember
    )

    typ