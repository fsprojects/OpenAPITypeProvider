module OpenAPITypeProvider.Parser.Response

open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let rec parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    
    let parseDirect node =
        {
            Description = node |> findScalarValue "description"
            Headers = 
                node |> findByNameM "headers" toMappingNode
                |> toMappingNode |> toNamedMapM (Header.parse rootNode)
            Content = 
                node |> findByNameM "content" toMappingNode 
                |> toMappingNode |> toNamedMapM (MediaType.parse rootNode)
        } : Response

    let parseRef refString =
        refString 
        |> findByRef rootNode
        |> parse rootNode
    
    match node with
    | Ref r -> r |> parseRef
    | _ -> node |> parseDirect