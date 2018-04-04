module OpenAPITypeProvider.Types.Operation

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations


let createType ctx findOrCreateSchemaFn name (operation:Operation) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    let tags = operation.Tags
    if tags.Length > 0 then
        ProvidedProperty("Tags", typeof<List<string>>, (fun _ -> <@@ tags @@>)) |> typ.AddMember

    // Summary
    if operation.Summary.IsSome then
        let summary = operation.Summary.Value
        ProvidedProperty("Summary", typeof<string>, (fun _ -> <@@ summary @@>)) |> typ.AddMember
    
    // Description
    if operation.Description.IsSome then
        let desc = operation.Description.Value
        ProvidedProperty("Description", typeof<string>, (fun _ -> <@@ desc @@>)) |> typ.AddMember
    
    // OperationId
    if operation.OperationId.IsSome then
        let opId = operation.OperationId.Value
        ProvidedProperty("OperationId", typeof<string>, (fun _ -> <@@ opId @@>)) |> typ.AddMember
    
    // Parameters
    //let pars = operation.Parameters |> Parameter.createTypes ctx 
    //pars |> typ.AddMember
    //ProvidedProperty("Parameters", pars, fun _ -> <@@ obj() @@>) |> typ.AddMember
    
    // RequestBody
    if operation.RequestBody.IsSome then
        let rb = operation.RequestBody.Value |> RequestBody.createType ctx findOrCreateSchemaFn "RequestBody"
        rb |> typ.AddMember
        ProvidedProperty("RequestBody", rb, (fun _ -> <@@ obj() @@>)) |> typ.AddMember
    
    // Responses
    let responses = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, "Responses", Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    operation.Responses |> Map.iter (fun n r -> 
        let response = Response.createType ctx findOrCreateSchemaFn n r
        response |> responses.AddMember
        ProvidedProperty(n, response, fun _ -> <@@ obj() @@>) |> responses.AddMember
    )
    responses |> typ.AddMember
    ProvidedProperty("Responses", responses, fun _ -> <@@ obj() @@>) |> typ.AddMember

    typ
