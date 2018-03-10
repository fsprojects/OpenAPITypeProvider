namespace ProviderImplementation

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types


[<TypeProvider>]
type OpenAPITypeProviderImplementation (cfg : TypeProviderConfig) as this =
   inherit TypeProviderForNamespaces (cfg)

   let ns = "OpenAPIProvider"
   let asm = Assembly.GetExecutingAssembly()
    
   let tp = ProvidedTypeDefinition(asm, ns, "OpenAPIV3Provider", None,  hideObjectMethods = true, nonNullable = true, isErased = true)
    
   let createTypes typeName (args:obj[]) =
       let filePath = args.[0] :?> string
       let resolutionFolder = args.[1] :?> string
       filePath 
       |> OpenAPITypeProvider.IO.mkAbsoluteFileName cfg resolutionFolder
       |> Document.createType asm ns typeName
    
   let parameters = [ 
         ProvidedStaticParameter("FilePath", typeof<string>)
         ProvidedStaticParameter("ResolutionFolder", typeof<string>, parameterDefaultValue = "")
       ]
        
   do tp.DefineStaticParameters(parameters, createTypes)
   do this.AddNamespace(ns, [tp])

[<assembly:TypeProviderAssembly()>]
do ()