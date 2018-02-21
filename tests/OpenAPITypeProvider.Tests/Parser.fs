module OpenAPITypeProvider.Tests.Parser

open System
open System.IO
open NUnit.Framework
open YamlDotNet.RepresentationModel
open OpenAPITypeProvider.Parser
open OpenAPITypeProvider.Specification

let parseSample parseFn name =
    let yamlFile = Path.Combine([|AppDomain.CurrentDomain.BaseDirectory; "Samples"; name |]) |> File.ReadAllText
    let reader = new StringReader(yamlFile)
    let yaml = YamlStream()
    yaml.Load(reader)
    yaml.Documents.[0].RootNode :?> YamlMappingNode |> parseFn

let licenseSample = { Name = "MIT"; Url = Some <| Uri("http://github.com/gruntjs/grunt/blob/master/LICENSE-MIT") }
let contactSample = { Name = Some "Swagger API Team"; Email = Some "foo@example.com"; Url = Some <| Uri("http://madskristensen.net") }
let infoSample = { 
    Version = "1.0.0"
    Title = "Swagger Petstore"
    Description = Some "A sample API that uses a petstore as an example to demonstrate features in the OpenAPI 3.0 specification"
    TermsOfService = Some <| Uri ("http://swagger.io/terms/")
    Contact = Some contactSample
    License = Some licenseSample
}


[<Test>]
let ``Parses license``() = 
    let actual = "License.yaml" |> parseSample License.parse
    Assert.AreEqual(licenseSample, actual)

[<Test>]
let ``Parses contact``() = 
    let actual = "Contact.yaml" |> parseSample Contact.parse
    Assert.AreEqual(contactSample, actual)

[<Test>]
let ``Parses info``() = 
    let actual = "Info.yaml" |> parseSample Info.parse
    Assert.AreEqual(infoSample, actual)


[<Test>]
let ``Parses array schema``() = 
    let schemas = "Schema-Array.yaml" |> parseSample Schema.parse
    let expected = IntFormat.Int32 |> Schema.Integer |> Schema.Array
    let actual = schemas.["ArrayInt"]
    Assert.AreEqual(expected, actual)


[<Test>]
let ``Parses int schema``() = 
    let schemas = "Schema-Int.yaml" |> parseSample Schema.parse
    let expected = IntFormat.Int32 |> Schema.Integer
    let actual = schemas.["Int"]
    Assert.AreEqual(expected, actual)

let ``Parses int schema (Int64)``() = 
    let schemas = "Schema-Int.yaml" |> parseSample Schema.parse
    let expected = IntFormat.Int64 |> Schema.Integer
    let actual = schemas.["Int64"]
    Assert.AreEqual(expected, actual)

[<Test>]
let ``Parses string schema``() = 
    let schemas = "Schema-String.yaml" |> parseSample Schema.parse
    let expected = StringFormat.String |> Schema.String
    let actual = schemas.["String"]
    Assert.AreEqual(expected, actual)

[<Test>]
let ``Parses string schema (Binary)``() = 
    let schemas = "Schema-String.yaml" |> parseSample Schema.parse
    let expected = StringFormat.Binary |> Schema.String
    let actual = schemas.["Binary"]
    Assert.AreEqual(expected, actual)

[<Test>]
let ``Parses string schema (Date)``() = 
    let schemas = "Schema-String.yaml" |> parseSample Schema.parse
    let expected = StringFormat.Date |> Schema.String
    let actual = schemas.["Date"]
    Assert.AreEqual(expected, actual)

[<Test>]
let ``Parses string schema (DateTime)``() = 
    let schemas = "Schema-String.yaml" |> parseSample Schema.parse
    let expected = StringFormat.DateTime |> Schema.String
    let actual = schemas.["DateTime"]
    Assert.AreEqual(expected, actual)

[<Test>]
let ``Parses string schema (Password)``() = 
    let schemas = "Schema-String.yaml" |> parseSample Schema.parse
    let expected = StringFormat.Password |> Schema.String
    let actual = schemas.["Password"]
    Assert.AreEqual(expected, actual)

[<Test>]
let ``Parses boolean schema``() = 
    let schemas = "Schema-Boolean.yaml" |> parseSample Schema.parse
    let expected = Schema.Boolean
    let actual = schemas.["Bool"]
    Assert.AreEqual(expected, actual)

[<Test>]
let ``Parses number schema``() = 
    let schemas = "Schema-Number.yaml" |> parseSample Schema.parse
    let expected = NumberFormat.Default |> Schema.Number
    let actual = schemas.["Num"]
    Assert.AreEqual(expected, actual)

[<Test>]
let ``Parses number schema (Float)``() = 
    let schemas = "Schema-Number.yaml" |> parseSample Schema.parse
    let expected = NumberFormat.Float |> Schema.Number
    let actual = schemas.["NumFloat"]
    Assert.AreEqual(expected, actual)

[<Test>]
let ``Parses number schema (Double)``() = 
    let schemas = "Schema-Number.yaml" |> parseSample Schema.parse
    let expected = NumberFormat.Double |> Schema.Number
    let actual = schemas.["NumDouble"]
    Assert.AreEqual(expected, actual)

[<Test>]
let ``Parses object schema``() = 
    let schemas = "Schema-Object.yaml" |> parseSample Schema.parse
    let props =
        [
            "name", Schema.String (StringFormat.Default)
            "age", Schema.Integer (IntFormat.Default)
        ] |> Map
    let expected = Schema.Object(props, ["name"; "age"])
    let actual = schemas.["Basic"]
    Assert.AreEqual(expected, actual)

[<Test>]
let ``Parses object schema (Nested object)``() = 
    let schemas = "Schema-Object.yaml" |> parseSample Schema.parse
    let subObj = 
        Schema.Object 
            (
                (["age", Schema.Integer (IntFormat.Default)] |> Map),
                [])
    let props =
        [
            "name", Schema.String (StringFormat.Default)            
            "subObj", subObj
        ] |> Map
    let expected = Schema.Object(props, ["subObj"])
    let actual = schemas.["ObjectInObject"]
    Assert.AreEqual(expected, actual)

[<Test>]
let ``Parses allOf schema``() = 
    let schemas = "Schema-AllOf.yaml" |> parseSample Schema.parse
    let props =
        [
            "name", Schema.String (StringFormat.Default)
            "age", Schema.Integer (IntFormat.Default)
        ] |> Map
    let expected = Schema.Object(props, ["name"; "age"])
    let actual = schemas.["Extended"]
    Assert.AreEqual(expected, actual)