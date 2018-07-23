namespace OpenAPITypeProvider.Types

open System
open System.Reflection
open ProviderImplementation.ProvidedTypes

type internal Context = {
    Assembly : Assembly
    Namespace : string
    DateFormatString : string
}

type internal SchemaType = {
    Name : string
    Type : ProvidedTypeDefinition
}
with
    static member GetType schema = schema.Type :> Type