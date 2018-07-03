module OpenAPITypeProvider.Types.Response

open ProviderImplementation.ProvidedTypes
open OpenAPIParser.Version3.Specification
open OpenAPITypeProvider.Types

let createType ctx findOrCreateSchemaFn name (response:Response) =
    let combinedName (n:string) = sprintf "%s as %s" name n
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    response.Content 
    |> Map.map (fun n m -> MediaType.createResponseType ctx findOrCreateSchemaFn (combinedName n) m)
    |> Map.iter (fun n t ->
        t |> typ.AddMember
        ProvidedProperty(n, t, (fun _ -> <@@ obj() @@>)) |> typ.AddMember
    )

    typ