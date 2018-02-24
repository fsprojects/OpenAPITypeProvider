module OpenAPITypeProvider.Types.Document

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types.Helpers
open OpenAPITypeProvider.Parser

let createType asm ns typeName (filePath:string) =
    let docType = ProvidedTypeDefinition(asm, ns, typeName, None, hideObjectMethods = true, nonNullable = true)
    
    let api = filePath |> Document.loadFromYamlFile

    // constructor
    docType |> addEmptyConstructor "Creates new OpenAPI Type Provider"

    // version    
    let versionValue = api.SpecificationVersion
    makeProperty<string> (fun _ -> <@@ versionValue @@>) "Version"
    |>! addXmlDocDelayed "Open API Version"
    |> docType.AddMember
    
    // info object
    Info.createType asm ns api.Info |> addAsProperty "Info" "Info object" docType

    docType