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
x.ToJToken() |> string
x.ToJToken() |> string

let parsed = new Provider.Schemas.NestedOne("TEST", new Provider.Schemas.NestedOne.SubValue(Some 36))


parsed.Name
parsed.Numbers
parsed.SubValue.Age

parsed.ToJToken() |> string

let a = Provider.Schemas.MyObject.Parse(""" {"name":"Roman","age":123} """)
a.Name
a.Age
a.ToJToken() |> string

let sub = new Provider.Schemas.NestedOne.SubValue(Some 36)
sub.Age

let y = new Provider.Schemas.NestedOne("abc", sub)
y.Name
y.SubValue.Age

let ab = y.SubValue
ab.Age

