module internal OpenAPITypeProvider.Types.Schema.Object

open ProviderImplementation.ProvidedTypes
open OpenAPIParser.Version3.Specification
open System
open OpenAPITypeProvider.Types
open OpenAPITypeProvider
open Microsoft.FSharp.Quotations

let private asOption (typ:#Type) = typedefof<option<_>>.MakeGenericType typ

let private createProperty isOptional originalName (schema:Schema) (findOrCreateSchema:Schema -> SchemaType) =
    let typ = schema |> Inference.getComplexType (findOrCreateSchema >> SchemaType.GetType) |> (fun x -> if isOptional then x |> asOption else x)
    let name = originalName |> Names.pascalName
    ProvidedProperty(name, typ, (fun args -> 
        let t = args.[0]
        <@@  
            let objectValue = ((%%t : ObjectValue))
            objectValue.GetValue(originalName)
        @@>))

let rec private createType ctx (findOrCreateSchema:string -> Schema -> SchemaType) name def =
    match def with
    | SchemaDefinition.Object (props, required) ->
        let isOptional n = required |> List.contains n |> not
        let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<ObjectValue>, hideObjectMethods = true, nonNullable = true, isErased = true)
        
        // create properties & sub-objects
        let properties =
            props
            |> Map.map (fun n s -> 
                let newPropTypeName = sprintf "%s_%s" name (Names.pascalName n)
                createProperty (isOptional n) n s (findOrCreateSchema newPropTypeName)
            )
        
        // add properties to type
        properties |> Map.iter (fun _ v -> typ.AddMember(v))
        
        // constructor
        let getCtorParam (name,typ:ProvidedProperty) =
            if name |> isOptional then ProvidedParameter(name, typ.PropertyType, false, None) else ProvidedParameter(name, typ.PropertyType)

        let sortFn (x:string,_) (y:string,_) = 
            match x |> isOptional, y |> isOptional with
            | true, true | false, false -> x.CompareTo(y)
            | true, false -> 1
            | false, true -> -1

        let ctorParams = 
            properties 
            |> Map.toList 
            |> List.sortWith sortFn
            |> List.map getCtorParam 
        
        ProvidedConstructor(ctorParams, (fun args -> 
            let values =
                Expr.NewArray(typeof<string * obj>,
                    args
                    |> Seq.toList
                    |> List.mapi(fun i v -> Expr.NewTuple [ Expr.Value ctorParams.[i].Name ; Expr.Coerce(v, typeof<obj>) ] ))
            <@@  
            let v = (%%values : (string * obj) array) |> Array.toList 
            ObjectValue(v)
            @@>)) |> typ.AddMember

        // static method Parse
        let strSchema = def |> Serialization.serialize
        ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>)], typ, (fun args -> 
            let format = Parser.defaultDateFormat
            <@@ 
                let json = %%args.Head : string
                ObjectValue.Parse(json, strSchema, format)
            @@>), isStatic = true)
            |> (fun x -> x.AddXmlDoc "Parses JSON string to strongly typed schema"; x)
            |> typ.AddMember
        
        // static method Parse
        ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>);ProvidedParameter("dateFormatString", typeof<string>)], typ, (fun args -> 
            <@@ 
                let json = %%args.[0] : string
                let format = %%args.[1] : string
                ObjectValue.Parse(json, strSchema, format)
            @@>), isStatic = true)
            |> (fun x -> x.AddXmlDoc "Parses JSON string to strongly typed schema with custom DateFormatString"; x)
            |> typ.AddMember

        
        typ
    | _ -> failwith "Please, report this error as Github issue - this should not happen!"

let rec createTypes ctx getSchemaFun name schema = 
    let def = schema |> Extract.getSchemaDefinition
    match def with
    | Object _ -> createType ctx getSchemaFun name def
    | _ -> failwith "This function should be called only for Object schema"