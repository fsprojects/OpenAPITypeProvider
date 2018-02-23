module OpenAPITypeProvider.Parser.RequestBody

open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let rec parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    
    let parseDirect node = 
        {
            Description = node |> tryFindScalarValue "description"
            Required = node |> tryFindScalarValue "required" |> someBoolOr false
            Content = 
                node |> findByNameM "content" toMappingNode 
                |> toMappingNode |> toNamedMapM (MediaType.parse rootNode)
        } : RequestBody

    let parseRef refString =
        refString 
        |> findByRef rootNode
        |> parse rootNode
    
    match node with
    | Ref r -> r |> parseRef
    | _ -> node |> parseDirect