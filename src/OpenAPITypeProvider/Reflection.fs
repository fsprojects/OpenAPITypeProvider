module OpenAPITypeProvider.Reflection

open System

type ReflectiveListBuilder = 
    static member BuildList<'a> (args: obj list) = 
        [ for a in args do yield a :?> 'a ] 
    static member BuildTypedList lType (args: obj list) = 
        typeof<ReflectiveListBuilder>
            .GetMethod("BuildList")
            .MakeGenericMethod([|lType|])
            .Invoke(null, [|args|])

let some (typ:Type) arg =
    let unionType = typedefof<option<_>>.MakeGenericType typ
    let meth = unionType.GetMethod("Some")
    meth.Invoke(null, [|arg|])

let getOptionValue (o:obj) =
        match o with
        | null -> null
        | v ->
            match v.GetType().GetProperty("Value") with
            | null -> null
            | prop -> prop.GetValue(o, null )
    
let isOption<'a> (o:obj) =
    match o |> getOptionValue with
    | null -> false
    | v -> v.GetType() = typeof<'a>