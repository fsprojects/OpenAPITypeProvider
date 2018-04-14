// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open OpenAPIProvider

//type Provider = OpenAPIV3Provider< @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\ConsoleApp\Sample.yaml">
type Provider = OpenAPIV3Provider< @"Sample.yaml">


[<EntryPoint>]
let main argv = 
    
    let item = Provider.Schemas.Error(123, "ABC")
    item.ToJToken() |> string |>printfn "%A"

    System.Console.ReadLine() |> ignore
    0
