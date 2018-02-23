module OpenAPITypeProvider.Parser.Components

open System
open OpenAPITypeProvider.Specification
open Core
open YamlDotNet.RepresentationModel

let private someOrEmptyMap = function
    | Some v -> v
    | None -> Map.empty

let parse (rootNode:YamlMappingNode) (node:YamlMappingNode) = 
    {
        Schemas = 
            node 
            |> tryFindByName "schemas"
            |> Option.map (toMappingNode >> toNamedMapM (Schema.parse rootNode))
            |> someOrEmptyMap
        Responses = 
            node 
            |> tryFindByName "responses"
            |> Option.map (toMappingNode >> toNamedMapM (Response.parse rootNode))
            |> someOrEmptyMap
        Parameters =
            node 
            |> tryFindByName "parameters"
            |> Option.map (toMappingNode >> toNamedMapM (Parameter.parse rootNode))
            |> someOrEmptyMap
        RequestBodies = 
            node 
            |> tryFindByName "requestBodies"
            |> Option.map (toMappingNode >> toNamedMapM (RequestBody.parse rootNode))
            |> someOrEmptyMap
        Headers = 
            node 
            |> tryFindByName "headers"
            |> Option.map (toMappingNode >> toNamedMapM (Header.parse rootNode))
            |> someOrEmptyMap
    } : Components