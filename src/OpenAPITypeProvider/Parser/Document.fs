module OpenAPITypeProvider.Parser.Document

open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open System.Dynamic
open System

let parse (rootNode:YamlMappingNode) = {
    SpecificationVersion = rootNode |> findScalarValue "openapi"
    Info = rootNode |> findByNameM "info" (toMappingNode >> Info.parse)
    Paths = rootNode |> findByNameM "paths" (toMappingNode >> toNamedMapM (Path.parse rootNode))
    Components = rootNode |> tryFindByName "components" |> Option.map (toMappingNode >> Components.parse rootNode)
}

let parseFromYaml content =
    let reader = new StringReader(content)
    let yaml = YamlStream()
    yaml.Load(reader)
    yaml.Documents.[0].RootNode |> toMappingNode |> parse

let readFile p = p |> File.ReadAllText

let loadFromYamlFile file = file |> readFile |> parseFromYaml

let convertJsonToYaml json =
    let expConverter = new ExpandoObjectConverter();
    let deserializedObject = JsonConvert.DeserializeObject<ExpandoObject>(json, expConverter);
    let serializer = new YamlDotNet.Serialization.Serializer();
    serializer.Serialize(deserializedObject)

let parseFromJson content = content |> convertJsonToYaml |> parseFromYaml

let loadFromJsonFile file = file |> readFile |> parseFromJson