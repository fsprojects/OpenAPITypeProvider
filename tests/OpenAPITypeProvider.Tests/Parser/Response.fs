module OpenAPITypeProvider.Tests.Parser.Response

open System
open NUnit.Framework
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Tests


let sample = {
    Description = "Hello"
    Headers = ["myHeader", Header.sample ] |> Map
    Content = ["application/json", { Schema = Schema.Integer(IntFormat.Default) }] |> Map
}

[<Test>]
let ``Parses response``() = 
    let actual = "Response.yaml" |> SampleLoader.parseWithRoot Response.parse
    Assert.AreEqual(sample, actual)