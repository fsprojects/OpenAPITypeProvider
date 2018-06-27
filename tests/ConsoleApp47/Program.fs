open System
open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"..\Sample.yaml">


[<EntryPoint>]
let main argv =
    
    let x = Provider.Schemas.Error(123, "AHOJ")
    x.ToJToken().ToString() |> printfn "%A"
    Console.ReadKey() |> ignore
    0 // return an integer exit code