#r "paket: groupref FakeBuild //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.DotNet
open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake
open Fake.Core.TargetOperators

let appSrc = "src/OpenAPITypeProvider.Runtime"
let testsSrc = "tests/OpenAPITypeProvider.Tests"

Target.create "BuildApp" (fun _ ->
    appSrc |> DotNet.build (fun p -> { p with Configuration = DotNet.Custom "Release"})
)

Target.create "RunTests" (fun _ ->
    testsSrc
    |> DotNet.test (fun p -> 
        { p with 
            Configuration = DotNet.Custom "Debug"; 
            Common = {p.Common with CustomParams = Some  "--logger trx;logfilename=../../../results.trx" }})
)

// Read release notes & version info from RELEASE_NOTES.md
let release = ReleaseNotes.load "RELEASE_NOTES.md"

Target.create "Nuget" <| fun _ ->
    let toNotes = List.map (fun x -> x + System.Environment.NewLine) >> List.fold (+) ""
    Fake.DotNet.Paket.pack (fun p -> 
        { p with 
            Version = release.NugetVersion
            OutputPath = appSrc
            ReleaseNotes = (release.Notes |> toNotes)
        })

Target.create "Clean" (fun _ -> 
    !! "src/*/bin"
    ++ "src/*/obj"
    ++ "tests/*/bin"
    ++ "tests/*/obj"
    |> Fake.IO.Shell.deleteDirs
)

"Clean" ==> "BuildApp" ==> "RunTests" ==> "Nuget"

// start build
Fake.Core.Target.runOrDefault "BuildApp"