module OpenAPITypeProviderImplementation

open System
open System.Collections.Generic
open System.IO
open System.Reflection
open FSharp.Quotations
open FSharp.Core.CompilerServices
open ProviderImplementation
open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider.Types

[<TypeProvider>]
type OpenAPITypeProviderImplementation (cfg : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces (cfg, assemblyReplacementMap=[("OpenAPITypeProvider.DesignTime", "OpenAPITypeProvider.Runtime")], addDefaultProbingLocation=true)

    let ns = "OpenAPITypeProvider"
    let asm = Assembly.GetExecutingAssembly()
    
    let tp = ProvidedTypeDefinition(asm, ns, "OpenAPIV3Provider", (Some typeof<obj>),  hideObjectMethods = true, nonNullable = true, isErased = true)

    let createTypes typeName (args:obj[]) =
        try
           let ctx = { Assembly = asm; Namespace = ns }
           args.[0] :?> string 
           |> OpenAPITypeProvider.IO.getFilename cfg 
           |> Document.createType ctx typeName
        with ex -> ex |> sprintf "%A" |> failwith

    let parameters = [ 
        ProvidedStaticParameter("FilePath", typeof<string>)
    ]

    let helpText = 
        """<summary>OpenAPI Version 3 Type Provider</summary>
           <param name='FilePath'>Location of a YAML or JSON file with specification.</param>
        """

    do tp.AddXmlDoc helpText
    do tp.DefineStaticParameters(parameters, createTypes) 
    do this.AddNamespace(ns, [tp])

[<assembly:TypeProviderAssembly()>]
do ()
