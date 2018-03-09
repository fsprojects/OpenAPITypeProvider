module OpenAPITypeProvider.Tests.Parser.Info

open System
open NUnit.Framework
open OpenAPITypeProvider.Parser
open OpenAPIProvider.Specification
open OpenAPITypeProvider.Tests

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
    let actual = "License.yaml" |> SampleLoader.parse License.parse
    Assert.AreEqual(licenseSample, actual)

[<Test>]
let ``Parses contact``() = 
    let actual = "Contact.yaml" |> SampleLoader.parse Contact.parse
    Assert.AreEqual(contactSample, actual)

[<Test>]
let ``Parses info``() = 
    let actual = "Info.yaml" |> SampleLoader.parse Info.parse
    Assert.AreEqual(infoSample, actual)
