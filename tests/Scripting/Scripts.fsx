#r @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\src\OpenAPITypeProvider\bin\Debug\Newtonsoft.Json.dll"
#r @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\src\OpenAPITypeProvider\bin\Debug\OpenAPITypeProvider.dll"

open OpenAPIProvider

//type Provider = OpenAPIV3Provider< @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\ConsoleApp\Sample.yaml">
type Provider = OpenAPIV3Provider< @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\Scripting\Sample.yaml">

let y = Provider.Schemas.Pet(Id = 1L, Name = "TEST")
y.Name
y.SomeArray
y.Tag.IsSome
y.Id

let p = Provider()
let x = Provider.Schemas.SimpleStringValue.Parse("'abc'")
x.Value