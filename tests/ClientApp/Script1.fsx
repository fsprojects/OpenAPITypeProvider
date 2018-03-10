// #r @"../../src/OpenAPITypeProvider/bin/Debug/netstandard2.0/OpenAPITypeProvider.dll"
// #r @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\packages\Newtonsoft.Json\lib\netstandard2.0\Newtonsoft.Json.dll"
// #r @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\packages\YamlDotNet\lib\netstandard1.3\YamlDotNet.dll"
#r @"../../src/OpenAPITypeProvider/bin/Debug/net461/OpenAPITypeProvider.dll"

open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\ClientApp\sample.yaml">

let p = Provider()
 
let x = Provider.Schemas.JustIntValue.ParseTest("aaa")
x.RawValue
