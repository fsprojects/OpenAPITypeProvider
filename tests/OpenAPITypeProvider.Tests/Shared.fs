module OpenAPITypeProvider.Tests.Shared

open Newtonsoft.Json
open Newtonsoft.Json.Linq
open NUnit.Framework

let toString (x:JToken) = x.ToString(Formatting.None)
let inline getJToken x = (^T:(member ToJToken: unit -> JToken)x)
let inline compareJson (json:string) ob =
    let j = ob |> getJToken |> toString
    Assert.AreEqual(json, j)