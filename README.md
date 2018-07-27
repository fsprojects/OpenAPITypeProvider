<img src="https://github.com/Dzoukr/OpenAPITypeProvider/raw/master/logo.jpg" alt="drawing" width="100px"/>


# OpenAPI Type Provider
*Erased netstandard2.0 type provider for web API providers.*

## Why to use this type provider?
If you write F# backend for some application providing JSON API, you probably want to document this using [OpenAPI v3 specification](https://www.openapis.org/) (previously called Swagger). This documentation can be created basically two ways:

1) Generate it from code (code first approach)
2) Write it manually (document first approach)

This type provider is focused on second option when you already got existing documentation (e.g. from frontend developer) and you want to be 100% sure that your API follows it correctly, which mostly means two things: Validation of requests payload is in correct form (as described in API documentation) and creating responses. Both things can be quite tedious and error-prone. It is common in web API development that after some time that server behavior is not what is written in documentation. This type provider is here to help with that.

## Goals of OpenAPITypeProvider
When I started to think about writing this type provider, I set few goals:

1. Netstandard2.0 support
2. Erased type
3. Tightly connected to Newtonsoft.Json
4. Based on the latest OpenAPI specification (no support for Swagger)


## Instalation

First install [NuGet package](https://www.nuget.org/packages/OpenAPITypeProvider/)

    Install-Package OpenAPITypeProvider

or using Paket

    nuget OpenAPITypeProvider

## How to use

First open correct namespace and create basic type based on your YAML documentation

```fsharp
open OpenAPITypeProvider

type PetStore = OpenAPIV3Provider<"PetStore.yaml">
```

Now you can use defined Schemas in your specification like F# types.

### Parsing from JSON

Each Schema type can be created from JSON string using static method `Parse`.

```fsharp
let json = """{"name":"Roman"}"""
let pet = PetStore.Schemas.NewPet.Parse(json)
let name = pet.Name // contains Roman
let tag = pet.Tag // contains Option value None
```

Sometimes you need to use parse JSON with custom date format.

```fsharp
let json = """{"date1":"31. 12. 2018 12:34:56","date2":"31. 12. 2017 12:34:56"}"""
let customDateFormat = "dd. MM. yyyy HH:mm:ss"
let twoDates = PetStore.Schemas.TwoDates.Parse(json, customDateFormat)
```

Method `Parse` throws an exception in case JSON does not fit to definition:

```fsharp
// fails with exception that property 'name' is not present,
// but should be based on Schema definition
let json = """{"notExistingProperty":"Roman"}"""
let pet = PetStore.Schemas.NewPet.Parse(json) 

// fails with exception that property 'name' not convertible
// to string value
let json = """{"name":123456}"""
let pet = PetStore.Schemas.NewPet.Parse(json) 
```

### Instantiation of Schema types

Schema types has constructors based on definition so you can instantiate them as you need.

```fsharp
let pet = new PetStore.Schemas.NewPet(name = "Roman")
```

### Converting to JSON

Each Schema instance has method `ToJson` with few overloads.

```fsharp
let pet = new PetStore.Schemas.NewPet(name = "Roman")
pet.ToJson() // returns '{"name":"Roman"}' - no indenting
pet.ToJson(Newtonsoft.Json.Formatting.Indented) // return json with fancy formatting
```

### Converting to JSON with custom serializer

Each `ToJson` method has overload with supporting `JsonSerializerSettings` as parameter.

```fsharp
let pet = new PetStore.Schemas.NewPet(name = "Roman")

let settings = JsonSerializerSettings()
settings.NullValueHandling <- NullValueHandling.Include

pet.ToJson(settings, Formatting.None) // returns '{"name":"Roman","tag":null}'
```

### Converting to Newtonsoft JToken

If you need `JToken` instead of string with JSON, method `ToJToken` is here for you.

```fsharp
let pet = new PetStore.Schemas.NewPet(name = "Roman")
let jtoken = pet.ToJToken()
```

Again, you can customize how to handle optional values.

```fsharp
let pet = new PetStore.Schemas.NewPet(name = "Roman")
let jtoken = pet.ToJToken(NullValueHandling.Include) // this now contains JNull value inside JObject
```

### Simple values

By specification you are allowed to have Schema types not an objects, but simple values like strings or integers. This type provider supports them as well.

```yaml
SimpleString:
    type: string    
    
SimpleArray:
    type: array
    items:
        type: string
```

```fsharp
let simpleString = PetStore.Schemas.SimpleString("ABC")
simpleString.Value // contains "ABC"

let simpleArray = PetStore.Schemas.SimpleArray(["A";"B"])
simpleArray.Values // contains List<string> ["A";"B"]
```

### Requests & ResponseBodies

Using Schema types directly is quite handy and straightforward, but it doesn't say anything about routes, requests and responses. If you want to be 100% sure that you are fullfilling specification, go for `Parse` on Requests and `ToJson` / `ToJToken` methods on ResponseBodies.

```fsharp
let petStoreAPI = new PetStore() // Note! Instance of PetStore type is needed here.
let pet = new PetStore.Schemas.NewPet("Roman")

// this route returns NewPet schema by definition so ToJson allows only NewPet schema as parameter
petStoreAPI.Paths.``/pets/{id}``.Get.Responses.``200``.``application/json``.ToJson(pet)

// this route expects NewPet schema by definition so Parse method returns NewPet
let parsedPet = petStoreAPI.Paths.``/pets``.Post.RequestBody.``application/json``.Parse(jsonFromRequest)
```

In case you have any doubts, you can always have a look [at unit tests](https://github.com/Dzoukr/OpenAPITypeProvider/blob/master/tests/OpenAPITypeProvider.Tests/BasicTests.fs)

## Known issues & Limitations
1. No support for `OneOf` and `ManyOf` since they are basically union types which is quite difficult (or maybe impossible) to generate from type provider
2. No filewatcher on source file - I'll probably need help with this. Anyone? :)


## Contribution

You are more than welcome to send a [pull request](https://github.com/Dzoukr/OpenAPITypeProvider/pulls) if you find some bug or missing functionality.
