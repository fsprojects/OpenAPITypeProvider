module OpenAPITypeProvider.Types.Operation

open ProviderImplementation.ProvidedTypes
open OpenAPIParser.Version3.Specification
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Types.Helpers

let createType ctx findOrCreateSchemaFn name (operation:Operation) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    // RequestBody
    if operation.RequestBody.IsSome then
        let rb = operation.RequestBody.Value |> RequestBody.createType ctx findOrCreateSchemaFn "RequestBody"
        rb |> typ.AddMember
        ProvidedProperty("RequestBody", rb, (fun _ -> <@@ obj() @@>))
        |?> operation.RequestBody.Value.Description
        |> typ.AddMember
    
    // Responses
    let responses = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, "Responses", Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    operation.Responses |> Map.iter (fun n r -> 
        let response = Response.createType ctx findOrCreateSchemaFn n r
        response |> responses.AddMember
        ProvidedProperty(n, response, fun _ -> <@@ obj() @@>) 
        |?> (Some r.Description)
        |> responses.AddMember
    )
    responses |> typ.AddMember
    ProvidedProperty("Responses", responses, fun _ -> <@@ obj() @@>) |> typ.AddMember

    typ