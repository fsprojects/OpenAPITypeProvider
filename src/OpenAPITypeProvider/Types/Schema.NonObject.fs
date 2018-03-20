module OpenAPITypeProvider.Types.Schema.NonObject

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations

let private createNonObjectType ctx existingTypes name (schema:Schema) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
    let schemaType = schema |> Inference.getType existingTypes
    let strSchema = schema |> Serialization.serialize
    // constructor
    ProvidedConstructor([ProvidedParameter("value", schemaType)], (fun args ->
        let value = Expr.Coerce(args.Head, typeof<obj>)
        <@@
            SimpleValue(%%value)
        @@>)) |> typ.AddMember

    // static method Parse
    ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>)], typ, (fun args -> 
        <@@ 
            let json = %%args.Head : string
            SimpleValue(json, strSchema)
        @@>), isStatic = true)
        |> typ.AddMember
        
    // Value(s) property
    let valueMethodName = 
        match schema with
        | Schema.Array _ -> "Values"
        | _ -> "Value"

    ProvidedProperty(valueMethodName, schemaType, (fun args -> 
        let t = args.[0]
        <@@  
            let simpleValue = ((%%t : obj) :?> SimpleValue)
            simpleValue.Value
        @@>)) |> typ.AddMember

    // ToJToken method
    ProvidedMethod("ToJToken", [], typeof<Newtonsoft.Json.Linq.JToken>, (fun args -> 
        let t = args.[0]
        <@@ 
            let simpleValue = ((%%t : obj ):?> SimpleValue)
            simpleValue.ToJToken()
        @@>))
        |> typ.AddMember
    typ


let tryCreateType ctx existingTypes name schema =
    match schema with
    | Schema.Object _ -> None
    | Schema.Boolean 
    | Schema.Array _
    | Schema.Integer _
    | Schema.Number _
    | Schema.String _ -> schema |> createNonObjectType ctx existingTypes name |> Some