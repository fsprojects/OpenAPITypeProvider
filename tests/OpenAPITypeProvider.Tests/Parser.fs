module OpenAPITypeProvider.Tests.Parser

open System
open System.IO
open NUnit.Framework
open YamlDotNet.RepresentationModel
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification


let parseSample parseFn name =
    let yamlFile = "Samples/" + name |> File.ReadAllText
    let reader = new StringReader(yamlFile)
    let yaml = YamlStream()
    yaml.Load(reader)
    yaml.Documents.[0].RootNode :?> YamlMappingNode |> parseFn

let licenseSample = { Name = "MIT"; Url = Some <| Uri("http://github.com/gruntjs/grunt/blob/master/LICENSE-MIT") }
let contactSample = { Name = Some "Swagger API Team"; Email = Some "foo@example.com"; Url = Some <| Uri("http://madskristensen.net") }
let infoSample = { 
    Version = "1.0.0"
    Title = "Swagger Petstore"
    Description = Some "A sample API that uses a petstore as an example to demonstrate features in the OpenAPI 3.0 specification"
    TermsOfService = Some <| Uri ("http://swagger.io/terms/")
    Contact = Some contactSample
    License = Some licenseSample
}


[<Test>]
let ``Parses license``() = 
    let actual = "License.yaml" |> parseSample License.parse
    Assert.AreEqual(licenseSample, actual)

[<Test>]
let ``Parses contact``() = 
    let actual = "Contact.yaml" |> parseSample Contact.parse
    Assert.AreEqual(contactSample, actual)

[<Test>]
let ``Parses info``() = 
    let actual = "Info.yaml" |> parseSample Info.parse
    Assert.AreEqual(infoSample, actual)