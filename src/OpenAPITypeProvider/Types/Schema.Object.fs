module OpenAPITypeProvider.Types.Schema.Object

open ProviderImplementation.ProvidedTypes
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open System
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations

let private asOption (typ:#Type) = typedefof<option<_>>.MakeGenericType typ

let private createProperty isOptional originalName (schema:Schema) (getSchemaFun:string -> Schema -> ProvidedTypeDefinition) =
    let name = Names.pascalName originalName
    let typ = schema |> Inference.getComplexType (getSchemaFun name) |> (fun x -> if isOptional then x |> asOption else x)
    ProvidedProperty(name, typ, (fun args -> 
        let t = args.[0]
        <@@  
            let objectValue = ((%%t : obj) :?> ObjectValue)
            objectValue.GetValue(originalName)
        @@>))

let rec private createType ctx isOptional (getSchemaFun:string -> Schema -> ProvidedTypeDefinition) name schema =
    match schema with
    | Schema.Object (props, required) ->
        let isOptional n = required |> List.contains n |> not
        let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
        
        // create properties & sub-objects
        props
        |> Map.map (fun n s -> createProperty (isOptional n) n s getSchemaFun)
        |> Map.iter (fun _ v -> typ.AddMember(v))
        
        // constructor
        let getCtorParam (name,typ) =
            if name |> isOptional then ProvidedParameter(name, asOption typ, false, None) else ProvidedParameter(name, typ)

        let ctorParams = 
            props 
            |> Map.toList 
            |> List.map (fun (n,s) -> n, s |> Inference.getComplexType (getSchemaFun n))
            |> List.sortBy (fun (n,_) -> isOptional n)
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
        let strSchema = schema |> Serialization.serialize
        ProvidedMethod("Parse", [ProvidedParameter("json", typeof<string>)], typ, (fun args -> 
            <@@ 
                let json = %%args.Head : string
                ObjectValue.Parse(json, strSchema)
            @@>), isStatic = true)
            |> (fun x -> x.AddXmlDoc "Parses JSON string to strongly typed schema"; x)
            |> typ.AddMember

        // ToJToken method
        ProvidedMethod("ToJToken", [], typeof<Newtonsoft.Json.Linq.JToken>, (fun args -> 
            let t = args.[0]
            <@@ 
                let objectValue = ((%%t : obj ):?> ObjectValue)
                objectValue.ToJToken()
            @@>))
            |> (fun x -> x.AddXmlDoc "Converts strongly typed schema to JToken"; x)
            |> typ.AddMember

        typ
    | _ -> failwith "Please, report this error as Github issue - this should not happen!"

let rec createTypes ctx getSchemaFun name schema = 
    match schema with
    | Object _ -> createType ctx false getSchemaFun name schema
    | _ -> failwith "This function should be called only for Object schema"