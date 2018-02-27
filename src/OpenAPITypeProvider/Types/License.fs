module OpenAPITypeProvider.Types.License

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider.Specification

let createType asm ns (license:License) =
    let typ = ProvidedTypeDefinition(asm, ns, "License", None, hideObjectMethods = true, nonNullable = true)
    
    // name
    let name = license.Name
    ProvidedProperty("Name", typeof<string>, (fun _ -> <@@ name @@>)) |> typ.AddMember
    
    // url
    if license.Url.IsSome then
        let url = license.Url.Value |> string
        ProvidedProperty("Url", typeof<System.Uri>, (fun _ -> <@@ new System.Uri(url) @@>)) |> typ.AddMember

    typ