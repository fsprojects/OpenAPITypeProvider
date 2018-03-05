module OpenAPITypeProvider.Tests.JsonParser.JsonParser

open NUnit.Framework
open OpenAPITypeProvider.JsonParser.JsonParser
open OpenAPITypeProvider.Tests
open Newtonsoft.Json.Linq
open OpenAPITypeProvider.Specification

[<Test>]
let ``Integer (Int32) is compatible with JValue``() = 
    let value = 123 |> JValue
    let schema = Schema.Integer (IntFormat.Int32)
    Assert.IsTrue(isSchemaAndJTokenCompatible value schema)

[<Test>]
let ``Integer (Int64) is compatible with JValue``() = 
    let value = 123L |> JValue
    let schema = Schema.Integer (IntFormat.Int64)
    Assert.IsTrue(isSchemaAndJTokenCompatible value schema)

[<Test>]
let ``String (String) is compatible with JValue``() = 
    let value = "abc" |> JValue
    let schema = Schema.String (StringFormat.String)
    Assert.IsTrue(isSchemaAndJTokenCompatible value schema)

[<Test>]
let ``String (Byte) is compatible with JValue``() = 
    let value = "64" |> JValue
    let schema = Schema.String (StringFormat.Byte)
    Assert.IsTrue(isSchemaAndJTokenCompatible value schema)

[<Test>]
let ``String (Date) is compatible with JValue``() = 
    let value = "2018-01-01" |> JValue
    let schema = Schema.String (StringFormat.Date)
    Assert.IsTrue(isSchemaAndJTokenCompatible value schema)