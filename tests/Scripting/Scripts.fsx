#r @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\src\OpenAPITypeProvider\bin\Debug\Newtonsoft.Json.dll"
#r @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\src\OpenAPITypeProvider\bin\Debug\OpenAPITypeProvider.dll"

open System
open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"Sample_01.yaml ">
let provider = new Provider()

provider.Paths.``/test``.Post.Responses.``200``.``application/json``.


let x = new Provider.Schemas.Simple01(Some 124, Some "NAME")
x.Id
x.Name

let b = Provider.Schemas.Simple02(["a"])
b.Values

let c = new Provider.Schemas.``application/json``()
c