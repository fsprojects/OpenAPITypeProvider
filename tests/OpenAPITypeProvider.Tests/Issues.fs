module OpenAPITypeProvider.Tests.Issues

open NUnit.Framework
open OpenAPIProvider
open OpenAPITypeProvider.Tests.Shared
open Newtonsoft.Json.Linq
open Newtonsoft.Json

type Issues = OpenAPIV3Provider<"Samples/Issues.yaml ">
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

[<Test>]
let ``Can parse json to strongly typed object``() =
    let instance = Issues.Schemas.SaveAgent.Parse(json)
    Assert.AreEqual("AG001b", instance.AccountNumber)
    Assert.AreEqual("Roman", instance.OtherContacts.Value.Head.Name)
    Assert.IsTrue(instance.Address.IsSome)
    Assert.AreEqual("CZE", instance.Address.Value.Country)


[<Test>]
let ``Can convert instance to JToken``() =
    let address = Issues.Schemas.Address("city", "country", "street", "zip")
    let instance = Issues.Schemas.SmallAgent(Some address, Some "Roman")
    Assert.AreEqual("Roman", instance.Name.Value)
    Assert.AreEqual("country", instance.Address.Value.Country)
    let jtoken = instance.ToJToken()
    Assert.AreEqual(JValue("Roman"), jtoken.["name"])
    Assert.AreEqual(JValue("country"), jtoken.["address"].["country"])
    Assert.AreEqual("""{"city":"city","country":"country","street":"street","zip":"zip"}""", jtoken.["address"].ToString(Formatting.None))


[<Test>]
let ``Can convert instance to JToken with optional array``() =
    let address = Issues.Schemas.Address("city", "country", "street", "zip")
    let instance = Issues.Schemas.SmallAgentWithArray(Some [address], Some "Roman")
    Assert.AreEqual("Roman", instance.Name.Value)
    Assert.AreEqual("country", instance.Addresses.Value.[0].Country)
    let jtoken = instance.ToJToken()
    Assert.AreEqual("""[{"city":"city","country":"country","street":"street","zip":"zip"}]""", jtoken.["addresses"].ToString(Formatting.None))
    Assert.AreEqual(JValue("Roman"), jtoken.["name"])
    Assert.AreEqual(JValue("country"), jtoken.["addresses"].[0].["country"])
