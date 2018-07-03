namespace OpenAPITypeProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types
open Newtonsoft.Json

[<TypeProvider>]
type OpenAPITypeProviderImplementation (cfg : TypeProviderConfig) as this =
   inherit TypeProviderForNamespaces (cfg)

   do System.AppDomain.CurrentDomain.add_AssemblyResolve(fun _ args ->
        let expectedName = (AssemblyName(args.Name)).Name + ".dll"
        let asmPath = 
            cfg.ReferencedAssemblies
            |> Seq.tryFind (fun asmPath -> System.IO.Path.GetFileName(asmPath) = expectedName)
        match asmPath with
        | Some f -> Assembly.LoadFrom f
        | None -> null)
    
   let ns = "OpenAPIProvider"
   let asm = Assembly.GetExecutingAssembly()
    
   let tp = ProvidedTypeDefinition(asm, ns, "OpenAPIV3Provider", None,  hideObjectMethods = true, nonNullable = true, isErased = true)
    
   let createTypes typeName (args:obj[]) =
       let filePath = args.[0] :?> string
       let dateFormatString = args.[1] :?> string
       let dateTimeZoneHandling = args.[2] :?> DateTimeZoneHandling
             
       Json.Serialization.settings.DateFormatString <- dateFormatString
       Json.Serialization.settings.DateTimeZoneHandling <- dateTimeZoneHandling
       
       let ctx = { Assembly = asm; Namespace = ns }

       filePath 
       |> OpenAPITypeProvider.IO.getFilename cfg
       |> Document.createType ctx typeName
    
   let parameters = [ 
         ProvidedStaticParameter("FilePath", typeof<string>)
         ProvidedStaticParameter("DateFormatString", typeof<string>, parameterDefaultValue = "yyyy-MM-ddTHH:mm:ss.fffZ") 
         ProvidedStaticParameter("DateTimeZoneHandling", typeof<DateTimeZoneHandling>, parameterDefaultValue = DateTimeZoneHandling.Utc) 
       ]
        
   do tp.DefineStaticParameters(parameters, createTypes)
   do this.AddNamespace(ns, [tp])

[<assembly:TypeProviderAssembly()>]
do ()