module OpenAPITypeProvider.Parser.License

open System
open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let parse (node:YamlMappingNode) = {
    Name = node |> findScalarValue "name"
    Url = node |> tryFindScalarValueM "url" Uri
}