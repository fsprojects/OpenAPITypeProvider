module internal OpenAPITypeProvider.Types.Schema.NonObject

open OpenAPITypeProvider
open ProviderImplementation.ProvidedTypes
open OpenAPIParser.Version3.Specification
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Json
open Microsoft.FSharp.Quotations

let getName name = function
    | Array subS ->
        match subS with
        | Object _ -> name + "Item"
        | _ -> name
    | _ -> name

let private createNonObjectType ctx getSchemaFun name (schema:Schema) =
    
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<SimpleValue>, hideObjectMethods = true, nonNullable = true, isErased = true)
    let schemaType = schema |> Inference.getComplexType (getSchemaFun (getName name schema) >> SchemaType.GetType)
    let strSchema = schema |> Serialization.serialize
    // constructor
    ProvidedConstructor([ProvidedParameter("value", schemaType)], (fun args ->
        let value = Expr.Coerce(args.Head, typeof<obj>)
        <@@
            SimpleValue(%%value)
        @@>)) |> typ.AddMember

    // static method Parse
    ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>)], typ, (fun args -> 
        let format = Json.Parser.defaultDateFormat
        <@@ 
            let json = %%args.Head : string
            SimpleValue.Parse(json, strSchema, format)
        @@>), isStatic = true)
        |> (fun x -> x.AddXmlDoc "Parses JSON string to strongly typed schema"; x)
        |> typ.AddMember
    
    // static method Parse
    ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>);ProvidedParameter("dateFormatString", typeof<string>)], typ, (fun args -> 
        <@@ 
            let json = %%args.[0] : string
            let format = %%args.[1] : string
            SimpleValue.Parse(json, strSchema, format)
        @@>), isStatic = true)
        |> (fun x -> x.AddXmlDoc "Parses JSON string to strongly typed schema with custom DateFormatString"; x)
        |> typ.AddMember

        
    // Value(s) property
    let valueMethodName = 
        match schema with
        | Schema.Array _ -> "Values"
        | _ -> "Value"

    ProvidedProperty(valueMethodName, schemaType, (fun args -> 
        let t = args.[0]
        <@@  
            let simpleValue = (%%t : SimpleValue)
            simpleValue.GetValue
        @@>)) |> typ.AddMember

    typ

let createTypes ctx getSchemaFun name schema =
    match schema with
    | Schema.Object _ -> failwith "This function should be called only for non-Object schema"
    | Schema.Boolean 
    | Schema.Array _
    | Schema.Integer _
    | Schema.Number _
    | Schema.String _ -> schema |> createNonObjectType ctx getSchemaFun name