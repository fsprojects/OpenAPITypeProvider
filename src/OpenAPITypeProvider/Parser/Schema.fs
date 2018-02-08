module OpenAPITypeProvider.Parser.Schema

open System
open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let private intFormatFromString = function
    | "int32" -> IntFormat.Int32
    | "int64" -> IntFormat.Int64
    | _ -> IntFormat.Default

let private parseIntFormat node =
    node 
    |> tryScalarValue "format"
    |> (fun x -> defaultArg x String.Empty)
    |> intFormatFromString

let rec private parseSchema (node:YamlMappingNode) =
    let typ = node |> scalarValue "type" 
    match typ with
    | "array" -> 
        let items = node |> findByName "items" |> toMappingNode
        items |> parseSchema |> Schema.Array
    | "integer" ->
        node |> parseIntFormat |> Schema.Integer

let parse (node:YamlMappingNode) = 
    let x = node.Children |> Seq.head
    let name = x.Key.ToString()
    let schema = x.Value |> toMappingNode |> parseSchema
    name, schema