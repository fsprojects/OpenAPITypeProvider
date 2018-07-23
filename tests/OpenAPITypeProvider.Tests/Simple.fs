module OpenAPITypeProvider.Tests.Simple

open NUnit.Framework
open OpenAPIProvider
open OpenAPITypeProvider.Tests.Shared

type Simple = OpenAPIV3Provider<"Samples/Simple.yaml">

[<Test>]
let ``String schema``() =
    let json = "\"ABC\""
    let instance = Simple.Schemas.SimpleString("ABC")
    instance.ToJToken() |> compareJson json
    json |> Simple.Schemas.SimpleString.Parse |> fun x -> x.ToJToken() |> compareJson json
    Assert.AreEqual("ABC", instance.Value)

[<Test>]
let ``String array``() =
    let json = """["A","B"]"""
    let instance = Simple.Schemas.SimpleArray(["A";"B"])
    instance.ToJToken() |> compareJson json
    json |> Simple.Schemas.SimpleArray.Parse |> fun x -> x.ToJToken() |> compareJson json
    Assert.AreEqual(["A";"B"], instance.Values)

[<Test>]
let ``String array2``() =
    let json = """[1,2]"""
    let instance = Simple.Schemas.SimpleArray2([1;2])
    instance.ToJToken() |> compareJson json
    json |> Simple.Schemas.SimpleArray2.Parse |> fun x -> x.ToJToken() |> compareJson json
    Assert.AreEqual([1;2], instance.Values)

[<Test>]
let ``String array3``() =
    let json = """[{"name":"N"}]"""
    let ob = Simple.Schemas.MyObj("N")
    let instance = Simple.Schemas.SimpleArray3([ob])
    instance.ToJToken() |> compareJson json
    json |> Simple.Schemas.SimpleArray3.Parse |> fun x -> x.ToJToken() |> compareJson json
    Assert.AreEqual("N", instance.Values.[0].Name)
