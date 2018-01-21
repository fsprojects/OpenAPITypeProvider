module OpenAPITypeProvider.Parser.Core

open YamlDotNet.RepresentationModel

let findByName name (node:YamlMappingNode) =
    node.Children 
    |> Seq.find (fun x -> x.Key.ToString() = name) 
    |> (fun x -> x.Value)

let tryFindByName name (node:YamlMappingNode) =
    node.Children 
    |> Seq.tryFind (fun x -> x.Key.ToString() = name)
    |> Option.bind (fun x -> x.Value |> Some)

let value (node:YamlNode) = node :?> YamlScalarNode |> (fun x -> x.Value)
let scalarValue name = findByName name >> value
let scalarValueM name mapFn = findByName name >> value >> mapFn
let tryScalarValue name = tryFindByName name >> Option.bind (value >> Some)
let tryScalarValueM name mapFn = tryScalarValue name >> Option.bind (mapFn >> Some)
let toMappingNode (node:YamlNode) = node :?> YamlMappingNode
let toNamedMap = 
    toMappingNode 
    >> (fun x -> x.Children)
    >> Seq.map (|KeyValue|)
    >> Seq.map (fun (k,v) -> k.ToString(), v)
    >> Map.ofSeq

let mapNode name mapFn = findByName name >> toMappingNode >> mapFn
let tryMapNode name mapFn = tryFindByName name >> Option.bind (toMappingNode >> mapFn >> Some)
let mapNodeChildren name mapFn = findByName name >> toMappingNode >> toNamedMap >> Map.map mapFn
