namespace OpenAPITypeProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open OpenAPITypeProvider.Types
open OpenAPIProvider.Common

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
       let dateTimeZoneHandling = args.[1] :?> OpenAPIProvider.Common.DateTimeZoneHandling
       let dateFormatString = args.[2] :?> string
       
       let toNewtonsoft (value:DateTimeZoneHandling) =
            match value with
            | DateTimeZoneHandling.Local -> Newtonsoft.Json.DateTimeZoneHandling.Local
            | DateTimeZoneHandling.Utc -> Newtonsoft.Json.DateTimeZoneHandling.Utc
            | DateTimeZoneHandling.Unspecified -> Newtonsoft.Json.DateTimeZoneHandling.Unspecified
            | DateTimeZoneHandling.RoundtripKind | _ -> Newtonsoft.Json.DateTimeZoneHandling.RoundtripKind

       Json.Serialization.settings.DateFormatString <- dateFormatString
       Json.Serialization.settings.DateTimeZoneHandling <- dateTimeZoneHandling |> toNewtonsoft
       
       let ctx = { Assembly = asm; Namespace = ns }

       filePath 
       |> OpenAPITypeProvider.IO.getFilename cfg
       |> Document.createType ctx typeName
    
   let parameters = [ 
         ProvidedStaticParameter("FilePath", typeof<string>)
         ProvidedStaticParameter("DateTimeZoneHandling", typeof<OpenAPIProvider.Common.DateTimeZoneHandling>, parameterDefaultValue = OpenAPIProvider.Common.DateTimeZoneHandling.Local) 
         ProvidedStaticParameter("DateFormatString", typeof<string>, parameterDefaultValue = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK") 
       ]
   
   let helpText = 
        """<summary>OpenAPI Version 3 Type Provider</summary>
           <param name='FilePath'>Location of a YAML file with specification.</param>
           <param name='DateTimeZoneHandling'>Specifies how to treat the time value when converting between string and DateTime. 1:1 to Newtonsoft DateTimeZoneHandling enumeration.</param>
           <param name='DateFormatString'>Specifies format for (de)serialization of DateTime. Default is 'yyyy-MM-ddTHH:mm:ss.FFFFFFFK'.</param>
           """
  
   do tp.AddXmlDoc helpText
   do tp.DefineStaticParameters(parameters, createTypes)
   do this.AddNamespace(ns, [tp])

[<assembly:TypeProviderAssembly()>]
do ()