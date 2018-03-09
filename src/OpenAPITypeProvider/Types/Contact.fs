module OpenAPITypeProvider.Types.Contact

open ProviderImplementation.ProvidedTypes
open OpenAPIProvider.Specification

let createType asm ns (contact:Contact) =
    let typ = ProvidedTypeDefinition(asm, ns, "Contact", None, hideObjectMethods = true, nonNullable = true)
    
    // name
    if contact.Name.IsSome then
        let name = contact.Name.Value
        ProvidedProperty("Name", typeof<string>, (fun _ -> <@@ name @@>)) |> typ.AddMember
    
    // url
    if contact.Url.IsSome then
        let url = contact.Url.Value |> string
        ProvidedProperty("Url", typeof<System.Uri>, (fun _ -> <@@ new System.Uri(url) @@>)) |> typ.AddMember

    // email
    if contact.Email.IsSome then
        let email = contact.Email.Value
        ProvidedProperty("Email", typeof<string>, (fun _ -> <@@ email @@>)) |> typ.AddMember
    
    typ