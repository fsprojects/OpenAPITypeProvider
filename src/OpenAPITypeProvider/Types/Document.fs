module OpenAPITypeProvider.Types.Document

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider.Parser

let createType asm ns typeName (filePath:string) =
    let typ = ProvidedTypeDefinition(asm, ns, typeName, None, hideObjectMethods = true, nonNullable = true)
    
    let api = filePath |> Document.loadFromYamlFile

    // ctor
    ProvidedConstructor([], fun _ -> <@@ () @@>) |> typ.AddMember

    // version    
    let version = api.SpecificationVersion
    ProvidedProperty("Version", typeof<string>, (fun _ -> <@@ version @@>)) |> typ.AddMember

    // info object
    let info = Info.createType asm ns api.Info
    info |> typ.AddMember
    ProvidedProperty("Info", info, (fun _ -> <@@ obj() @@>)) |> typ.AddMember
    
    // components object
    if api.Components.IsSome then
        let components = Components.createType asm ns api.Components.Value
        components |> typ.AddMember
        ProvidedProperty("Components", components, (fun _ -> <@@ obj() @@>)) |> typ.AddMember

    typ