// include Fake lib
#I "packages/FAKE/tools/"

#r "FakeLib.dll"

open Fake
open Fake.DotNetCli

let appSrc = "src/OpenAPITypeProvider"
let testsSrc = "tests/OpenAPITypeProvider.Tests"

Target "BuildApp" (fun _ ->
    Fake.DotNetCli.Build (fun p -> { p with Project = appSrc; Configuration = "Debug";})
)

Target "RunTests" (fun _ ->
    Fake.DotNetCli.Test (fun p -> { p with Project = testsSrc; Configuration = "Debug";})
)

Target "Publish" (fun _ ->
    Fake.DotNetCli.Publish (fun p -> { p with Project = appSrc; Configuration = "Release"; Output = "../../deploy" })
)

Target "Clean" (fun _ -> DeleteDir "deploy" )

"RunTests" ==> "Clean" ==> "Publish"

// start build
RunTargetOrDefault "BuildApp"