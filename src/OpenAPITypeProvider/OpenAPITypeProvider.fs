namespace OpenAPITypeProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types


[<TypeProvider>]
type OpenAPITypeProviderImplementation (cfg : TypeProviderConfig) as this =
   inherit TypeProviderForNamespaces (cfg, assemblyReplacementMap=[ "OpenAPITypeProvider", "OpenAPIProvider" ])

   let ns = "OpenAPIProvider"
   let asm = Assembly.GetExecutingAssembly()
    
   let tp = ProvidedTypeDefinition(asm, ns, "OpenAPIV3Provider", None,  hideObjectMethods = true, nonNullable = true, isErased = true)
    
   let createTypes typeName (args:obj[]) =
       let ctx = { Assembly = asm; Namespace = ns }
       let filePath = args.[0] :?> string
       filePath 
       |> OpenAPITypeProvider.IO.getFilename cfg
       |> Document.createType ctx typeName
    
   let parameters = [ 
         ProvidedStaticParameter("FilePath", typeof<string>)
       ]
        
   do tp.DefineStaticParameters(parameters, createTypes)
   do this.AddNamespace(ns, [tp])

[<assembly:TypeProviderAssembly()>]
do ()