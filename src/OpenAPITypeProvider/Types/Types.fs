namespace OpenAPITypeProvider.Types

open System.Reflection

type Context = {
    Assembly : Assembly
    Namespace : string
}