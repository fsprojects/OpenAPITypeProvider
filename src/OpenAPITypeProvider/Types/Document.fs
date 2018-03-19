module OpenAPITypeProvider.Types.Document

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification

let createType ctx typeName (filePath:string) =
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, typeName, None, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    let api = filePath |> Document.loadFromYamlFile

    // ctor
    ProvidedConstructor([], fun _ -> <@@ obj() @@>) |> typ.AddMember

    // version    
    let version = api.SpecificationVersion
    ProvidedProperty("Version", typeof<string>, (fun _ -> <@@ version @@>)) |> typ.AddMember

    // info object
    let info = Info.createType ctx api.Info
    info |> typ.AddMember
    ProvidedProperty("Info", info, fun _ -> <@@ obj() @@>) |> typ.AddMember
    
    // components object
    if api.Components.IsSome then
        
        // Schemas
        let schemas = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, "Schemas", None, hideObjectMethods = true, nonNullable = true, isErased = true)
        
        // Add object root types
        let createdSchemas = 
            api.Components.Value.Schemas
            |> Map.filter (fun _ s -> s <> Schema.Empty)
            |> Map.map (Schema.Object.createTypes ctx)
            |> Map.toList
            |> List.collect snd
        
        createdSchemas |> List.map snd |> List.iter schemas.AddMember

        // Add non-object root types
        api.Components.Value.Schemas
        |> Map.filter (fun _ s -> s <> Schema.Empty)
        |> Map.map (Schema.NonObject.tryCreateType ctx)
        |> Map.filter (fun _ v -> v.IsSome)
        |> Map.map (fun _ v -> v.Value)
        |> Map.iter (fun _ v -> schemas.AddMember v)



        schemas |> typ.AddMember
        ProvidedProperty("Schemas", schemas, isStatic = true) |> typ.AddMember

    typ