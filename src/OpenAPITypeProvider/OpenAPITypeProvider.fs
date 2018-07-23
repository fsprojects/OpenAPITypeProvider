namespace OpenAPITypeProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types

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
    
   let ns = "OpenAPITypeProvider"
   let asm = Assembly.GetExecutingAssembly()
    
   let tp = ProvidedTypeDefinition(asm, ns, "OpenAPIV3Provider", None,  hideObjectMethods = true, nonNullable = true, isErased = true)
    
   let createTypes typeName (args:obj[]) =
       try
           let filePath = args.[0] :?> string
           
           let ctx = { Assembly = asm; Namespace = ns }

           filePath 
           |> OpenAPITypeProvider.IO.getFilename cfg
           |> Document.createType ctx typeName
        with ex -> 
            let rec getMsg list (ex:System.Exception) = 
                match ex.InnerException with
                | null -> (sprintf "Error: %s, StackTrace: %s" ex.Message ex.StackTrace) :: list
                | x -> x |> getMsg list
            ex |> getMsg [] |> List.rev |> String.concat ", " |> failwith
    
   let parameters = [ 
         ProvidedStaticParameter("FilePath", typeof<string>)
       ]
   
   let helpText = 
        """<summary>OpenAPI Version 3 Type Provider</summary>
           <param name='FilePath'>Location of a YAML file with specification.</param>
        """
  
   do tp.AddXmlDoc helpText
   do tp.DefineStaticParameters(parameters, createTypes)
   do this.AddNamespace(ns, [tp])

[<assembly:TypeProviderAssembly()>]
do ()