// Learn more about F# at http://fsharp.org

open System
open OpenAPIProvider
open Newtonsoft.Json

type Provider = OpenAPIV3Provider<  @"..\Sample.yaml">


[<EntryPoint>]
let main argv =
    
    let x = Provider.RequestBodies.Input.``application/json``.Parse("")// .Schemas.Error(123, "AHOJ")
    x.ToJToken().ToString() |> printfn "%A"
    Console.ReadKey() |> ignore
    0 // return an integer exit code
