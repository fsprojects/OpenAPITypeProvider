module OpenAPITypeProvider.Types.Components

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types.Helpers
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification
open System

let createType asm ns (components:Components) =
    let typ = ProvidedTypeDefinition(asm, ns, "Components", None, hideObjectMethods = true, nonNullable = true)
    
    let schemas = ProvidedTypeDefinition(asm, ns, "Schemas", None, hideObjectMethods = true, nonNullable = true)
    components.Schemas
    |> Map.iter (fun name schema -> 
        Schema.getMembers asm ns schemas name schema |> schemas.AddMember
    )
    schemas |> addAsProperty "Schemas" typ
    typ