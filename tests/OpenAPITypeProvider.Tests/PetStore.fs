module OpenAPITypeProvider.Tests.PetStore

open NUnit.Framework
open OpenAPIProvider
open OpenAPITypeProvider.Tests.Shared

type PetStore = OpenAPIV3Provider<"Samples/PetStore.yaml">

[<Test>]
let ``Empty schema``() =
    let json = "{}"
    let instance = PetStore.Schemas.Empty()
    instance |> compareJson json
    json |> PetStore.Schemas.Empty.Parse |> compareJson json

[<Test>]
let ``NewPet schema``() =
    let json = """{"name":"Name","tag":"TAG"}"""
    let instance = new PetStore.Schemas.NewPet(name = "Name", tag = Some "TAG")
    instance |> compareJson json
    json |> PetStore.Schemas.NewPet.Parse |> compareJson json
    Assert.AreEqual("Name", instance.Name)
    Assert.AreEqual(Some "TAG", instance.Tag)

[<Test>]
let ``Error schema``() =
    let json = """{"code":123,"message":"Msg"}"""
    let instance = new PetStore.Schemas.Error (code = 123, message = "Msg")
    instance |> compareJson json
    json |> PetStore.Schemas.Error.Parse |> compareJson json
    Assert.AreEqual(123, instance.Code)
    Assert.AreEqual("Msg", instance.Message)
