module OpenAPITypeProvider.Types.RequestBody

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations


let createType ctx name (par:RequestBody) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    // Description
    if par.Description.IsSome then
        let desc = par.Description.Value
        ProvidedProperty("Description", typeof<string>, (fun _ -> <@@ desc @@>)) |> typ.AddMember

    typ
