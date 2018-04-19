module OpenAPITypeProvider.Types.Document

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification

let mutable allSchemas : Map<Schema,ProvidedTypeDefinition> = Map.empty

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
    
    // Schemas
    let schemas = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, "Schemas", None, hideObjectMethods = true, nonNullable = true, isErased = true)

    let rec findOrCreateSchema name (schema:Schema) =
        match allSchemas |> Map.tryFind schema with
        | Some t -> t
        | None ->
            let newType =
                match schema with
                | Object _ -> 
                    printf "Creating OBJECT %A %s \n" schema name
                    Schema.Object.createTypes ctx (findOrCreateSchema) name schema
                | _ -> 
                    printf "Creating OTHER %A %s \n" schema name
                    Schema.NonObject.createTypes ctx (findOrCreateSchema) name schema
            newType |> schemas.AddMember
            allSchemas <- allSchemas.Add(schema, newType)

            newType

    // components object
    if api.Components.IsSome then
        
            // Add object root types
            api.Components.Value.Schemas 
            |> Map.map (findOrCreateSchema)
            |> Map.iter (fun k v -> schemas.AddMember(v))
            
    // paths
    let paths = api.Paths |> Path.createTypes ctx (findOrCreateSchema)
    paths |> typ.AddMember
    ProvidedProperty("Paths", paths, fun _ -> <@@ obj() @@>) |> typ.AddMember

    // add schemas as last
    printf "ADDING SCHEMAS"
    schemas |> typ.AddMember
    ProvidedProperty("Schemas", schemas, isStatic = true) |> typ.AddMember
            
    typ