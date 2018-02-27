module OpenAPITypeProvider.Types.Info

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types.Helpers
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification
open System

let createType asm ns (info:Info) =
    let typ = ProvidedTypeDefinition(asm, ns, "Info", None, hideObjectMethods = true, nonNullable = true)
    
    // version
    let version = info.Version
    makeProperty<string> (fun _ -> <@@ version @@>) "Version"
    |> typ.AddMember

    // title
    let title = info.Title
    makeProperty<string> (fun _ -> <@@ title @@>) "Title"
    |> typ.AddMember
    
    // description
    if info.Description.IsSome then
        let desc = info.Description.Value
        makeProperty<string> (fun _ -> <@@ desc @@>) "Description"
        |> typ.AddMember
    
    // terms of service
    if info.TermsOfService.IsSome then
        let trms = info.TermsOfService.Value |> string
        makeProperty<System.Uri> (fun _ -> <@@ new System.Uri(trms) @@>) "TermsOfService"
        |> typ.AddMember

    // info object
    if info.Contact.IsSome then
        Contact.createType asm ns info.Contact.Value
        |> addAsProperty "Contact" typ
    
    // license object
    if info.License.IsSome then
        License.createType asm ns info.License.Value
        |> addAsProperty "License" typ

    typ