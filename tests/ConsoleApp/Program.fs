open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"Sample.yaml ">

[<EntryPoint>]
let main argv = 
    let item = Provider.Schemas.Error(123, "ABCD")
    item.ToJToken() |> string |>printfn "%A"
    System.Console.ReadLine() |> ignore
    0
