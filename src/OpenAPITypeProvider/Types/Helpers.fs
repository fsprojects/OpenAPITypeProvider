module OpenAPITypeProvider.Types.Helpers

open ProviderImplementation.ProvidedTypes

let (|?>) (prop:ProvidedProperty) (s:string option)  =
    match s with
    | Some v -> 
        prop.AddXmlDoc v
        prop
    | None -> prop