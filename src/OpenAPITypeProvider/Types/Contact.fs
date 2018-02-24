module OpenAPITypeProvider.Types.Contact

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types.Helpers
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification
open System

let createType asm ns (contact:Contact) =
    let typ = ProvidedTypeDefinition(asm, ns, "Contact", None, hideObjectMethods = true, nonNullable = true)
    
    // name
    if contact.Name.IsSome then
        let name = contact.Name.Value
        makeProperty<string> (fun _ -> <@@ name @@>) "Name"
        |> typ.AddMember
    
    // url
    if contact.Url.IsSome then
        let url = contact.Url.Value |> string
        makeProperty<System.Uri> (fun _ -> <@@ new System.Uri(url) @@>) "Url"
        |> typ.AddMember

    // email
    if contact.Email.IsSome then
        let email = contact.Email.Value
        makeProperty<string> (fun _ -> <@@ email @@>) "Email"
        |> typ.AddMember

    typ