namespace OpenAPITypeProvider.Types

open System
open System.Reflection
open ProviderImplementation.ProvidedTypes

type Context = {
    Assembly : Assembly
    Namespace : string
}

type SchemaType = {
    Name : string
    Type : ProvidedTypeDefinition
}
with
    static member GetType schema = schema.Type :> Type