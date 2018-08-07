module OpenAPITypeProvider.Tests.Issues

open NUnit.Framework
open OpenAPITypeProvider
open Newtonsoft.Json
open System

type Issues = OpenAPIV3Provider<"Samples/Issues.yaml">

[<Test>]
let ``Issue #2 - Parses and converts object schema with optional sub schema``() =
    let json = """{"name":"Roman","invoiceAddress":{"city":"Prague","country":"CZE","street":"Krakovska 9b","zip":"15100"}}"""
    let parsed = Issues.Schemas.AgentRequest.Parse json
    Assert.AreEqual("Prague",parsed.InvoiceAddress.Value.City)