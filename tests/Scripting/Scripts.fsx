#r @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\src\OpenAPITypeProvider\bin\Debug\Newtonsoft.Json.dll"
#r @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\src\OpenAPITypeProvider\bin\Debug\OpenAPITypeProvider.dll"

open System
open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\Scripting\Sample.yaml">

let json =
    """
    {
        "name":"ABC",
        "subValue": {"age":123456}
    }
    """

let x = Provider.Schemas.NestedOne.Parse(json)

x.Name
x.Numbers
x.SubValue.Age

x.ToJToken() |> string

let item = new Provider.Schemas.ObjectArrayItem(Some "AAAA")
item.Name
item.ToJToken() |> string
let items = new Provider.Schemas.ObjectArray([item])
items.ToJToken() |> string