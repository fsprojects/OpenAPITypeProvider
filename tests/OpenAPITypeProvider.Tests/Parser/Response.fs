module OpenAPITypeProvider.Tests.Parser.Response

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
    Description = "Hello"
    Headers = ["myHeader", Header.sample ] |> Map
    Content = ["application/json", { Schema = Schema.Integer(IntFormat.Default) }] |> Map
}

[<Test>]
let ``Parses response``() = 
    let actual = "Response.yaml" |> parseSample Response.parse
    Assert.AreEqual(sample, actual)