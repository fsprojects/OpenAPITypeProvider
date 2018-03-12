﻿#r @"../../src/OpenAPITypeProvider/bin/Debug/netstandard2.0/OpenAPITypeProvider.dll"
#r @"../../src/OpenAPITypeProvider/bin/Debug/netstandard2.0/netstandard.dll"
#r @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\packages\Newtonsoft.Json\lib\netstandard2.0\Newtonsoft.Json.dll"

open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\ClientApp\sample.yaml">

let p = Provider()

let x = Provider.Schemas.JustIntArray.Parse(""" [123] """)

x.Values
x.ToJToken()
//let y = Provider.Schemas.PlainStringValue.PlainStringValue.Parse(""" 'abc' """)


