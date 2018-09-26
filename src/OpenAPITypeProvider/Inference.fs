module internal OpenAPITypeProvider.Inference

open System
open OpenAPIParser.Version3.Specification

let rec getComplexType (getSchemaFun: Schema -> Type) schema = 
    match schema with
    | Boolean -> typeof<bool>
    | Integer IntFormat.Int32 -> typeof<int>
    | Integer IntFormat.Int64 -> typeof<int64>
    | Number NumberFormat.Float -> typeof<float32>
    | Number NumberFormat.Double -> typeof<double>
    | String StringFormat.String 
    | String StringFormat.Password 
    | String StringFormat.Binary -> typeof<string>
    | String StringFormat.Byte -> typeof<byte>
    | String StringFormat.Date 
    | String StringFormat.DateTime -> typeof<DateTime>
    | String StringFormat.UUID -> typeof<Guid>
    | String (StringFormat.Enum _) -> schema |> getSchemaFun
    | Array schema -> 
        let typ = schema |> getComplexType getSchemaFun
        typedefof<List<_>>.MakeGenericType([|typ|])
    | Object _ -> schema |> getSchemaFun