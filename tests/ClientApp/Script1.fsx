#r @"../../src/OpenAPITypeProvider/bin/Debug/netstandard2.0/OpenAPITypeProvider.dll"
//#r @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\packages\Newtonsoft.Json\lib\netstandard2.0\Newtonsoft.Json.dll"
//#r @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\packages\YamlDotNet\lib\netstandard1.3\YamlDotNet.dll"

open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\ClientApp\sample.yaml">

let x = Provider()

let xy = new Provider.Components.Schemas.NewPet(Name = "", SomeArray = None, Tag = "")
Provider.Components.Schemas.

//x.Components.Schemas.


//Provider.MyProperty
// Type `MyType.MyProperty` on next line dow
