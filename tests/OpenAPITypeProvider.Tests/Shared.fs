module OpenAPITypeProvider.Tests.Shared

open Newtonsoft.Json
open Newtonsoft.Json.Linq
open NUnit.Framework

let nullSettings = new JsonSerializerSettings()
nullSettings.NullValueHandling <- NullValueHandling.Include

let notNullSettings = new JsonSerializerSettings()
notNullSettings.NullValueHandling <- NullValueHandling.Ignore


let toString (settings:JsonSerializerSettings) (x:JToken) = 
    JsonConvert.SerializeObject(x, Formatting.None, settings)

let compareJson (json:string) (jtoken:JToken) =
    let j = jtoken |> toString notNullSettings
    Assert.AreEqual(json, j)

let compareJsonWithNulls (json:string) (jtoken:JToken) =
    let j = jtoken |> toString nullSettings
    if json <> j then
        failwith (sprintf "TADYAAA: %A = %A" json j)
    Assert.AreEqual(json, j)