module OpenAPITypeProvider.Tests.Parser.Operation

open NUnit.Framework
open OpenAPITypeProvider.Parser
open OpenAPIProvider.Specification
open OpenAPITypeProvider.Tests

let sample = {
    Description = None
    Tags = ["pet"]
    Summary = Some "Updates a pet in the store with form data"
    OperationId = Some "updatePetWithForm"
    Parameters = 
        [
            {
                Name = "petId"
                In = "path"
                Description = Some "ID of pet that needs to be updated"
                Required = true
                Deprecated = false
                AllowEmptyValue = false
                Schema = Schema.String(StringFormat.Default)
                Content = Map.empty
            }
        ]
    RequestBody = 
        Some ({
                Description = None
                Content = 
                    [
                        "application/x-www-form-urlencoded",
                        { 
                            Schema = 
                                Schema.Object (
                                    ["name", Schema.String (StringFormat.Default)] |> Map
                                    ,["name"]
                                ) 
                        }
                    ] |> Map
                Required = false
    })
    Responses = 
        [
            "200", { 
                Description = "Pet updated"
                Headers = Map.empty
                Content = 
                    [
                        "application/json", { Schema = Schema.Empty }
                        "application/xml", { Schema = Schema.Empty }
                    ] |> Map}
        ] |> Map
    Deprecated = false
}

[<Test>]
let ``Parses operation``() = 
    let actual = "Operation.yaml" |> SampleLoader.parseWithRoot Operation.parse
    Assert.AreEqual(sample, actual)