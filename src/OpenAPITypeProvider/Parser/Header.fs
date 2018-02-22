module OpenAPITypeProvider.Parser.Header

open System
open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let private someBoolOrFalse = function
    | Some v -> v |> Boolean.Parse
    | None -> false

let parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    {
        Description = node |> tryFindScalarValue "description"
        Required = node |> tryFindScalarValue "required" |> someBoolOrFalse
        Deprecated = node |> tryFindScalarValue "deprecated" |> someBoolOrFalse
        AllowEmptyValue = node |> tryFindScalarValue "allowEmptyValue" |> someBoolOrFalse
        Schema = node |> findByNameM "schema" (toMappingNode >> Schema.parseSchema rootNode)
    } : Header