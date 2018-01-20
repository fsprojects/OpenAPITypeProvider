namespace OpenAPITypeProvider

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

type Discriminator = {
    PropertyName : string
    Mapping : Map<string, Schema>
}
and 
    Schema = {
        Type : string
        // TODO: OTHERS
        Items : Schema option
        Properties : Schema option
        AllOf : Schema list
        OneOf : Schema list
        AnyOf : Schema list
        Discriminator : Discriminator
    }

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

type OpenAPI = {
    SpecificationVersion : string
    Info : Info
    Paths : Path list
    //Servers : TBD LATER (maybe :))
}