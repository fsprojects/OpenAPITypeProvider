module OpenAPITypeProvider.Types.Document

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types.Helpers
open OpenAPITypeProvider.Parser

let createType asm ns typeName (filePath:string) =
    let docType = ProvidedTypeDefinition(asm, ns, typeName, None)
    
    let api = filePath |> Document.loadFromYamlFile

    // constructor
    let ctor =
        ProvidedConstructor(List.empty, (fun _ -> <@@ () @@>))
        //makeConstructor List.empty (fun _ -> <@@ () @@>)
        //|>! addXmlDocDelayed "Creates new OpenAPI Type Provider"
    docType.AddMember(ctor)
    
    let version = api.SpecificationVersion
    let vProp = ProvidedProperty("Version", typeof<string>, (fun args -> <@@ version @@>))
        //makeProperty<string> (fun _ -> <@@ version @@>) "Version"
        //|>! addXmlDocDelayed "Open API Version"
    docType.AddMember(vProp)

    docType