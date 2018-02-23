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
let ``Parses request body (direct)``() = 
    let actual = "RequestBody.yaml" |> SampleLoader.parseMapWithRoot RequestBody.parse
    Assert.AreEqual(rbSample, actual.["direct"])

[<Test>]
let ``Parses request body (ref)``() = 
    let actual = "RequestBody.yaml" |> SampleLoader.parseMapWithRoot RequestBody.parse
    Assert.AreEqual(rbSample, actual.["referenced"])