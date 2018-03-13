// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open OpenAPIProvider
open Newtonsoft.Json.Linq

type Provider = OpenAPIV3Provider< @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\ClientApp\sample.yaml">

let p = Provider()

let x = Provider.Schemas.JustIntArray.Parse(""" [123] """)
x.Values


[<EntryPoint>]
let main argv = 
    //printfn "%A" (x.ToJToken().ToString())
    0 // return an integer exit code
