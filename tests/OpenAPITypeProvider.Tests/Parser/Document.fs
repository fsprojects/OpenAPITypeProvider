module OpenAPITypeProvider.Tests.Parser.Document

open NUnit.Framework
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Tests

[<Test>]
let ``Parses document (Petstore)``() = 
    "Document-Petstore.yaml" |> SampleLoader.parse Document.parse |> ignore
    Assert.Pass()