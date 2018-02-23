module OpenAPITypeProvider.Parser.Response

open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    {
        Description = node |> findScalarValue "description"
        Headers = 
            node |> findByNameM "headers" toMappingNode
            |> toMappingNode |> toNamedMapM (Header.parse rootNode)
        Content = 
            node |> findByNameM "content" toMappingNode 
            |> toMappingNode |> toNamedMapM (MediaType.parse rootNode)
    } : Response