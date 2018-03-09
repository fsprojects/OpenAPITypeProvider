module OpenAPITypeProvider.Parser.Header

open System
open OpenAPIProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let rec parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    
    let parseDirect node = 
        {
            Description = node |> tryFindScalarValue "description"
            Required = node |> tryFindScalarValue "required" |> someBoolOr false
            Deprecated = node |> tryFindScalarValue "deprecated" |> someBoolOr false
            AllowEmptyValue = node |> tryFindScalarValue "allowEmptyValue" |> someBoolOr false
            Schema = node |> findSchema (toMappingNode >> Schema.parse rootNode)
        } : Header

    let parseRef refString =
        refString 
        |> findByRef rootNode
        |> parse rootNode
    
    match node with
    | Ref r -> r |> parseRef
    | _ -> node |> parseDirect