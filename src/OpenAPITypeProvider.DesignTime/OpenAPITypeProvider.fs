namespace OpenAPITypeProvider

open System
open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types


[<TypeProvider>]
type public OpenAPITypeProvider (cfg : TypeProviderConfig) as this =
   inherit TypeProviderForNamespaces (cfg)

   //static do
   //   AppDomain.CurrentDomain.add_AssemblyResolve(fun source args ->
   //     OpenAPITypeProvider.Configuration.resolveReferencedAssembly args.Name)


   let ns = "OpenAPIProvider"
   let asm = Assembly.LoadFrom cfg.RuntimeAssembly //Assembly.GetExecutingAssembly()
    
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
       
   do this.RegisterRuntimeAssemblyLocationAsProbingFolder cfg
   do tp.DefineStaticParameters(parameters, createTypes)
   do this.AddNamespace(ns, [tp])
