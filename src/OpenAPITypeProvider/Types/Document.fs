module OpenAPITypeProvider.Types.Document

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider.Parser

type MyType() =
    member __.Value = "AHOJ"

let createType asm ns typeName (filePath:string) =
    let typ = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<MyType>, hideObjectMethods = true, nonNullable = true)
    
    let api = filePath |> Document.loadFromYamlFile

    // ctor
    ProvidedConstructor([], fun _ -> <@@ obj() @@>) |> typ.AddMember

    // version    
    let version = api.SpecificationVersion
    ProvidedProperty("Version", typeof<string>, (fun _ -> <@@ version @@>)) |> typ.AddMember

    // info object
    let info = Info.createType asm ns api.Info
    info |> typ.AddMember
    ProvidedProperty("Info", info, fun _ -> <@@ obj() @@>) |> typ.AddMember
    
    // components object
    if api.Components.IsSome then
        
        // Schemas
        let schemas = ProvidedTypeDefinition(asm, ns, "Schemas", None, hideObjectMethods = true, nonNullable = true)
        
        api.Components.Value.Schemas
        |> Map.iter (fun name schema -> 
            Schema.createRootNonObjectTypes asm ns schemas name schema
        )
        schemas |> typ.AddMember
        ProvidedProperty("Schemas", schemas, isStatic = true) |> typ.AddMember

    typ