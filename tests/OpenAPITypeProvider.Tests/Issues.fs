module OpenAPITypeProvider.Tests.Issues

open NUnit.Framework
open OpenAPIProvider
open OpenAPITypeProvider.Tests.Shared

type Issues = OpenAPIV3Provider<"Samples/Issues.yaml">

[<Test>]
let ``Save Agent - Parse``() =
    let json = """
{

  "accountNumber": "AG001b",
  "address": {
    "city": "Prague",
    "country": "CZE",
    "street": "Krakovska 9",
    "zip": "15100"
  },
  "apiKey": "12345asdfApiSECRET",
  "country": "CZ",
  "name": "Travel Agent Roman",
  "otherContacts": [
    {
    "description": "Business contact",
    "email": "roman@example.com",
    "group": "Commercial",
    "name": "Roman",
    "phone": "123456789"
  }
  ],
  "primaryContact": {
    "description": "Business contact",
    "email": "roman@example.com",
    "group": "Commercial",
    "name": "Karel Šťastný",
    "phone": "123456789"
  },
  "status": "Active"
}    
    """
    let instance = Issues.Schemas.SaveAgent.Parse(json)
    Assert.AreEqual("AG001b", instance.AccountNumber)
    Assert.AreEqual("Roman", instance.OtherContacts.Value.Head.Name)
