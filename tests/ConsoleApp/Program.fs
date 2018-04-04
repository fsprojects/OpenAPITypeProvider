// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open OpenAPIProvider

//type Provider = OpenAPIV3Provider< @"C:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\ConsoleApp\Sample.yaml">
type Provider = OpenAPIV3Provider< @"c:\Dzoukr\Personal\dzoukr\OpenAPITypeProvider\tests\Scripting\Sample.yaml">


[<EntryPoint>]
let main argv = 
    
    let item = new Provider.Schemas.ObjectArrayItem(Some "AAAA")
    let p = Provider.Schemas.ObjectArrayItem.Parse("{'name':'JO'}")
    p.Name |> printfn "%A"
    p.ToJToken() |> string |>printfn "%A"

    item.Name |> printfn "%A"
    item.ToJToken() |> string |> printfn "%A"
    let items = new Provider.Schemas.ObjectArray([item;item])

    items.Values |> List.map (fun x -> x.ToJToken() |> string) |> printfn "%A"
    items.ToJToken() |> string |> printfn "%A"

    System.Console.ReadLine() |> ignore
    0
