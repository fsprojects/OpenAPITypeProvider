module OpenAPITypeProvider.Tests.Parser.Response

open NUnit.Framework
open OpenAPITypeProvider.Parser
open OpenAPIProvider.Specification
open OpenAPITypeProvider.Tests


let sample = {
    Description = "Hello"
    Headers = ["myHeader", Header.sample ] |> Map
    Content = 
        [
            "application/json", { Schema = Schema.Integer(IntFormat.Default) }
            "application/xml", { Schema = Schema.Empty }
        ] |> Map
}

[<Test>]
let ``Parses response (direct)``() = 
    let responses = "Response.yaml" |> SampleLoader.parseMapWithRoot Response.parse
    Assert.AreEqual(sample, responses.["direct"])

[<Test>]
let ``Parses response (ref)``() = 
    let responses = "Response.yaml" |> SampleLoader.parseMapWithRoot Response.parse
    Assert.AreEqual(sample, responses.["referenced"])