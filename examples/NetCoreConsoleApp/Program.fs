open System
open OpenAPITypeProvider

type PetStore = OpenAPIV3Provider<"PetStore.yaml">

[<EntryPoint>]
let main argv =
    let pet = PetStore.Schemas.Pet(1L, "Test")
    sprintf "Pet with ID %i and name %s" pet.Id pet.Name |> Console.WriteLine
    Console.ReadKey() |> ignore
    0