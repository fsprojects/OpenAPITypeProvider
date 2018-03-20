#r @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\src\OpenAPITypeProvider\bin\Debug\Newtonsoft.Json.dll"
#r @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\src\OpenAPITypeProvider\bin\Debug\OpenAPITypeProvider.dll"

open System
open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\Scripting\Sample.yaml">

let provider = new Provider()

provider.Path.``/pets``.Get.



let o = Provider.Schemas.MyObject.Parse(""" { "date": "2018-03-20T08:34:32.6913454Z" }""")
o.Date
o.Name
o.ToJToken() |> string

let item = new Provider.Schemas.ObjectArrayItem(Some "AAAA")
let items = new Provider.Schemas.ObjectArray([item])
items.ToJToken() |> string


let p = Provider.Schemas.ObjectArrayItem.Parse("{'name':'JO'}")
p.Name
p.ToJToken() |> string

item.Name
item.ToJToken() |> string
items.Values |> List.map (fun x -> x.Name)

let parsed = Provider.Schemas.ObjectArray.Parse(""" [{'name':'JO'}] """)
parsed.ToJToken() |> string
parsed.Values.Length


parsed.Values


let a = Provider.Schemas.SimpleArray.Parse("[1]")
a.ToJToken() |> string

let json =
    """
    {
        "name":"ABC",
        "subValue": {"age":123456}
    }
    """

let x = Provider.Schemas.NestedOne.Parse(json)
let y = Provider.Schemas.SubValue.Parse(""" {"age":123456} """)
y.Age
y.ToJToken() |> string

x.Name
x.Numbers
x.SubValue.Age
x.SubValue.ToJToken() |> string
x.ToJToken() |> string
