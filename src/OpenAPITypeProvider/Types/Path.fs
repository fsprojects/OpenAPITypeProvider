module OpenAPITypeProvider.Types.Path

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations


let private createType ctx name (path:Path) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    // summary
    if path.Summary.IsSome then
        let summary = path.Summary.Value
        ProvidedProperty("Summary", typeof<string>, (fun _ -> <@@ summary @@>)) |> typ.AddMember
    
    // description
    if path.Description.IsSome then
        let desc = path.Description.Value
        ProvidedProperty("Description", typeof<string>, (fun _ -> <@@ desc @@>)) |> typ.AddMember
    
    let paths = 
        [
            path.Get, "Get"
            path.Put, "Put"
            path.Post, "Post"
            path.Delete, "Delete"
            path.Options, "Options"
            path.Head, "Head"
            path.Patch, "Patch"
            path.Trace, "Trace"
        ] 
        |> List.filter (fun (x,_) -> x.IsSome)
        |> List.map (fun (x,y) -> x.Value, y)
    
    paths |> List.iter (fun (path, name) -> 
        let p = path|> Operation.createType ctx name
        p |> typ.AddMember
        ProvidedProperty(name, p, (fun _ -> <@@ obj() @@>)) |> typ.AddMember
    )

    typ

let createTypes ctx (paths:Map<string, Path>) =
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, "Paths", Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    paths |> Map.iter (fun n p -> 
        let path = createType ctx n p
        path |> typ.AddMember
        ProvidedProperty(n, path, fun _ -> <@@ obj() @@>) |> typ.AddMember
    )

    typ