module OpenAPITypeProvider.Types.License

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types.Helpers
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification
open System

let createType asm ns (license:License) =
    let typ = ProvidedTypeDefinition(asm, ns, "License", None, hideObjectMethods = true, nonNullable = true)
    
    // name
    let name = license.Name
    makeProperty<string> (fun _ -> <@@ name @@>) "Name"
    |> typ.AddMember
    
    // url
    if license.Url.IsSome then
        let url = license.Url.Value |> string
        makeProperty<System.Uri> (fun _ -> <@@ new System.Uri(url) @@>) "Url"
        |> typ.AddMember

    typ