module OpenAPITypeProvider.IO

open System
open System.IO
open Microsoft.FSharp.Core.CompilerServices

let makeAbsolute resolutionFolderParam pathName = 
    if String.IsNullOrWhiteSpace pathName then pathName 
    elif Path.IsPathRooted pathName then Path.GetFullPath pathName
    else Path.GetFullPath(Path.Combine(resolutionFolderParam, pathName))

let getAbsoluteDesignTimeDirectory (cfg:TypeProviderConfig) resolutionFolderParam =  
    if String.IsNullOrWhiteSpace resolutionFolderParam then 
        cfg.ResolutionFolder
    else 
        makeAbsolute cfg.ResolutionFolder resolutionFolderParam

let makeAbsoluteWithResolutionFolder cfg resolutionFolderParam pathName = 
    let absoluteDesignTimeDirectory = getAbsoluteDesignTimeDirectory cfg resolutionFolderParam
    makeAbsolute absoluteDesignTimeDirectory pathName


let mkDefaultRuntimeResolutionFolder (cfg:TypeProviderConfig) resolutionFolderParam =  
    if cfg.IsHostedExecution then 
        getAbsoluteDesignTimeDirectory cfg resolutionFolderParam
    else
        System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/')

let mkAbsoluteFileName cfg resolutionFolderParam staticFileName =  
    if Path.IsPathRooted staticFileName then 
        staticFileName
    else
        let baseDirectory = mkDefaultRuntimeResolutionFolder cfg resolutionFolderParam
        System.IO.Path.Combine([| baseDirectory; staticFileName |])