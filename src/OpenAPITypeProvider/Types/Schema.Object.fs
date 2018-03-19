module OpenAPITypeProvider.Types.Schema.Object

open ProviderImplementation.ProvidedTypes
open System.Reflection
open OpenAPITypeProvider
open OpenAPITypeProvider.Specification
open System
open OpenAPITypeProvider.Types
open OpenAPITypeProvider.Json
open OpenAPITypeProvider.Json.Types
open Microsoft.FSharp.Quotations

let asOption (typ:#Type) = typedefof<option<_>>.MakeGenericType typ

let createProperty isOptional originalName (schema:Schema) existingObjects =
    let name = Names.pascalName originalName
    let typ = schema |> Inference.getType (Map existingObjects) |> (fun x -> if isOptional then x |> asOption else x)
    ProvidedProperty(name, typ, (fun args -> 
        let t = args.[0]
        <@@  
            let objectValue = ((%%t : obj) :?> ObjectValue)
            objectValue.GetValue(originalName)
        @@>))

let rec createObject ctx originalName (schema:Schema) existingObjects =
    let name = Names.pascalName originalName
    match schema with
    | Schema.Object (props, required) ->
        let isOptional n = required |> List.contains n |> not
        
        let typ = ProvidedTypeDefinition(ctx.Assembly, ctx.Namespace, name, Some typeof<obj>, hideObjectMethods = true, nonNullable = true, isErased = true)
        
        let foldFn acc n s =
            match s with
            | Object _ ->
                let newType = createObject ctx n s acc
                typ.AddMember(newType)
                ProvidedProperty(Names.pascalName n, newType, fun args -> 
                    <@@  
                        let objectValue = ((%%args.[0] : obj) :?> ObjectValue)
                        objectValue.GetValue(n)
                    @@>) |> typ.AddMember
                (s, newType) :: acc
            | _ ->
                createProperty (isOptional n) n s acc |> typ.AddMember
                acc
            
        // properties
        let existingObjects = props |> Map.fold foldFn existingObjects
        
        // constructor
        let getCtorParam (name,typ) =
            if name |> isOptional then
                ProvidedParameter(name, (typ |> asOption), false, null)
            else ProvidedParameter(name, typ)

        let ctorParams = 
            props 
            |> Map.toList 
            |> List.map (fun (n,s) -> n, s |> Inference.getType (Map existingObjects))
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
                ObjectValue(json, strSchema, Map.empty)
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

        typ
    
    | _ -> failwith "YOLO"
        

let tryCreateType ctx name schema =
    match schema with
    | Schema.Object _ -> createObject ctx name schema [] |> Some
    | _ -> None
