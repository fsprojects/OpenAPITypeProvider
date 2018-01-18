#I "../../packages/YamlDotNet/lib/netstandard1.3"

#r "YamlDotNet.dll"

open YamlDotNet
open YamlDotNet.RepresentationModel
open System.IO

let yamlFile = "sample.yaml" |> File.ReadAllText

let reader = new StringReader(yamlFile)

let yaml = YamlStream()
yaml.Load(reader)
let root = yaml.Documents.[0].RootNode :?> YamlMappingNode
root.Children 
|> Seq.map (fun pair -> pair.Key) 
|> Seq.map (fun x -> x :?> YamlScalarNode)
|> Seq.map (fun x -> x.Value)