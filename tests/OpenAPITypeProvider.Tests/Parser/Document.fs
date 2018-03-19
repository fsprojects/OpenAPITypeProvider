module OpenAPITypeProvider.Tests.Parser.Document

open NUnit.Framework
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Tests

[<Test>]
let ``Parses document (Petstore)``() = 
    let x = "Document-Petstore.yaml" |> SampleLoader.parse Document.parse |> ignore
    let y = x
    Assert.Pass()