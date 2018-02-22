module OpenAPITypeProvider.Tests.Parser.Header

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

let headerSample = {
    Description = Some "Hello"
    Required = true
    Deprecated = true
    AllowEmptyValue = true
    Schema = Schema.Integer(IntFormat.Default)
}

[<Test>]
let ``Parses header``() = 
    let actual = "Header.yaml" |> parseSample Header.parse
    Assert.AreEqual(headerSample, actual)