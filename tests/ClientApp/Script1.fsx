#r @"../../src/OpenAPITypeProvider/bin/Debug/net461/OpenAPITypeProvider.dll"

open OpenAPIProvider

type Provider = OpenAPIV3Provider<FilePath="test">

//Provider.MyProperty
// Type `MyType.MyProperty` on next line dow
