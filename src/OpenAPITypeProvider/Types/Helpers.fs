module OpenAPITypeProvider.Types.Helpers

open ProviderImplementation.ProvidedTypes
open System.Reflection

let inline (|>!) x fn = x |> fn |> ignore; x

let inline makeConstructor parameters invokeCode = ProvidedConstructor(parameters, invokeCode)
let inline makeProperty< ^T> getterCode propName = ProvidedProperty(propName, typeof< ^T>, getterCode)
let inline makeTypedProperty getterCode propName typ = ProvidedProperty(propName, typ, getterCode)
let inline makeMethod< ^T> parameters invokeCode methodName = ProvidedMethod(methodName, parameters, typeof< ^T>, invokeCode)
let inline makeParameter< ^T> paramName = ProvidedParameter(paramName, typeof< ^T>)
let inline addXmlDocDelayed comment providedMember = (^a : (member AddXmlDocDelayed : (unit -> string) -> unit) providedMember, (fun () -> comment))
let inline addEmptyConstructor comment (t:ProvidedTypeDefinition) = 
    makeConstructor [] (fun _ -> <@@ () @@>)
    |>! addXmlDocDelayed comment
    |> t.AddMember

let inline addAsProperty name comment (t:ProvidedTypeDefinition) (prop:ProvidedTypeDefinition)  = 
    prop |> t.AddMember
    ProvidedProperty(name, prop, (fun _ -> <@@ obj() @@>))
    |>! addXmlDocDelayed comment
    |> t.AddMember