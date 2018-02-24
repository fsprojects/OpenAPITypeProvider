
#r @"../../src/OpenAPITypeProvider/bin/Debug/OpenAPITypeProvider.dll"


open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\ClientApp\sample.yaml">

let x = Provider()
x.Version


//Provider.MyProperty
// Type `MyType.MyProperty` on next line dow
