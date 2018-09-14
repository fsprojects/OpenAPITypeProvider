namespace OpenAPITypeProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types
open System.IO

[<TypeProvider>]
type OpenAPITypeProviderImplementation (cfg : TypeProviderConfig) as this =
   inherit TypeProviderForNamespaces (cfg)

   do System.AppDomain.CurrentDomain.add_AssemblyResolve(fun _ args ->
        let expectedName = (AssemblyName(args.Name)).Name + ".dll"
        let asmPath = 
            cfg.ReferencedAssemblies
            |> Seq.tryFind (fun asmPath -> Path.GetFileName(asmPath) = expectedName)
        match asmPath with
        | Some f -> Assembly.LoadFrom f
        | None -> null)
    
   let ns = "OpenAPITypeProvider"
   let asm = Assembly.GetExecutingAssembly()
    
   let tp = ProvidedTypeDefinition(asm, ns, "OpenAPIV3Provider", None,  hideObjectMethods = true, nonNullable = true, isErased = true)
    
   let createTypes typeName filePath =
       try
           let ctx = { Assembly = asm; Namespace = ns }
           filePath |> Document.createType ctx typeName
        with ex -> ex |> sprintf "%A" |> failwith
    
   let parameters = [ 
         ProvidedStaticParameter("FilePath", typeof<string>)
       ]
   
   let helpText = 
        """<summary>OpenAPI Version 3 Type Provider</summary>
           <param name='FilePath'>Location of a YAML or JSON file with specification.</param>
        """
  
   do tp.AddXmlDoc helpText
   do tp.DefineStaticParameters(parameters, (fun typeName args -> 
       let filePath = args.[0] :?> string |> OpenAPITypeProvider.IO.getFilename cfg 
       let dirToWatch = filePath |> Path.GetDirectoryName
       let createTypesFn = (fun _ -> createTypes typeName filePath |> ignore)

       use watcher = new FileSystemWatcher()
       watcher.Path <- dirToWatch
       watcher.EnableRaisingEvents <- true
       watcher.IncludeSubdirectories <- true
       watcher.Changed.Add createTypesFn
       watcher.Created.Add createTypesFn
       watcher.Deleted.Add createTypesFn
       
       createTypes typeName filePath
   ))
   do this.AddNamespace(ns, [tp])

[<assembly:TypeProviderAssembly()>]
do ()