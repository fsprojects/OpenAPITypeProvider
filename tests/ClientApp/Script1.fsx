#r @"../../src/OpenAPITypeProvider/bin/Debug/netstandard2.0/System.Dynamic.Runtime.dll"
#r @"../../src/OpenAPITypeProvider/bin/Debug/netstandard2.0/System.Linq.Expressions.dll"
#r @"../../src/OpenAPITypeProvider/bin/Debug/netstandard2.0/netstandard.dll"
#r @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\packages\Newtonsoft.Json\lib\netstandard2.0\Newtonsoft.Json.dll"
#r @"../../src/OpenAPITypeProvider/bin/Debug/netstandard2.0/OpenAPITypeProvider.dll"

open OpenAPIProvider
open Newtonsoft.Json.Linq

type Provider = OpenAPIV3Provider< @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\ClientApp\sample.yaml">

let p = Provider()

let x = Provider.Schemas.JustIntArray.Parse(""" [123] """)

x.Values |> fun y -> y.Length

let y = x.ToJToken()

y.ToString()



