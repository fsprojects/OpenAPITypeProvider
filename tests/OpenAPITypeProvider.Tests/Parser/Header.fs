module OpenAPITypeProvider.Tests.Parser.Header

open NUnit.Framework
open OpenAPITypeProvider.Parser
open OpenAPIProvider.Specification
open OpenAPITypeProvider.Tests

let sample = {
    Description = Some "Hello"
    Required = true
    Deprecated = true
    AllowEmptyValue = true
    Schema = Schema.Integer(IntFormat.Default)
}

[<Test>]
let ``Parses header (direct)``() = 
    let actual = "Header.yaml" |> SampleLoader.parseMapWithRoot Header.parse
    Assert.AreEqual(sample, actual.["direct"])

[<Test>]
let ``Parses header (ref)``() = 
    let actual = "Header.yaml" |> SampleLoader.parseMapWithRoot Header.parse
    Assert.AreEqual(sample, actual.["referenced"])
