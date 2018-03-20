module OpenAPITypeProvider.Types.Operation

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations


let createType ctx name (operation:Operation) =
    
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
    let pars = operation.Parameters |> Parameter.createTypes ctx 
    pars |> typ.AddMember
    ProvidedProperty("Parameters", pars, fun _ -> <@@ obj() @@>) |> typ.AddMember
    
    typ
