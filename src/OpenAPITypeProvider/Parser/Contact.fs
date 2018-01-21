module OpenAPITypeProvider.Parser.Contact

open System
open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let parse (node:YamlMappingNode) = 
    {
        Name = node |> tryScalarValue "name"
        Url = node |> tryScalarValueM "url" Uri
        Email = node |> tryScalarValue "email"
    }