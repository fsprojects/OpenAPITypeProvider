module OpenAPITypeProvider.Parser.Info

open System
open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let parse (node:YamlMappingNode) = 
    {
        Title = node |> scalarValue "title"
        Description = node |> tryScalarValue "description"
        TermsOfService = node |> tryScalarValueM "termsOfService" Uri
        Contact = node |> tryMapNode "contact" Contact.parse
        License = node |> tryMapNode "license" License.parse
        Version = node |> scalarValue "version"
    } : Info