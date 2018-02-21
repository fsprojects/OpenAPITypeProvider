module OpenAPITypeProvider.Design

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

[<TypeProvider>]
type OpenAPIProvider (cfg : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let ns = "OpenAPIProvider"
    let asm = Assembly.GetExecutingAssembly()


    let myType = ProvidedTypeDefinition(asm, ns, "OpenAPIV3Provider", None)

    let createTypes typeName (args:obj[]) =
        let subType = ProvidedTypeDefinition(asm, ns, typeName, None)
        let ctor = ProvidedConstructor(List.empty, InvokeCode = fun [] -> <@@ () @@>)

        let filePath = args.[0] :?> string
        
        filePath.ToCharArray() |> Array.iter (fun i -> 
            let myProp = ProvidedProperty("MyProperty"+i.ToString(), typeof<string>,
                                        GetterCode = (fun args -> <@@ sprintf "Hello my %c %s" i filePath @@>))
            subType.AddMember(myProp)
        )


        let myProp2 = ProvidedProperty("MyStatic", typeof<string>, IsStatic = true, GetterCode = (fun _ -> <@@ "HIII" @@>))
        subType.AddMember(ctor)
        //subType.AddMember(myProp)
        
        subType.AddMember(myProp2)
        subType
    
    let parameters = 
        [ ProvidedStaticParameter("FilePath", typeof<string>) ]

    do myType.DefineStaticParameters(parameters, createTypes)
    do this.AddNamespace(ns, [myType])

[<assembly:TypeProviderAssembly>]
do ()