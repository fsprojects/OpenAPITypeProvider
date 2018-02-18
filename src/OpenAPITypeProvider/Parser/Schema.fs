module OpenAPITypeProvider.Parser.Schema

open System
open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let private intFormatFromString = function
    | "int32" -> IntFormat.Int32
    | "int64" -> IntFormat.Int64
    | _ -> IntFormat.Default

let private numberFormatFromString = function
    | "float" -> NumberFormat.Float
    | "double" -> NumberFormat.Double
    | _ -> NumberFormat.Default

let private stringFormatFromString = function
    | "binary" -> StringFormat.Binary
    | "byte" -> StringFormat.Byte
    | "date" -> StringFormat.Date
    | "date-time" -> StringFormat.DateTime
    | "password" -> StringFormat.Password
    | _ -> StringFormat.Default

let private tryParseFormat fn node =
    node 
    |> tryScalarValue "format"
    |> (fun x -> defaultArg x String.Empty)
    |> fn

let rec private parseSchema (node:YamlMappingNode) =
    let typ = node |> scalarValue "type" 
    match typ with
    | "array" -> 
        let items = node |> findByName "items" |> toMappingNode
        items |> parseSchema |> Schema.Array
    | "integer" -> node |> tryParseFormat intFormatFromString |> Schema.Integer
    | "string" -> node |> tryParseFormat stringFormatFromString |> Schema.String
    | "boolean" -> Schema.Boolean
    | "number" -> node |> tryParseFormat numberFormatFromString |> Schema.Number

let parse (node:YamlMappingNode) = 
    node.Children 
    |> Seq.map (fun x ->
        let name = x.Key.ToString()
        let schema = x.Value |> toMappingNode |> parseSchema
        name, schema
    )
    |> Map