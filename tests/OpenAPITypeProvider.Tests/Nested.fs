module OpenAPITypeProvider.Tests.Nested

open NUnit.Framework
open OpenAPIProvider
open OpenAPITypeProvider.Tests.Shared
open Newtonsoft.Json

type Nested = OpenAPIV3Provider<"Samples/Nested.yaml ">

[<Test>]
let ``Works with nested array schema``() =
    let json = """{"contacts":[{"name":"A","surname":"B"}],"name":"Roman"}"""
    let contacts = new Nested.Schemas.Contact(name = "A", surname = "B") |> List.singleton
    let instance = new Nested.Schemas.ContactWrapper(name = "Roman", contacts = contacts)
    instance.ToJToken() |> compareJson json
    json |> Nested.Schemas.ContactWrapper.Parse |> fun x -> x.ToJToken() |> compareJson json
    Assert.AreEqual("Roman", instance.Name)
    Assert.AreEqual("A", instance.Contacts.[0].Name)
    Assert.AreEqual("B", instance.Contacts.[0].Surname)

[<Test>]
let ``Works with nested optional schema``() =
    let json = """{"contact":{"name":"A","surname":"B"},"name":"Roman"}"""
    let contact = new Nested.Schemas.Contact(name = "A", surname = "B")
    let instance = new Nested.Schemas.ContactWrapper2(name = "Roman", contact = Some contact)
    instance.ToJToken() |> compareJson json
    json |> Nested.Schemas.ContactWrapper2.Parse |> fun x -> x.ToJToken() |> compareJson json
    Assert.AreEqual("Roman", instance.Name)
    Assert.AreEqual("A", instance.Contact.Value.Name)
    Assert.AreEqual("B", instance.Contact.Value.Surname)

[<Test>]
let ``Works with array schema``() =
    let json = """{"items":["A","B"]}"""
    let instance = new Nested.Schemas.WithArray(items = ["A";"B"])
    instance.ToJToken() |> compareJson json
    json |> Nested.Schemas.WithArray.Parse |> fun x -> x.ToJToken() |> compareJson json
    Assert.AreEqual("A", instance.Items.[0])
    Assert.AreEqual("B", instance.Items.[1])

[<Test>]
let ``Works with nested optional array simple schema``() =
    let json = """{"items":["A","B"]}"""
    let instance = new Nested.Schemas.WithArrayOptional(items = Some ["A";"B"])
    json |> Nested.Schemas.WithArrayOptional.Parse |> fun x -> x.ToJToken() |> compareJson json
    instance.ToJToken() |> compareJson json
    Assert.AreEqual("A", instance.Items.Value.[0])
    Assert.AreEqual("B", instance.Items.Value.[1])

[<Test>]
let ``Works with nested optional array simple schema (with nulls)``() =
    let jsonWithNull = """{"items":["A","B"],"name":null}"""
    let instance = new Nested.Schemas.WithArrayOptional(items = Some ["A";"B"])
    jsonWithNull |> Nested.Schemas.WithArrayOptional.Parse |> fun x -> x.ToJToken() |> compareJsonWithNulls jsonWithNull
    instance.ToJToken() |> compareJsonWithNulls jsonWithNull
    Assert.AreEqual("A", instance.Items.Value.[0])
    Assert.AreEqual("B", instance.Items.Value.[1])

[<Test>]
let ``Nested optional array simple schema - not provided``() =
    let json = """{"name":"A"}"""
    let instance = new Nested.Schemas.WithArrayOptional(name = Some "A")
    //failwithf "HERE %A" (instance.ToJToken().ToString())
    let jt = instance.ToJToken()
    jt.ToString()
    let settings = new JsonSerializerSettings()
    settings.NullValueHandling <- NullValueHandling.Ignore
    settings.TypeNameHandling <- TypeNameHandling.All
    failwithf "%A" <| Newtonsoft.Json.JsonConvert.SerializeObject(jt, settings)

    instance.ToJToken() |> compareJson json
    json |> Nested.Schemas.WithArrayOptional.Parse |> fun x -> x.ToJToken() |> compareJson json
    Assert.AreEqual("A", instance.Name.Value)

[<Test>]
let ``Nested optional array simple schema - not provided (with nulls)``() =
    let jsonWithNull = """{"items":null,"name":"A"}"""
    let instance = new Nested.Schemas.WithArrayOptional(name = Some "A")
    instance.ToJToken() |> compareJsonWithNulls jsonWithNull
    jsonWithNull |> Nested.Schemas.WithArrayOptional.Parse |> fun x -> x.ToJToken() |> compareJsonWithNulls jsonWithNull
    Assert.AreEqual("A", instance.Name.Value)