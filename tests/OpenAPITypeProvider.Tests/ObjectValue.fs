module OpenAPITypeProvider.Tests.ObjectValue

open NUnit.Framework
open OpenAPITypeProvider
open Newtonsoft.Json
open System

type PetStore = OpenAPIV3Provider<"Samples/PetStore.yaml">

let customDateFormat = "dd. MM. yyyy HH:mm:ss"

[<Test>]
let ``Parses and converts empty schema``() =
    let json = "{}"
    let instance = PetStore.Schemas.Empty()
    let parsed = PetStore.Schemas.Empty.Parse json
    Assert.AreEqual(json, instance.ToJson())
    Assert.AreEqual(json, parsed.ToJson())

[<Test>]
let ``Parses and converts basic schema with all required properties``() =
    let json = """{"code":123,"message":"Roman"}"""
    let instance = PetStore.Schemas.Error(123, "Roman")
    let parsed = PetStore.Schemas.Error.Parse json
    Assert.AreEqual(json, instance.ToJson())
    Assert.AreEqual(json, parsed.ToJson())
    Assert.AreEqual(123, parsed.Code)
    Assert.AreEqual(123, instance.Code)
    Assert.AreEqual("Roman", parsed.Message)
    Assert.AreEqual("Roman", instance.Message)

[<Test>]
let ``Parses and converts basic schema with all required properties (with nulls in json)``() =
    let json = """{"code":123,"message":"Roman"}"""
    let jsonToParse = """{"code":123,"message":"Roman","notExisting":null}"""
    let instance = PetStore.Schemas.Error(123, "Roman")
    let parsed = PetStore.Schemas.Error.Parse jsonToParse
    Assert.AreEqual(json, instance.ToJson())
    Assert.AreEqual(json, parsed.ToJson())
    Assert.AreEqual(123, parsed.Code)
    Assert.AreEqual(123, instance.Code)
    Assert.AreEqual("Roman", parsed.Message)
    Assert.AreEqual("Roman", instance.Message)

[<Test>]
let ``Parses and converts basic schema with optional properties``() =
    let json = """{"name":"Name","tag":"TAG"}"""
    let instance = new PetStore.Schemas.NewPet(name = "Name", tag = Some "TAG")
    let parsed = PetStore.Schemas.NewPet.Parse json
    Assert.AreEqual(json, instance.ToJson())
    Assert.AreEqual(json, parsed.ToJson())
    Assert.AreEqual("Name", instance.Name)
    Assert.AreEqual(Some "TAG", instance.Tag)
    Assert.AreEqual("Name", parsed.Name)
    Assert.AreEqual(Some "TAG", parsed.Tag)

[<Test>]
let ``Parses and converts basic schema with optional properties (with nulls in json)``() =
    let json = """{"name":"Name"}"""
    let jsonToParse = """{"name":"Name","tag":null}"""
    let instance = new PetStore.Schemas.NewPet(name = "Name", tag = None)
    let parsed = PetStore.Schemas.NewPet.Parse jsonToParse
    Assert.AreEqual(json, instance.ToJson())
    Assert.AreEqual(json, parsed.ToJson())
    Assert.AreEqual("Name", instance.Name)
    Assert.AreEqual(None, instance.Tag)
    Assert.AreEqual("Name", parsed.Name)
    Assert.AreEqual(None, parsed.Tag)

[<Test>]
let ``Parses and converts basic schema with optional properties (with nulls in json even in response)``() =
    let customSettings = new JsonSerializerSettings()
    customSettings.NullValueHandling <- NullValueHandling.Include
    let json = """{"name":"Name","tag":null}"""
    let instance = new PetStore.Schemas.NewPet(name = "Name", tag = None)
    let parsed = PetStore.Schemas.NewPet.Parse json
    Assert.AreEqual(json, instance.ToJson(customSettings, Formatting.None))
    Assert.AreEqual(json, parsed.ToJson(customSettings, Formatting.None))
    Assert.AreEqual("Name", instance.Name)
    Assert.AreEqual(None, instance.Tag)
    Assert.AreEqual("Name", parsed.Name)
    Assert.AreEqual(None, parsed.Tag)

