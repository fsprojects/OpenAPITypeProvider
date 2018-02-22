module OpenAPITypeProvider.Parser.Response

open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    {
        Description = node |> findScalarValue "description"
        Headers = 
            node |> findByNameM "headers" toMappingNode
            |> toNamedMapM (fun _ v -> v |> toMappingNode |> Header.parse rootNode) 
        Content = 
            node |> findByNameM "content" toMappingNode 
            |> toNamedMapM (fun _ v -> v |> toMappingNode |> MediaType.parse rootNode) 
    } : Response