module OpenAPITypeProvider.Tests.PetStore

open NUnit.Framework
open OpenAPIProvider
open OpenAPITypeProvider.Tests.Shared
open System

type PetStoreWithDate = OpenAPIV3Provider<"Samples/PetStore.yaml", "dd. MM. yyyy HH:mm:ss">
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

[<Test>]
let ``Two dates schema``() =
    let json = """{"date1":"31. 12. 2018 12:34:56","date2":"31. 12. 2017 12:34:56"}"""
    let parsed = PetStoreWithDate.Schemas.TwoDates.Parse(json)
    Assert.AreEqual(DateTime(2018,12,31,12,34,56, DateTimeKind.Local), parsed.Date1)
    Assert.AreEqual(DateTime(2017,12,31,12,34,56, DateTimeKind.Local), parsed.Date2)

[<Test>]
let ``Two dates schema default``() =
    let json = """{"date1":"2018-12-31T12:34:56","date2":"2017-12-31T12:34:56"}"""
    let parsed = PetStoreWithDate.Schemas.TwoDates.Parse(json)
    Assert.AreEqual(DateTime(2018,12,31,12,34,56, DateTimeKind.Local), parsed.Date1)
    Assert.AreEqual(DateTime(2017,12,31,12,34,56, DateTimeKind.Local), parsed.Date2)

// [<Test>]
// let ``Two dates schema``() =
    
//     let json = """{"date1":"2018-12-31T12:34:56Z","date2":"2018-12-31T12:34:56.0"}"""
//     let instance = 
//         new PetStore.Schemas.TwoDates (
//             date1 = DateTime(2018,12,31,12,34,56, DateTimeKind.Local), 
//             date2 = DateTime(2018,12,31,12,34,56, DateTimeKind.Local)
//         )
//     let str = instance.ToJToken() |> (fun x -> Newtonsoft.Json.JsonConvert.SerializeObject(x))
//     Assert.AreEqual(json, str)

    //instance |> compareJson json
    //json |> PetStore.Schemas.TwoDates.Parse |> compareJson json
    //Assert.AreEqual(123, instance.Code)
    //Assert.AreEqual("Msg", instance.Message)
