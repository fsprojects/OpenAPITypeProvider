#r @"../../src/OpenAPITypeProvider/bin/Debug/net461/OpenAPITypeProvider.dll"


open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\ClientApp\sample.yaml">

let x = Provider()

x.Components.Schemas.NewPet.someArray


//Provider.MyProperty
// Type `MyType.MyProperty` on next line dow
