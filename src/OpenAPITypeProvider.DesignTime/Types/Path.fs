module internal OpenAPITypeProvider.Types.Path

open ProviderImplementation.ProvidedTypes
open OpenAPIParser.Version3.Specification
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Types.Helpers

let private createType ctx findOrCreateSchemaFn name (path:Path) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
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
        let p = path |> Operation.createType ctx findOrCreateSchemaFn name
        p |> typ.AddMember
        ProvidedProperty(name, p, (fun _ -> <@@ obj() @@>))
        |?> path.Description
        |> typ.AddMember
    )
    typ

let createTypes ctx findOrCreateSchemaFn (paths:Map<string, Path>) =
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, "Paths", Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    paths |> Map.iter (fun n p -> 
        let path = createType ctx findOrCreateSchemaFn n p
        path |> typ.AddMember
        ProvidedProperty(n, path, fun _ -> <@@ obj() @@>) 
        |?> p.Description
        |> typ.AddMember
    )
    typ