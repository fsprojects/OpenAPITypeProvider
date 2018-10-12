module internal OpenAPITypeProvider.Types.License

open ProviderImplementation.ProvidedTypes
open OpenAPIParser.Version3.Specification

let createType ctx (license:License) =
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, "License", Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    // name
    let name = license.Name
    ProvidedProperty("Name", typeof<string>, (fun _ -> <@@ name @@>)) |> typ.AddMember
    
    // url
    if license.Url.IsSome then
        let url = license.Url.Value |> string
        ProvidedProperty("Url", typeof<System.Uri>, (fun _ -> <@@ new System.Uri(url) @@>)) |> typ.AddMember

    typ