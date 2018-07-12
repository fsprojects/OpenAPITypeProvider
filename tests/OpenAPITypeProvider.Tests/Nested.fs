module OpenAPITypeProvider.Tests.Nested

open NUnit.Framework
open OpenAPIProvider
open OpenAPITypeProvider.Tests.Shared

type Nested = OpenAPIV3Provider<"Samples/Nested.yaml ">

[<Test>]
let ``Nested array schema``() =
    let json = """{"contacts":[{"name":"A","surname":"B"}],"name":"Roman"}"""
    let contacts = new Nested.Schemas.Contact(name = "A", surname = "B") |> List.singleton
    let instance = new Nested.Schemas.ContactWrapper(name = "Roman", contacts = contacts)
    instance |> compareJson json
    json |> Nested.Schemas.ContactWrapper.Parse |> compareJson json
    Assert.AreEqual("Roman", instance.Name)
    Assert.AreEqual("A", instance.Contacts.[0].Name)
    Assert.AreEqual("B", instance.Contacts.[0].Surname)

[<Test>]
let ``Nested direct schema``() =
    let json = """{"contact":{"name":"A","surname":"B"},"name":"Roman"}"""
    let contact = new Nested.Schemas.Contact(name = "A", surname = "B")
    let instance = new Nested.Schemas.ContactWrapper2(name = "Roman", contact = contact)
    instance |> compareJson json
    json |> Nested.Schemas.ContactWrapper2.Parse |> compareJson json
    Assert.AreEqual("Roman", instance.Name)
    Assert.AreEqual("A", instance.Contact.Name)
    Assert.AreEqual("B", instance.Contact.Surname)

[<Test>]
let ``Nested array simple schema``() =
    let json = """{"items":["A","B"]}"""
    let instance = new Nested.Schemas.WithArray(items = ["A";"B"])
    instance |> compareJson json
    json |> Nested.Schemas.WithArray.Parse |> compareJson json
    Assert.AreEqual("A", instance.Items.[0])
    Assert.AreEqual("B", instance.Items.[1])

[<Test>]
let ``Nested optional array simple schema``() =
    let json = """{"items":["A","B"]}"""
    let instance = new Nested.Schemas.WithArrayOptional(items = Some ["A";"B"])
    instance |> compareJson json
    json |> Nested.Schemas.WithArrayOptional.Parse |> compareJson json
    Assert.AreEqual("A", instance.Items.Value.[0])
    Assert.AreEqual("B", instance.Items.Value.[1])

[<Test>]
let ``Nested optional array simple schema - not provided``() =
    let json = """{"name":"A"}"""
    let instance = new Nested.Schemas.WithArrayOptional(name = Some "A")
    instance |> compareJson json
    json |> Nested.Schemas.WithArrayOptional.Parse |> compareJson json
    Assert.AreEqual("A", instance.Name.Value)