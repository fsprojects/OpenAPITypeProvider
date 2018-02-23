module OpenAPITypeProvider.Parser.RequestBody

open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    {
        Description = node |> tryFindScalarValue "description"
        Required = node |> tryFindScalarValue "required" |> someBoolOr false
        Content = 
            node |> findByNameM "content" toMappingNode 
            |> toMappingNode |> toNamedMapM (MediaType.parse rootNode)
    } : RequestBody