namespace OpenAPITypeProvider.Specification

open System

type Contact = {
    Name : string option
    Url : Uri option
    Email : string option
}

type License = {
    Name : string
    Url : Uri option
}

type Info = {
    Title : string
    Description : string option
    TermsOfService : Uri option
    Contact : Contact option
    License : License option
    Version : string
}

type IntFormat =
    | Int32
    | Int64
    with static member Default = Int32

type NumberFormat =
    | Float
    | Double
    with static member Default = Float

type StringFormat =
    | String
    | Byte
    | Binary
    | Date
    | DateTime
    | Password
    with static member Default = String

type Schema =
    | Array of items:Schema
    | Object of props:Map<string, Schema> * required:string list
    | Boolean
    | Integer of format:IntFormat
    | Number of format:NumberFormat
    | String of format:StringFormat
    | AllOf of schemaList:Schema list
    // AnyOf: TBD LATER (maybe :))
    // OneOf: TBD LATER (maybe :))

type MediaType  = {
    Schema: Schema
    //Example:  TBD LATER (maybe :))
    //Examples: TBD LATER (maybe :))
    //Encoding: TBD LATER (maybe :))
}

type Parameter = {
    Name : string
    In : string
    Description : string option
    Required : bool // false
    Deprecated : bool // false
    AllowEmptyValue : bool //false
    Schema : Schema
    Content : Map<string, MediaType>
    //Style:            TBD LATER (maybe :))
    //Explode:          TBD LATER (maybe :))
    //AllowReserved:    TBD LATER (maybe :))
    //Example:          TBD LATER (maybe :))
    //Examples:         TBD LATER (maybe :))
}

type Header = { // follow structure of Parameter
    Description : string option
    Required : bool // false
    Deprecated : bool // false
    AllowEmptyValue : bool //false
    Schema : Schema
}

type RequestBody = {
    Description : string option
    Content: Map<string, MediaType>
    Required: bool
}

type Response = {
    Description : string
    Headers : Map<string, Header>
    Content : Map<string, MediaType>
    //Links : TBD LATER (maybe :))
}

type Operation = {
    Tags : string list
    Summary: string option
    Description : string option
    OperationId : string option
    Parameters : Parameter list
    RequestBody : RequestBody option
    Responses : Map<string, Response>
    Deprecated : bool
    //ExternalDocs :    TBD LATER (maybe :))
    //Callbacks :       TBD LATER (maybe :))
    //Security :        TBD LATER (maybe :))
    //Servers :         TBD LATER (maybe :))
}

// can be referenced via $ref
type Path = {
    Summary : string option
    Description : string option
    Get : Operation option
    Put : Operation option
    Post : Operation option
    Delete : Operation option
    Options : Operation option
    Head : Operation option
    Patch : Operation option
    Trace : Operation option
    Parameters : Parameter list
    //Servers : TBD LATER (maybe :))
}

type Components = {
    Schemas : Map<string, Schema>
    Responses : Map<string, Response>
    Parameters : Map<string, Parameter>
    RequestBodies : Map<string, RequestBody>
    Headers : Map<string, Header>
    //Links : Map<string, Link> TBD LATER (maybe :))
    //Callbacks : Map<string, Callback> TBD LATER (maybe :))
    //SecuritySchemes : Map<string, SecurityScheme> TBD LATER (maybe :))
    //Examples : Map<string, Example> TBD LATER (maybe :))
}

type OpenAPI = {
    SpecificationVersion : string
    Info : Info
    Paths : Map<string, Path>
    Components : Components option
    //Security : TBD LATER (maybe :))
    //Tags : TBD LATER (maybe :))
    //ExternalDocs : TBD LATER (maybe :))
    //Servers : TBD LATER (maybe :))
}