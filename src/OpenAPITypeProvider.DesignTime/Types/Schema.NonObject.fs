module internal OpenAPITypeProvider.Types.Schema.NonObject

open OpenAPITypeProvider
open ProviderImplementation.ProvidedTypes
open OpenAPIParser.Version3.Specification
open OpenAPITypeProvider.Types
open Microsoft.FSharp.Quotations

let getName name schema = 
    let def = schema |> Extract.getSchemaDefinition
    match def with
    | Array subS ->
        let subDef = subS |> Extract.getSchemaDefinition
        match subDef with
        | Object _ -> name + "_Item"
        | _ -> name
    | _ -> name

let private createNonObjectType ctx (findOrCreateSchema:string -> Schema -> SchemaType) name (schema:Schema) =
    let def = schema |> Extract.getSchemaDefinition
    let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<SimpleValue>, hideObjectMethods = true, nonNullable = true, isErased = true)
    let itemsName = getName name schema
    let schemaType = schema |> Inference.getComplexType (findOrCreateSchema itemsName >> SchemaType.GetType)
    let strSchema = def |> Serialization.serialize
    
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
            SimpleValue.Parse(json, strSchema)
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
        match def with
        | SchemaDefinition.Array _ -> "Values"
        | _ -> "Value"

    ProvidedProperty(valueMethodName, schemaType, (fun args -> 
        let t = args.[0]
        <@@  
            let simpleValue = (%%t : SimpleValue)
            simpleValue.GetValue
        @@>)) |> typ.AddMember

    typ

let createTypes ctx getSchemaFun name schema =
    let def = schema |> Extract.getSchemaDefinition
    match def with
    | SchemaDefinition.Object _ -> failwith "This function should be called only for non-Object schema"
    | SchemaDefinition.Boolean 
    | SchemaDefinition.Array _
    | SchemaDefinition.Integer _
    | SchemaDefinition.Number _
    | SchemaDefinition.String _ -> createNonObjectType ctx getSchemaFun name schema