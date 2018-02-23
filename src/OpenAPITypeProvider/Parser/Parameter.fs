module OpenAPITypeProvider.Parser.Parameter

open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    {
        Name = node |> findScalarValue "name"
        In = node |> findScalarValue "in"
        Description = node |> tryFindScalarValue "description"
        Required = node |> tryFindScalarValue "required" |> someBoolOr false
        Deprecated = node |> tryFindScalarValue "deprecated" |> someBoolOr false
        AllowEmptyValue = node |> tryFindScalarValue "allowEmptyValue" |> someBoolOr false
        Schema = node |> findByNameM "schema" (toMappingNode >> Schema.parseSchema rootNode)        
        Content = 
            node |> findByNameM "content" toMappingNode 
            |> toNamedMapM (fun _ v -> v |> toMappingNode |> MediaType.parse rootNode) 
    } : Parameter