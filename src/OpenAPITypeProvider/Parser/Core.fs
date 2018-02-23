module OpenAPITypeProvider.Parser.Core

open YamlDotNet.RepresentationModel

// finders
let findByName name (node:YamlMappingNode) =
    node.Children 
    |> Seq.find (fun x -> x.Key.ToString() = name) 
    |> (fun x -> x.Value)

let findByNameM name mapFn = findByName name >> mapFn

let tryFindByName name (node:YamlMappingNode) =
    node.Children 
    |> Seq.tryFind (fun x -> x.Key.ToString() = name)
    |> Option.map (fun x -> x.Value)

let tryFindByNameM name mapFn = tryFindByName name >> Option.map mapFn

// value extractors
let value (node:YamlNode) = node :?> YamlScalarNode |> (fun x -> x.Value)
let seqValue (node:YamlNode) = node :?> YamlSequenceNode |> (fun x -> x.Children) |> Seq.toList

// finders with extractors
let findScalarValue name = findByName name >> value
let findScalarValueM name mapFn = findByName name >> value >> mapFn
let tryFindScalarValue name = tryFindByName name >> Option.map value
let tryFindScalarValueM name mapFn = tryFindScalarValue name >> Option.map mapFn

// change type
let toMappingNode (node:YamlNode) = node :?> YamlMappingNode

// key-value mappings
let toNamedMap (node:YamlMappingNode) = 
    node.Children
    |> Seq.map (|KeyValue|)
    |> Seq.map (fun (k,v) -> k.ToString(), v)
    |> Map.ofSeq

let toNamedMapM mapFn = toNamedMap >> Map.map (fun _ v -> v |> toMappingNode |> mapFn)

let someBoolOr value = function
    | Some v -> v |> System.Boolean.Parse
    | None -> value

