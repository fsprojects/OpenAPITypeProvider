module OpenAPITypeProvider.Parser.Components

open System
open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

// let parse (root:YamlMappingNode) (node:YamlMappingNode) = 
//     {
//         Schemas = node |> tryFindByNameM "schemas" (toMappingNode >> Schema.parse root)
        
//     } : Components