module OpenAPITypeProvider.Parser.MediaType

open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    {
        Schema = node |> findSchema (toMappingNode >> Schema.parse rootNode)
    } : MediaType