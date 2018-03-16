#r @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\src\OpenAPITypeProvider\bin\Debug\Newtonsoft.Json.dll"
#r @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\src\OpenAPITypeProvider\bin\Debug\OpenAPITypeProvider.dll"

open OpenAPIProvider

//type Provider = OpenAPIV3Provider< @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\ConsoleApp\Sample.yaml">
type Provider = OpenAPIV3Provider< @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\Scripting\Sample.yaml">

let y = Provider.Schemas.Error.Parse(""" {"code":36, "message":"AHOJ"} """)
y.Code
y.Message

let x = Provider.Schemas.NestedOne.Parse(""" { "name":"ahoj", "subValue":{"age":123}} """)
x.Name
x.SubValue




let p = Provider()
let x = Provider.Schemas.SimpleStringValue.Parse("'abc'")
x.Value