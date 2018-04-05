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
    let allSchemas = 
        if api.Components.IsSome then
        
            // Schemas
            let schemas = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, "Schemas", None, hideObjectMethods = true, nonNullable = true, isErased = true)
        
            // Add object root types
            let createdSchemas = 
                api.Components.Value.Schemas
                |> Map.map (Schema.Object.createTypes ctx)
                |> Map.toList
                |> List.collect snd
        
            createdSchemas |> List.map snd |> List.iter schemas.AddMember

            // Add non-object root types
            let createdNonObjSchemas = 
                api.Components.Value.Schemas
                |> Map.map (Schema.NonObject.createTypes ctx createdSchemas)
                |> Map.toList
                |> List.collect snd
        
            createdNonObjSchemas |> List.map snd |> List.iter schemas.AddMember
            schemas |> typ.AddMember
            ProvidedProperty("Schemas", schemas, isStatic = true) |> typ.AddMember
            
            createdSchemas @ createdNonObjSchemas
        else []
       
    let findOrCreateSchema name schema =
        match allSchemas |> List.tryFind (fun (s,t) -> s = schema) with
        | Some found -> (true, found |> snd)
        | None ->
            let objects = Schema.Object.createTypes ctx name schema
            let others = Schema.NonObject.createTypes ctx allSchemas name schema
            objects @ others |> List.head |> snd |> (fun x -> (false, x))

    // paths
    let paths = api.Paths |> Path.createTypes ctx findOrCreateSchema
    paths |> typ.AddMember
    ProvidedProperty("Paths", paths, fun _ -> <@@ obj() @@>) |> typ.AddMember

    typ