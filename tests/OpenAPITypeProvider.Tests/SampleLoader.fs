module OpenAPITypeProvider.Tests.SampleLoader

open System
open System.IO
open YamlDotNet.RepresentationModel
open OpenAPITypeProvider.Parser.Core

let parseWithRoot parseFn name =
    let yamlFile = Path.Combine([|AppDomain.CurrentDomain.BaseDirectory; "Samples"; name |]) |> File.ReadAllText
    let reader = new StringReader(yamlFile)
    let yaml = YamlStream()
    yaml.Load(reader)
    yaml.Documents.[0].RootNode :?> YamlMappingNode |> (fun n -> parseFn n n)

let parse parseFn name =
    let yamlFile = Path.Combine([|AppDomain.CurrentDomain.BaseDirectory; "Samples"; name |]) |> File.ReadAllText
    let reader = new StringReader(yamlFile)
    let yaml = YamlStream()
    yaml.Load(reader)
    yaml.Documents.[0].RootNode :?> YamlMappingNode |> parseFn

let parseMapWithRoot parseFn name =
    let root = name |> parse id
    root |> toNamedMapM (parseFn root)