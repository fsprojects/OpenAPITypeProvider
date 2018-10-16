module internal OpenAPITypeProvider.Types.Document

open ProviderImplementation.ProvidedTypes
open OpenAPIParser.Version3
open OpenAPIParser.Version3.Specification
open OpenAPITypeProvider.Types.Helpers
open System
open System.Collections.Concurrent
open OpenAPITypeProvider

let allSchemas = new ConcurrentDictionary<Schema,SchemaType>()

let rec private uniqueName (name:string) =
    match allSchemas.Values |> Seq.exists (fun v -> v.Name = name) with
    | true -> 
        let nameParts = name.Split([|'_'|], StringSplitOptions.RemoveEmptyEntries)
        let newName = 
            match nameParts |> Array.tryLast with
            | None -> name + "_1"
            | Some v -> 
                match v |> Int32.TryParse with
                | true, i -> 
                    nameParts 
                    |> Array.filter (fun x -> x <> v)
                    |> Array.fold (+) ""
                    |> fun x -> sprintf "%s_%i" x (i + 1) 
                | false, _ -> name + "_1"
        newName |> uniqueName
    | false -> name

let private tryFindByRef (ref:string) =
    allSchemas 
    |> Seq.tryFind (fun kvp -> kvp.Value.Ref = Some ref)
    |> Option.map (fun x -> x.Value)

let createType ctx typeName (filePath:string) =
    
    allSchemas.Clear()// <- Map.empty

    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, typeName, None, hideObjectMethods = true, nonNullable = true, isErased = true)
    let api = filePath |> Parser.Document.loadFromYamlFile

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
    
    // Responses
    let responses = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, "Responses", None, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    // Request bodies
    let requestBodies = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, "RequestBodies", None, hideObjectMethods = true, nonNullable = true, isErased = true)

    let createSchema findOrCreateSchema name (schema:Schema) =
        let def = schema |> Extract.getSchemaDefinition
        match def with
        | Object _ -> Schema.Object.createTypes ctx findOrCreateSchema name schema
        | String (StringFormat.Enum values) ->
            let enumType = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, None)
            enumType.SetEnumUnderlyingType(typeof<string>)
            values |> List.iter (fun x ->
                let n = x |> Names.pascalName
                enumType.AddMember(ProvidedField.Literal(n, enumType, x))
            )
            enumType
        | _ -> Schema.NonObject.createTypes ctx findOrCreateSchema name schema

    
    let toLocalRef (name:string) = sprintf "#/components/schemas/%s" name
    let getRefParts (ref:string) = 
       match ref.Split([|'#'|], StringSplitOptions.RemoveEmptyEntries) with
       | [|remote;local|] -> Some remote, ("#" + local)
       | [|local|] -> None, ("#" + local)
       | _ -> failwithf "%s contains more than one '#' character" ref
    let getNameFromLocalRef (ref:string) = 
        ref.Replace("#","/").Split([|'/'|], StringSplitOptions.RemoveEmptyEntries) |> Array.last
    

    let rec findOrCreateSchema isRoot name (schema:Schema) =
        match isRoot with
        // always create root items
        | true ->
            let refValue = name |> toLocalRef
            match refValue |> tryFindByRef with
            | Some t -> t
            | None ->
                let newType = createSchema (findOrCreateSchema false) name schema
                newType |> schemas.AddMember
                let schemaType = { Name = name; Type = newType; Ref = Some refValue }
                allSchemas.AddOrUpdate (schema, schemaType, (fun _ _ -> schemaType)) |> ignore
                schemaType
        | false ->
            match schema with
            | Schema.Inline _ ->
                let name = name |> uniqueName
                let newType = createSchema (findOrCreateSchema false) name schema
                newType |> schemas.AddMember
                let schemaType = { Name = name; Type = newType; Ref = None }
                allSchemas.AddOrUpdate (schema, schemaType, (fun _ _ -> schemaType)) |> ignore
                schemaType
            | Schema.Reference (ref, _) ->
                match ref |> getRefParts with
                | None, local ->
                    match local |> tryFindByRef with
                    | Some t -> t
                    | None ->
                        let name = local |> getNameFromLocalRef 
                        let newType = createSchema (findOrCreateSchema false) name schema
                        newType |> schemas.AddMember
                        let schemaType = { Name = name; Type = newType; Ref = Some local }
                        allSchemas.AddOrUpdate (schema, schemaType, (fun _ _ -> schemaType)) |> ignore
                        schemaType
                | Some _, _ ->
                    let name = name |> uniqueName
                    let newType = createSchema (findOrCreateSchema false) name schema
                    newType |> schemas.AddMember
                    let schemaType = { Name = name; Type = newType; Ref = None }
                    allSchemas.AddOrUpdate (schema, schemaType, (fun _ _ -> schemaType)) |> ignore
                    schemaType
    
    // components object
    if api.Components.IsSome then
        
            // Add object root types
            api.Components.Value.Schemas
            |> Map.map (findOrCreateSchema true)
            |> ignore
    
            // add responses
            if api.Components.Value.Responses.Count > 0 then
                api.Components.Value.Responses
                |> Map.iter (fun n p -> 
                    let resp = Response.createType ctx (findOrCreateSchema false) n p
                    resp |> responses.AddMember
                    ProvidedProperty(n, resp, (fun _ -> <@@ obj() @@>), isStatic = true) 
                    |?> Some p.Description
                    |> responses.AddMember
                )

                responses |> typ.AddMember
                ProvidedProperty("Responses", responses, isStatic = true) |> typ.AddMember
    
            // add request bodies
            if api.Components.Value.RequestBodies.Count > 0 then
                api.Components.Value.RequestBodies
                |> Map.iter (fun n p -> 
                    let resp = RequestBody.createType ctx (findOrCreateSchema false) n p
                    resp |> requestBodies.AddMember
                    ProvidedProperty(n, resp, (fun _ -> <@@ obj() @@>), isStatic = true) 
                    |?> p.Description
                    |> requestBodies.AddMember
                )
                requestBodies |> typ.AddMember
                ProvidedProperty("RequestBodies", requestBodies, isStatic = true) |> typ.AddMember


    // paths
    let paths = api.Paths |> Path.createTypes ctx (findOrCreateSchema false)
    paths |> typ.AddMember
    ProvidedProperty("Paths", paths, fun _ -> <@@ obj() @@>) |> typ.AddMember
            
    // add schemas as last
    schemas |> typ.AddMember
    ProvidedProperty("Schemas", schemas, isStatic = true) |> typ.AddMember
            
    typ