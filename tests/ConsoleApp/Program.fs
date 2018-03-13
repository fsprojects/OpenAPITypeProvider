// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\ConsoleApp\Sample.yaml">

let p = Provider()

let x = Provider.Schemas.JustArray.Parse("""['a', 'b']""")


[<EntryPoint>]
let main argv = 
    printfn "%A" x.Values
    printfn "%A" <| x.ToJToken().ToString(Newtonsoft.Json.Formatting.None)
    
    System.Console.ReadLine() |> ignore
    0
