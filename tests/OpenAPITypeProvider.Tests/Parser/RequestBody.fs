module OpenAPITypeProvider.Tests.Parser.RequestBody

open NUnit.Framework
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Tests

let rbSample = {
    Description = Some "Hello"
    Required = false
    Content =
        ["application/json", { Schema = Schema.Integer(IntFormat.Default) }] |> Map
}

[<Test>]
let ``Parses request body``() = 
    let actual = "RequestBody.yaml" |> SampleLoader.parseWithRoot RequestBody.parse
    Assert.AreEqual(rbSample, actual)