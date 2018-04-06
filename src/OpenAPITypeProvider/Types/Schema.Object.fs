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

let private createProperty isOptional originalName (schema:Schema) existingObjects =
    let name = Names.pascalName originalName
    let typ = schema |> Inference.getComplexType existingObjects |> (fun x -> if isOptional then x |> asOption else x)
    ProvidedProperty(name, typ, (fun args -> 
        let t = args.[0]
        <@@  
            let objectValue = ((%%t : obj) :?> ObjectValue)
            objectValue.GetValue(originalName)
        @@>))

let private getNameForSubArrayObject (name:string) = Names.pascalName(name + "Item")

let rec private createType ctx (parent:ProvidedTypeDefinition) isOptional existingTypes name schema =
    match schema with
    | Schema.Object (props, required) ->
        let isOptional n = required |> List.contains n |> not
        let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
        
        // create properties & sub-objects
        let existingTypes = 
            props
            |> Map.map (fun n s -> createType ctx typ (isOptional n) existingTypes n s)
            |> Map.toList
            |> List.collect snd
        
        // constructor
        let getCtorParam (name,typ) =
            if name |> isOptional then ProvidedParameter(name, asOption typ, false, None) else ProvidedParameter(name, typ)

        let ctorParams = 
            props 
            |> Map.toList 
            |> List.map (fun (n,s) -> n, s |> Inference.getComplexType existingTypes)
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
            |> typ.AddMember

        // ToJToken method
        ProvidedMethod("ToJToken", [], typeof<Newtonsoft.Json.Linq.JToken>, (fun args -> 
            let t = args.[0]
            <@@ 
                let objectValue = ((%%t : obj ):?> ObjectValue)
                objectValue.ToJToken()
            @@>))
            |> typ.AddMember

        typ |> parent.AddMember
        (schema, typ) :: existingTypes
    | Schema.Array arraySchema ->
        match arraySchema with
        | Schema.Object _ ->
            let existingTypes = createType ctx parent isOptional existingTypes (getNameForSubArrayObject name) arraySchema
            createProperty isOptional name schema existingTypes |> parent.AddMember
            existingTypes
        | _ -> 
            createProperty isOptional name schema existingTypes |> parent.AddMember
            existingTypes

    | _ ->
        createProperty isOptional name (schema:Schema) existingTypes |> parent.AddMember
        existingTypes

let createTypes ctx parent name schema = 
    match schema with
    | Array s ->
        match s with
        | Object _ -> createType ctx parent false [] name s
        | _ -> []
    | Object _ -> createType ctx parent false [] name schema
    | _ -> []