[<Test>]
let ``Parses and converts basic schema with optional properties (with not present values in json)``() =
    let json = """{"name":"Name"}"""
    let jsonToParse = """{"name":"Name"}"""
    let instance = new PetStore.Schemas.NewPet(name = "Name", tag = None)
    let parsed = PetStore.Schemas.NewPet.Parse jsonToParse
    Assert.AreEqual(json, instance.ToJson())
    Assert.AreEqual(json, parsed.ToJson())
    Assert.AreEqual("Name", instance.Name)
    Assert.AreEqual(None, instance.Tag)
    Assert.AreEqual("Name", parsed.Name)
    Assert.AreEqual(None, parsed.Tag)

[<Test>]
let ``Parses and converts basic schema with two dates properties (custom format)``() =
    let customSettings = new JsonSerializerSettings()
    customSettings.DateFormatString <- customDateFormat
    let json = """{"date1":"31. 12. 2018 12:34:56","date2":"31. 12. 2017 12:34:56"}"""
    let d1 = DateTime(2018,12,31,12,34,56, DateTimeKind.Local)
    let d2 = DateTime(2017,12,31,12,34,56, DateTimeKind.Local)
    let instance = new PetStore.Schemas.TwoDates(d1, d2)
    let parsed = PetStore.Schemas.TwoDates.Parse(json, customDateFormat)
    Assert.AreEqual(json, instance.ToJson(customSettings, Formatting.None))
    Assert.AreEqual(json, parsed.ToJson(customSettings, Formatting.None))

//[<Test>]
//let ``Converts response to JToken``() =
//    let petStoreAPI = new PetStore()
//    let json = """{"id":1,"name":"AHOJ"}"""
//    let instance = PetStore.Schemas.Pet(1L, "AHOJ")
//    let parsed = PetStore.Schemas.Pet.Parse json
//    Assert.AreEqual(json, petStoreAPI.Paths.``/pets``.Post.Responses.``200``.``application/json``.ToJToken(instance).ToString(Formatting.None))
//    Assert.AreEqual(json, petStoreAPI.Paths.``/pets``.Post.Responses.``200``.``application/json``.ToJToken(parsed).ToString(Formatting.None))

// [<Test>]
// let ``Parses request from JSON``() =
//     let petStoreAPI = PetStore()
//     let json = """{"name":"Name"}"""
//     let instance = PetStore.Schemas.NewPet("Name")
//     let parsed = petStoreAPI.Paths.``/pets``.Post.RequestBody.``application/json``.Parse(json)
//     Assert.AreEqual(json, instance.ToJson())
//     Assert.AreEqual(json, parsed.ToJson())
//     Assert.AreEqual("Name", instance.Name)
//     Assert.AreEqual(None, instance.Tag)
//     Assert.AreEqual("Name", parsed.Name)
//     Assert.AreEqual(None, parsed.Tag)
    

[<Test>]
let ``Fails with parsing mismatched types``() =
    Assert.Throws<System.FormatException>(fun _ -> 
        let json = """{"message":123,"code":"Roman"}"""
        PetStore.Schemas.Error.Parse json |> ignore
    ) |> ignore

[<Test>]
let ``Fails with parsing mismatched types 2``() =
    Assert.Throws<System.InvalidCastException>(fun _ -> 
        let json = """{"message":123,"code":{}}"""
        PetStore.Schemas.Error.Parse json |> ignore
    ) |> ignore

[<Test>]
let ``Fails with parsing null in required properties``() =
    Assert.Throws<System.FormatException>(fun _ -> 
        let json = """{"code":123,"message":null}"""
        PetStore.Schemas.Error.Parse json |> ignore
    ) |> ignore
