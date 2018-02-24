module OpenAPITypeProvider.Types.Document

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types.Helpers
open OpenAPITypeProvider.Parser

let createType asm ns typeName (filePath:string) =
    let docType = ProvidedTypeDefinition(asm, ns, typeName, None, hideObjectMethods = true, nonNullable = true)
    
    let api = filePath |> Document.loadFromYamlFile
    let version = api.SpecificationVersion

    // constructor
    let ctor =
        makeConstructor List.empty (fun _ -> <@@ () @@>)
        |>! addXmlDocDelayed "Creates new OpenAPI Type Provider"
    docType.AddMember(ctor)
    
    let vProp = 
        makeProperty<string> (fun _ -> <@@ version @@>) "Version"
        |>! addXmlDocDelayed "Open API Version"
    docType.AddMember(vProp)


    docType