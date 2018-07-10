module OpenAPITypeProvider.Tests.PetStore

open NUnit.Framework
open OpenAPIProvider
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type OpenAPI = OpenAPIV3Provider<"Samples/PetStore.yaml">

let private toString (x:JToken) = x.ToString(Formatting.None)
let inline private getJToken x = (^T:(member ToJToken: unit -> JToken)x)
let inline private compareJson (json:string) ob =
    let j = ob |> getJToken |> toString
    Assert.AreEqual(json, j)

[<Test>]
let ``Empty schema``() =
    let json = "{}"
    let instance = OpenAPI.Schemas.Empty()
    instance |> compareJson json
    json |> OpenAPI.Schemas.Empty.Parse |> compareJson json

[<Test>]
let ``NewPet schema``() =
    let json = """{"name":"Name","tag":"TAG"}"""
    let instance = new OpenAPI.Schemas.NewPet(name = "Name", tag = Some "TAG")
    instance |> compareJson json
    json |> OpenAPI.Schemas.NewPet.Parse |> compareJson json
    Assert.AreEqual("Name", instance.Name)
    Assert.AreEqual(Some "TAG", instance.Tag)
