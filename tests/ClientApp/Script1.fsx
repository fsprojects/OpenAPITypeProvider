#r @"../../src/OpenAPITypeProvider/bin/Debug/net461/OpenAPITypeProvider.dll"

open OpenAPIProvider

type Provider = OpenAPIV3Provider<"abcd">

let x = Provider()
x.MyPropertya
Provider.MyStatic


//Provider.MyProperty
// Type `MyType.MyProperty` on next line dow
