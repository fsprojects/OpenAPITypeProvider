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

type Schema = {
    Type : string
    // TODO: OTHERS
}

type Parameter = {
    Name : string
    In : string
    Description : string option
    Required : bool option
    Deprecated : bool option
    AllowEmptyValue : bool option
    
    //Style: OMITTED
    //Explode: OMITTED
    //AllowReserved: OMITTED
    
    Schema : Schema

    //Example: OMITTED
    //Examples: OMITTED

    //Content : Map<string, MediaType> : OMITTED
}

type Operation = {
    Tags : string list
    Summary: string option
    Description : string option
    //ExternalDocs : OMMITED
    OperationId : string option
    Parameters : Parameter list
    RequestBody : RequestBody option
    Responses : Response list
    //Callbacks : OMMITED
    Deprecated : bool
    Security : Security option
    //Servers : TBD
}

type Path = {
    //``$ref`` : string option // Probably will not be necessary
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
    //Servers : TBD
    Parameters : Parameter list
}

type OpenAPI = {
    SpecificationVersion : string
    Info : Info
    //Servers : TBD
    Paths : Path list
}
