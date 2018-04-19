module OpenAPITypeProvider.IO

open System
open System.IO
open Microsoft.FSharp.Core.CompilerServices

let getFilename (cfg:TypeProviderConfig) (staticFileName:string) = 
    let staticFileName = staticFileName.Trim()
    if Path.IsPathRooted staticFileName then Path.GetFullPath staticFileName
    else System.IO.Path.Combine([| cfg.ResolutionFolder; staticFileName |])