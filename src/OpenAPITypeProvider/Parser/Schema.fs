module OpenAPITypeProvider.Parser.Schema

open System
open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel


let rec private parseSchema (node:YamlMappingNode) =
    let typ = node |> scalarValue "type" 
    match typ with
    | "array" -> 
        let items = node |> findByName "items" |> toMappingNode
        items |> parseSchema |> Schema.Array
    | "integer" ->
        Schema.Integer IntFormat.Default

let parse (node:YamlMappingNode) = 
    let x = node.Children |> Seq.head
    let name = x.Key.ToString()
    let schema = x.Value |> toMappingNode |> parseSchema
    name, schema