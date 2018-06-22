// Learn more about F# at http://fsharp.org

open System
open OpenAPIProvider

type Provider = OpenAPIV3Provider< @"Sample.yaml">


[<EntryPoint>]
let main argv =
    

    printfn "Hello World from F#!"
    0 // return an integer exit code
