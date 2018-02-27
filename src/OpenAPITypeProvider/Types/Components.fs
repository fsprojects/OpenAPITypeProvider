module OpenAPITypeProvider.Types.Components

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification

let createType asm ns (components:Components) =
    let typ = ProvidedTypeDefinition(asm, ns, "Components", None, hideObjectMethods = true, nonNullable = true)
    
    // Schemas
    let schemas = ProvidedTypeDefinition(asm, ns, "Schemas", None, hideObjectMethods = true, nonNullable = true)
    components.Schemas
    |> Map.iter (fun name schema -> 
        Schema.getMembers asm ns schemas name schema |> schemas.AddMember
    )
    schemas |> typ.AddMember
    ProvidedProperty("Schemas", schemas, (fun _ -> <@@ obj() @@>)) |> typ.AddMember

    typ