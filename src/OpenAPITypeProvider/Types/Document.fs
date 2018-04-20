module OpenAPITypeProvider.Types.Document

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification
open System

let mutable allSchemas : Map<Schema,string * ProvidedTypeDefinition> = Map.empty

let rec private uniqueName (name:string) =
    match allSchemas |> Map.exists (fun _ v -> v |> fst = name) with
    | true -> 
        let nameParts = name.Split([|'_'|], StringSplitOptions.RemoveEmptyEntries)
        let newName = 
            match nameParts |> Array.tryLast with
            | None -> name + "_1"
            | Some v -> 
                match v |> Int32.TryParse with
                | true, i -> i + 1 |> sprintf "%s_%i" name
                | false, _ -> name + "_1"
        newName |> uniqueName
    | false -> name

let createType ctx typeName (filePath:string) =
    
    allSchemas <- Map.empty

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
        | Some t -> snd t
        | None ->
            let newType =
                match schema with
                | Object _ -> Schema.Object.createTypes ctx findOrCreateSchema name schema
                | _ -> Schema.NonObject.createTypes ctx findOrCreateSchema name schema
            newType |> schemas.AddMember
            let n = uniqueName name
            allSchemas <- allSchemas.Add(schema, (n, newType))
            n, newType // must return unique name and type

    // components object
    if api.Components.IsSome then
        
            // Add object root types
            api.Components.Value.Schemas 
            |> Map.map (findOrCreateSchema)
            |> ignore
            
    // paths
    let paths = api.Paths |> Path.createTypes ctx (findOrCreateSchema)
    paths |> typ.AddMember
    ProvidedProperty("Paths", paths, fun _ -> <@@ obj() @@>) |> typ.AddMember

    // add schemas as last
    schemas |> typ.AddMember
    ProvidedProperty("Schemas", schemas, isStatic = true) |> typ.AddMember
            
    typ