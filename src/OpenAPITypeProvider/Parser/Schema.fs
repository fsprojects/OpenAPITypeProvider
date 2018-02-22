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

let private optionToList = function
    | Some v -> v |> Seq.toList
    | None -> []

let private isNodeType typ node = 
    node |> tryFindScalarValue "type" 
    |> Option.map (fun x -> x = typ)
    |> Option.bind (fun x -> if x then Some () else None)

let private (|Array|_|) (node:YamlMappingNode) = node |> isNodeType "array"
let private (|Integer|_|) (node:YamlMappingNode) = node |> isNodeType "integer"
let private (|String|_|) (node:YamlMappingNode) = node |> isNodeType "string"
let private (|Boolean|_|) (node:YamlMappingNode) = node |> isNodeType "boolean"
let private (|Number|_|) (node:YamlMappingNode) = node |> isNodeType "number"
let private (|Object|_|) (node:YamlMappingNode) = node |> isNodeType "object"
let private (|AllOf|_|) (node:YamlMappingNode) = node |> tryFindByName "allOf" |> Option.map seqValue
let private (|Ref|_|) (node:YamlMappingNode) = node |> tryFindScalarValue "$ref"

let private mergeSchemaPair (schema1:Schema) (schema2:Schema) = 
    match schema1, schema2 with
    | Schema.Object (p1, r1), Schema.Object (p2, r2) ->
        let required = (r1 @ r2) |> List.distinct
        let props = Map(Seq.concat [ (Map.toSeq p1) ; (Map.toSeq p2) ])
        Schema.Object (props, required)
    | _ -> failwith "Both schemas must be Object type"

let private mergeSchemas (schemas:Schema list) = 
    match schemas with
    | [] -> failwith "Schema list should not be empty"
    | list -> list |> List.reduce mergeSchemaPair

let private findByRef (rootNode:YamlMappingNode) (refString:string) =
    let parts = refString.Split([|'/'|]) |> Array.filter (fun x -> x <> "#")
    let foldFn (node:YamlMappingNode) (name:string) =
        node.Children 
        |> Seq.filter (fun x -> x.Key.ToString() = name)
        |> Seq.head
        |> (fun x -> x.Value |> toMappingNode)
    parts |> Array.fold foldFn rootNode

let rec parseSchema (rootNode:YamlMappingNode) (node:YamlMappingNode) =
    
    let parseDirect node =
        match node with
        | AllOf n -> n |> List.map (toMappingNode >> parseSchema rootNode) |> mergeSchemas
        | Array ->
            let items = node |> findByName "items" |> toMappingNode
            items |> parseSchema rootNode |> Schema.Array
        | Integer -> node |> tryParseFormat intFormatFromString |> Schema.Integer
        | String -> node |> tryParseFormat stringFormatFromString |> Schema.String
        | Boolean -> Schema.Boolean
        | Number -> node |> tryParseFormat numberFormatFromString |> Schema.Number
        | Object ->
            let props = 
                node 
                |> findByNameM "properties" toMappingNode
                |> toNamedMapM (fun _ v -> v |> toMappingNode |> parseSchema rootNode)
            let required = 
                node |> tryFindByName "required" 
                |> Option.map seqValue
                |> Option.map (List.map (fun x -> x.ToString()))
                |> optionToList
            Schema.Object(props, required)

    let parseRef refString =
        refString 
        |> findByRef rootNode
        |> parseSchema rootNode
    
    match node with
    | Ref r -> r |> parseRef
    | _ -> node |> parseDirect

let parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    node |> toNamedMapM (fun _ v -> v |> toMappingNode |> parseSchema rootNode)