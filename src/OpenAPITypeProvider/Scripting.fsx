open System.Collections.Generic
#I "../../packages/YamlDotNet/lib/netstandard1.3"

#r "YamlDotNet.dll"

open System
open YamlDotNet
open YamlDotNet.RepresentationModel
open System.IO

#load "OpenAPI.fsx"
open OpenAPI

let yamlFile = "sample.yaml" |> File.ReadAllText
let reader = new StringReader(yamlFile)
let yaml = YamlStream()
yaml.Load(reader)
let document = yaml.Documents.[0]

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


let mappingNode (node:YamlNode) = node :?> YamlMappingNode

let toNamedMap = 
    mappingNode 
    >> (fun x -> x.Children)
    >> Seq.map (|KeyValue|)
    >> Seq.map (fun (k,v) -> k.ToString(), v)
    >> Map.ofSeq

let mapNode name mapFn = findByName name >> mappingNode >> mapFn
let tryMapNode name mapFn = tryFindByName name >> Option.bind (mappingNode >> mapFn >> Some)

let mapNodeChildren name mapFn = findByName name >> mappingNode >> toNamedMap >> Map.map mapFn

let parseContact (node:YamlMappingNode) = 
    {
        Name = node |> tryScalarValue "name"
        Url = node |> tryScalarValueM "url" Uri
        Email = node |> tryScalarValue "email"
    } : Contact

let parseLicense (node:YamlMappingNode) = {
    Name = node |> scalarValue "name"
    Url = node |> tryScalarValueM "url" Uri
}

let parseInfo (node:YamlMappingNode) = 
    {
        Title = node |> scalarValue "title"
        Description = node |> tryScalarValue "description"
        TermsOfService = node |> tryScalarValueM "termsOfService" Uri
        Contact = node |> tryMapNode "contact" parseContact
        License = node |> tryMapNode "license" parseLicense
        Version = node |> scalarValue "version"
    } : Info

let parsePath key node = 
    let node = node |> mappingNode
    {
        Summary = node |> tryScalarValue "summary"
        Description = node |> tryScalarValue "description"
        Get = None
        Put = None
        Post = None
        Delete = None
        Options = None
        Head = None
        Patch = None
        Trace = None
        Parameters = []
    } : Path


let parseOpenAPI (doc:YamlDocument) =
    let root = doc.RootNode :?> YamlMappingNode
    {
        SpecificationVersion = root |> scalarValue "openapi"
        Info = root |> mapNode "info" parseInfo
        Paths = root |> mapNodeChildren "paths" parsePath
        Components = None
    } : OpenAPI

document |> parseOpenAPI
//let test = document.RootNode :?> YamlMappingNode
//test |> mapNodeChildren "paths" parsePath
