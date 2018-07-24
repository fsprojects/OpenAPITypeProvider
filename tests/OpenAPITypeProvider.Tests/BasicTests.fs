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

[<Test>]
let ``Parses and converts basic schema with two dates properties (custom format with time zone transform)``() =
    let customSettings = new JsonSerializerSettings()
    customSettings.DateTimeZoneHandling <- DateTimeZoneHandling.Utc
    let json = """{"date1":"2018-12-31T11:34:56Z","date2":"2017-12-31T11:34:56Z"}"""
    let d1 = DateTime(2018,12,31,12,34,56, DateTimeKind.Local)
    let d2 = DateTime(2017,12,31,12,34,56, DateTimeKind.Local)
    let instance = new PetStore.Schemas.TwoDates(d1, d2)
    Assert.AreEqual(json, instance.ToJson(customSettings, Formatting.None))

[<Test>]
let ``Parses and converts nested array schema``() =
    let json = """{"items":["A","B"]}"""
    let instance = new PetStore.Schemas.WithArray(items = ["A";"B"])
    let parsed = PetStore.Schemas.WithArray.Parse json
    Assert.AreEqual(json, instance.ToJson(Formatting.None))
    Assert.AreEqual(json, parsed.ToJson(Formatting.None))
    Assert.AreEqual("A", instance.Items.[0])
    Assert.AreEqual("B", instance.Items.[1])

[<Test>]
let ``Parses and converts nested optional array schema``() =
    let json = """{"items":["A","B"]}"""
    let instance = new PetStore.Schemas.WithArrayOptional(items = Some ["A";"B"])
    let parsed = PetStore.Schemas.WithArrayOptional.Parse json
    Assert.AreEqual(json, instance.ToJson(Formatting.None))
    Assert.AreEqual(json, parsed.ToJson(Formatting.None))
    Assert.AreEqual("A", instance.Items.Value.[0])
    Assert.AreEqual("B", instance.Items.Value.[1])

[<Test>]
let ``Parses and converts nested optional array schema (with nulls)``() =
    let jsonToParse = """{"items":["A","B"],"name":null}"""
    let json = """{"items":["A","B"]}"""
    let instance = new PetStore.Schemas.WithArrayOptional(items = Some ["A";"B"])
    let parsed = PetStore.Schemas.WithArrayOptional.Parse jsonToParse
    Assert.AreEqual(json, instance.ToJson(Formatting.None))
    Assert.AreEqual(json, parsed.ToJson(Formatting.None))
    Assert.AreEqual("A", instance.Items.Value.[0])
    Assert.AreEqual("B", instance.Items.Value.[1])

[<Test>]
let ``Parses and converts nested optional array schema (with nulls in array)``() =
    let jsonToParse = """{"items":null,"name":"Roman"}"""
    let json = """{"name":"Roman"}"""
    let instance = new PetStore.Schemas.WithArrayOptional(name = Some "Roman")
    let parsed = PetStore.Schemas.WithArrayOptional.Parse jsonToParse
    Assert.AreEqual(json, instance.ToJson(Formatting.None))
    Assert.AreEqual(json, parsed.ToJson(Formatting.None))
    Assert.AreEqual("Roman", instance.Name.Value)
    Assert.AreEqual("Roman", parsed.Name.Value)

[<Test>]
let ``Parses and converts simple value schema (string)``() =
    let json = "\"ABC\""
    let instance = PetStore.Schemas.SimpleString("ABC")
    let parsed = PetStore.Schemas.SimpleString.Parse json
    Assert.AreEqual(json, instance.ToJson(Formatting.None))
    Assert.AreEqual(json, parsed.ToJson(Formatting.None))
    Assert.AreEqual("ABC", instance.Value)

[<Test>]
let ``Parses and converts simple value schema (string array)``() =
    let json = """["A","B"]"""
    let instance = PetStore.Schemas.SimpleArray(["A";"B"])
    let parsed = PetStore.Schemas.SimpleArray.Parse json
    Assert.AreEqual(json, instance.ToJson(Formatting.None))
    Assert.AreEqual(json, parsed.ToJson(Formatting.None))
    Assert.AreEqual(["A";"B"], instance.Values)
    Assert.AreEqual("A", instance.Values.Head)

[<Test>]
let ``Parses and converts simple value schema (int array)``() =
    let json = """[1,2]"""
    let instance = PetStore.Schemas.SimpleArray2([1;2])
    let parsed = PetStore.Schemas.SimpleArray2.Parse json
    Assert.AreEqual(json, instance.ToJson(Formatting.None))
    Assert.AreEqual(json, parsed.ToJson(Formatting.None))
    Assert.AreEqual([1;2], instance.Values)
    Assert.AreEqual(1, instance.Values.Head)

[<Test>]
let ``Parses and converts simple value schema (obj array)``() =
    let json = """[{"code":123,"message":"Roman"}]"""
    let ob = PetStore.Schemas.Error(123, "Roman")
    let instance = PetStore.Schemas.SimpleArray3([ob])
    let parsed = PetStore.Schemas.SimpleArray3.Parse json
    Assert.AreEqual(json, instance.ToJson(Formatting.None))
    Assert.AreEqual(json, parsed.ToJson(Formatting.None))
    Assert.AreEqual([ob], instance.Values)
    Assert.AreEqual("Roman", instance.Values.[0].Message)

[<Test>]
let ``Parses and converts simple value schema (date)``() =
    let customSettings = new JsonSerializerSettings()
    customSettings.DateFormatString <- customDateFormat
    let d1 = DateTime(2018,12,31,12,34,56, DateTimeKind.Local)
    let json = "\"31. 12. 2018 12:34:56\""
    let instance = PetStore.Schemas.SimpleDate(d1)
    let parsed = PetStore.Schemas.SimpleDate.Parse(json, customDateFormat)
    Assert.AreEqual(json, instance.ToJson(customSettings, Formatting.None))
    Assert.AreEqual(json, parsed.ToJson(customSettings, Formatting.None))
    Assert.AreEqual(d1, instance.Value)

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

//[<Test>]
//let ``Path methods works``() =
//    let petStoreAPI = new PetStore()
//    let instance = PetStore.Schemas.Pet(1L, "A")
//    let json = """{"id":1,"name":"A"}"""
//    Assert.AreEqual(json, petStoreAPI.Paths.``/pets/{id}``.Get.Responses.``200``.``application/json``.ToJson(instance, Formatting.None))