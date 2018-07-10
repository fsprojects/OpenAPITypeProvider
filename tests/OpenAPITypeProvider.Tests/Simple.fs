module OpenAPITypeProvider.Tests.Simple

open NUnit.Framework
open OpenAPIProvider
open OpenAPITypeProvider.Tests.Shared

type Simple = OpenAPIV3Provider<"Samples/Simple.yaml">

[<Test>]
let ``String schema``() =
    let json = "\"ABC\""
    let instance = Simple.Schemas.SimpleString("ABC")
    instance |> compareJson json
    json |> Simple.Schemas.SimpleString.Parse |> compareJson json
    Assert.AreEqual("ABC", instance.Value)

[<Test>]
let ``String array``() =
    let json = """["A","B"]"""
    let instance = Simple.Schemas.SimpleArray(["A";"B"])
    instance |> compareJson json
    json |> Simple.Schemas.SimpleArray.Parse |> compareJson json
    Assert.AreEqual(["A";"B"], instance.Values)

[<Test>]
let ``String array2``() =
    let json = """[1,2]"""
    let instance = Simple.Schemas.SimpleArray2([1;2])
    instance |> compareJson json
    json |> Simple.Schemas.SimpleArray2.Parse |> compareJson json
    Assert.AreEqual([1;2], instance.Values)

[<Test>]
let ``String array3``() =
    let json = """[{"name":"N"}]"""
    let ob = Simple.Schemas.MyObj("N")
    let instance = Simple.Schemas.SimpleArray3([ob])
    instance |> compareJson json
    json |> Simple.Schemas.SimpleArray3.Parse |> compareJson json
    Assert.AreEqual("N", instance.Values.[0].Name)
