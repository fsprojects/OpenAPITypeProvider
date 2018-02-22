module OpenAPITypeProvider.Tests.Parser.RequestBody

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

let rbSample = {
    Description = Some "Hello"
    Required = false
    Content =
        ["application/json", { Schema = Schema.Integer(IntFormat.Default) }] |> Map
}

[<Test>]
let ``Parses request body``() = 
    let actual = "RequestBody.yaml" |> parseSample RequestBody.parse
    Assert.AreEqual(rbSample, actual)