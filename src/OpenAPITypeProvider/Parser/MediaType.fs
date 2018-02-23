module OpenAPITypeProvider.Parser.MediaType

open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    {
        Schema = node |> findByNameM "schema" (toMappingNode >> Schema.parse rootNode)
    } : MediaType