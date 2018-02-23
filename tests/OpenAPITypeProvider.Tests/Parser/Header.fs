module OpenAPITypeProvider.Tests.Parser.Header

open NUnit.Framework
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Tests

let sample = {
    Description = Some "Hello"
    Required = true
    Deprecated = true
    AllowEmptyValue = true
    Schema = Schema.Integer(IntFormat.Default)
}

[<Test>]
let ``Parses header``() = 
    let actual = "Header.yaml" |> SampleLoader.parseWithRoot Header.parse
    Assert.AreEqual(sample, actual)