module OpenAPITypeProvider.Parser.Contact

open System
open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let parse (node:YamlMappingNode) = 
    {
        Name = node |> tryFindScalarValue "name"
        Url = node |> tryFindScalarValueM "url" Uri
        Email = node |> tryFindScalarValue "email"
    }