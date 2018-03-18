#r @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\src\OpenAPITypeProvider\bin\Debug\Newtonsoft.Json.dll"
#r @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\src\OpenAPITypeProvider\bin\Debug\OpenAPITypeProvider.dll"

open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\Scripting\Sample.yaml">

let json =
    """
    {
        "name":"ABC",
        "subValue": {"age":123456}
    }
    """

let parsed = Provider.Schemas.NestedOne.Parse(json)

let sub = new Provider.Schemas.NestedOne.SubValue(Some 36)
sub.Age

let y = new Provider.Schemas.NestedOne("abc", sub)
y.Name
y.SubValue.Age

let ab = y.SubValue
ab.Age

