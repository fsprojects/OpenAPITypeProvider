module internal OpenAPITypeProvider.Inference

open System
open OpenAPIParser.Version3.Specification

let private getIntType = function
    | IntFormat.Int32 -> typeof<int>
    | IntFormat.Int64 -> typeof<int64>

let private getNumberType = function
    | NumberFormat.Float -> typeof<float32>
    | NumberFormat.Double -> typeof<double>

let private getStringType = function
    | StringFormat.String | StringFormat.Password | StringFormat.Binary -> typeof<string>
    | StringFormat.Byte -> typeof<byte>
    | StringFormat.Date | StringFormat.DateTime -> typeof<DateTime>
    | StringFormat.UUID -> typeof<Guid>

let rec getComplexType (getSchemaFun: Schema -> Type) schema = 
    match schema with
    | Boolean -> typeof<bool>
    | Integer format -> format |> getIntType
    | Number format -> format |> getNumberType
    | String format -> format |> getStringType
    | Array schema -> 
        let typ = schema |> getComplexType getSchemaFun
        typedefof<List<_>>.MakeGenericType([|typ|])
    | Object _ -> schema |> getSchemaFun