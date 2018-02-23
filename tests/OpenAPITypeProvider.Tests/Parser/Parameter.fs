module OpenAPITypeProvider.Tests.Parser.Parameter

open System
open System.IO
open NUnit.Framework
open YamlDotNet.RepresentationModel
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification

let parseSample parseFn name =
    let yamlFile = Path.Combine([|AppDomain.CurrentDomain.BaseDirectory; "Samples"; name |]) |> File.ReadAllText
    let reader = new StringReader(yamlFile)
    let yaml = YamlStream()
    yaml.Load(reader)
    yaml.Documents.[0].RootNode :?> YamlMappingNode |> (fun n -> parseFn n n)

let sample = {
    Name = "Hello"
    In = "In value"
    Description = Some "Some desc"
    Required = true
    Deprecated = true
    AllowEmptyValue = true
    Schema = Schema.String(StringFormat.Default)
    Content =
        ["application/json", { Schema = Schema.Integer(IntFormat.Default) }] |> Map
}

[<Test>]
let ``Parses parameter``() = 
    let actual = "Parameter.yaml" |> parseSample Parameter.parse
    Assert.AreEqual(sample, actual)