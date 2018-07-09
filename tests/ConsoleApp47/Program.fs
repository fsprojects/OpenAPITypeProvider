open System
open OpenAPIProvider


type Provider = OpenAPIV3Provider< FilePath = @"..\Sample.yaml", DateTimeZoneHandling = Common.DateTimeZoneHandling.Utc >

let openApi = new Provider()

[<EntryPoint>]
let main argv =
    
    //let x = new Provider.Schemas.``Input as application/json``(DateTime.Now)

    let d = Provider.RequestBodies.Input.``application/json``.Parse("{'name':'2018-12-31 12:34:56'}").Name
    let x = new Provider.Schemas.``Direct as application/json``(None, None)
    //let x = Provider.Schemas.``Direct as application/json``.Parse("{}")

    //let x = Provider.Schemas.Error(123, "AHOJ")
    x.ToJToken().ToString() |> printfn "%A"
    Console.ReadKey() |> ignore
    0 // return an integer exit code