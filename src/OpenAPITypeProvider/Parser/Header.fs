module OpenAPITypeProvider.Parser.Header

open System
open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    {
        Description = node |> tryFindScalarValue "description"
        Required = node |> tryFindScalarValue "required" |> someBoolOr false
        Deprecated = node |> tryFindScalarValue "deprecated" |> someBoolOr false
        AllowEmptyValue = node |> tryFindScalarValue "allowEmptyValue" |> someBoolOr false
        Schema = node |> findByNameM "schema" (toMappingNode >> Schema.parseSchema rootNode)
    } : Header