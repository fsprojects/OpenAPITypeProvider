module OpenAPITypeProvider.Types.Parameter

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations


let createType ctx name (par:Parameter) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    
    let name = par.Name
    ProvidedProperty("Name", typeof<string>, (fun _ -> <@@ name @@>)) |> typ.AddMember

    // In
    let i = par.In
    ProvidedProperty("In", typeof<string>, (fun _ -> <@@ i @@>)) |> typ.AddMember

    // Description
    if par.Description.IsSome then
        let desc = par.Description.Value
        ProvidedProperty("Description", typeof<string>, (fun _ -> <@@ desc @@>)) |> typ.AddMember
    
    // Required
    let req = par.Required
    ProvidedProperty("Required", typeof<bool>, (fun _ -> <@@ req @@>)) |> typ.AddMember
    
    // Deprecated
    let dep = par.Deprecated
    ProvidedProperty("Deprecated", typeof<bool>, (fun _ -> <@@ dep @@>)) |> typ.AddMember
    
    // AllowEmptyValue
    let aev = par.AllowEmptyValue
    ProvidedProperty("AllowEmptyValue", typeof<bool>, (fun _ -> <@@ aev @@>)) |> typ.AddMember

    typ

let createTypes ctx (pars:Parameter list) =
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, "Parameters", Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    pars |> List.iter (fun p -> 
        let name = Names.pascalName p.Name
        let par = createType ctx name p
        par |> typ.AddMember
        ProvidedProperty(name, par, fun _ -> <@@ obj() @@>) |> typ.AddMember
    )
    typ