module OpenAPITypeProvider.Tests.Parser.Parameter

open NUnit.Framework
open OpenAPITypeProvider.Parser
open OpenAPIProvider.Specification
open OpenAPITypeProvider.Tests

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
let ``Parses parameter (direct)``() = 
    let actual = "Parameter.yaml" |> SampleLoader.parseMapWithRoot Parameter.parse
    Assert.AreEqual(sample, actual.["direct"])

[<Test>]
let ``Parses parameter (ref)``() = 
    let actual = "Parameter.yaml" |> SampleLoader.parseMapWithRoot Parameter.parse
    Assert.AreEqual(sample, actual.["referenced"])