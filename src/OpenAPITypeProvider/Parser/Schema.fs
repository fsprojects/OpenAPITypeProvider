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
    |> tryFindScalarValue "format"
    |> (fun x -> defaultArg x String.Empty)
    |> fn

let optionToList = function
    | Some v -> v |> Seq.toList
    | None -> []

let rec private parseSchema (node:YamlMappingNode) =
    let typ = node |> findScalarValue "type" 
    match typ with
    | "array" -> 
        let items = node |> findByName "items" |> toMappingNode
        items |> parseSchema |> Schema.Array
    | "integer" -> node |> tryParseFormat intFormatFromString |> Schema.Integer
    | "string" -> node |> tryParseFormat stringFormatFromString |> Schema.String
    | "boolean" -> Schema.Boolean
    | "number" -> node |> tryParseFormat numberFormatFromString |> Schema.Number
    | "object" ->
        let props = 
            node 
            |> findByNameM "properties" toMappingNode
            |> toNamedMapM (fun _ v -> v |> toMappingNode |> parseSchema)
        let required = 
            node |> tryFindByName "required" 
            |> Option.map seqValue
            |> Option.map (List.map (fun x -> x.ToString()))
            |> optionToList
        Schema.Object(props, required)

let parse (node:YamlMappingNode) = 
    node |> toNamedMapM (fun _ v -> v |> toMappingNode |> parseSchema)