module OpenAPITypeProvider.Tests.Parser.Parameter

open NUnit.Framework
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification
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
let ``Parses parameter``() = 
    let actual = "Parameter.yaml" |> SampleLoader.parseWithRoot Parameter.parse
    Assert.AreEqual(sample, actual)