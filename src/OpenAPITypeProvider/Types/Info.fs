module OpenAPITypeProvider.Types.Info

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider.Parser
open OpenAPIProvider.Specification

let createType asm ns (info:Info) =
    let typ = ProvidedTypeDefinition(asm, ns, "Info", None, hideObjectMethods = true, nonNullable = true)
    
    // version
    let version = info.Version
    ProvidedProperty("Version", typeof<string>, (fun _ -> <@@ version @@>)) |> typ.AddMember

    // title
    let title = info.Title
    ProvidedProperty("Title", typeof<string>, (fun _ -> <@@ title @@>)) |> typ.AddMember
    
    // description
    if info.Description.IsSome then
        let desc = info.Description.Value
        ProvidedProperty("Description", typeof<string>, (fun _ -> <@@ desc @@>)) |> typ.AddMember
    
    // terms of service
    if info.TermsOfService.IsSome then
        let trms = info.TermsOfService.Value |> string
        ProvidedProperty("TermsOfService", typeof<System.Uri>, (fun _ -> <@@ new System.Uri(trms) @@>)) |> typ.AddMember

    // info object
    if info.Contact.IsSome then
        let contact = Contact.createType asm ns info.Contact.Value
        contact |> typ.AddMember
        ProvidedProperty("Contact", contact, fun _ -> <@@ obj() @@>) |> typ.AddMember
    
    // license object
    if info.License.IsSome then
        let license = License.createType asm ns info.License.Value
        license |> typ.AddMember
        ProvidedProperty("License", license, fun _ -> <@@ obj() @@>) |> typ.AddMember

    typ