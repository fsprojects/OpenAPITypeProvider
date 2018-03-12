module OpenAPITypeProvider.Tests.JsonParser.JsonParser

open System
open NUnit.Framework
open OpenAPITypeProvider.Json.Parser
open OpenAPITypeProvider.Tests
open Newtonsoft.Json.Linq
open OpenAPITypeProvider.Specification

[<Test>]
let ``Parses Integer (Int32) from JValue``() = 
    let value = 123 |> JValue
    let schema = Schema.Integer (IntFormat.Int32)
    let expected = 123
    Assert.AreEqual(expected, (parseForSchema schema value))

[<Test>]
let ``Parses Integer (Int64) from JValue``() = 
    let value = 123L |> JValue
    let schema = Schema.Integer (IntFormat.Int64)
    let expected = 123L
    Assert.AreEqual(expected, (parseForSchema schema value))

[<Test>]
let ``Parses String (String) from JValue``() = 
    let value = "abc" |> JValue
    let schema = Schema.String (StringFormat.String)
    let expected = "abc"
    Assert.AreEqual(expected, (parseForSchema schema value))

[<Test>]
let ``Parses String (Byte) from JValue``() = 
    let value = "64" |> JValue
    let schema = Schema.String (StringFormat.Byte)
    let expected = 64uy
    Assert.AreEqual(expected, (parseForSchema schema value))

[<Test>]
let ``Parses String (Date) from JValue``() = 
    let value = "2018-01-02" |> JValue
    let schema = Schema.String (StringFormat.Date)
    let expected = new DateTime(2018, 01, 02)
    Assert.AreEqual(expected, (parseForSchema schema value))

[<Test>]
let ``Parses String (DateTime) from JValue``() = 
    let value = "2018-01-02" |> JValue
    let schema = Schema.String (StringFormat.DateTime)
    let expected = new DateTime(2018, 01, 02)
    Assert.AreEqual(expected, (parseForSchema schema value))

[<Test>]
let ``Parses Number (Float) from JValue``() = 
    let value = "1.2" |> JValue
    let schema = Schema.Number (NumberFormat.Float)
    let expected = 1.2f
    Assert.AreEqual(expected, (parseForSchema schema value))

[<Test>]
let ``Parses Number (Double) from JValue``() = 
    let value = "1.2" |> JValue
    let schema = Schema.Number (NumberFormat.Double)
    let expected = 1.2
    Assert.AreEqual(expected, (parseForSchema schema value))

[<Test>]
let ``Parses Array (Float) from JArray``() = 
    let jArray = "[1.2]" |> JToken.Parse
    let schema = Schema.Array (Schema.Number NumberFormat.Float)
    let expected = [| 1.2f |]
    Assert.AreEqual(expected, (parseForSchema schema jArray))

[<Test>]
let ``Parses Array (Double) from JArray``() = 
    let jArray = "[1.2]" |> JToken.Parse
    let schema = Schema.Array (Schema.Number NumberFormat.Double)
    let expected = [ 1.2 ]
    Assert.AreEqual(expected, (parseForSchema schema jArray))

[<Test>]
let ``Parses Boolean from JValue``() = 
    let value = true |> JValue
    let schema = Schema.Boolean
    let expected = true
    Assert.AreEqual(expected, (parseForSchema schema value))

[<Test>]
let ``Parses Object from JObject``() = 
    let jObject = JObject()
    jObject.["name"] <- "abc" |> JValue
    jObject.["age"] <- 123 |> JValue

    let props = 
        [ 
            ("name", Schema.String (StringFormat.String))
            ("age", Schema.Integer (IntFormat.Int32))
        ] |> Map

    let schema = Schema.Object (props, ["name"])
    let expected = 
        [
            ("name", "abc" :> obj)
            ("age", 123 :> obj)
        ] |> Map
    Assert.AreEqual(expected, (parseForSchema schema jObject))