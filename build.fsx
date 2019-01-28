// include Fake lib
#I "packages/FAKE/tools/"

#r "FakeLib.dll"

open Fake
open System.IO
let appSrc = "src/OpenAPITypeProvider.Runtime"
let testsSrc = "tests/OpenAPITypeProvider.Tests"

Target "BuildApp" (fun _ ->
    Fake.DotNetCli.Build (fun p -> { p with Configuration = "Release";})
)

Target "RunTests" (fun _ ->
    Fake.DotNetCli.Test (fun p -> { p with Configuration = "Release"; })
)

// Read release notes & version info from RELEASE_NOTES.md
let release = File.ReadLines "RELEASE_NOTES.md" |> ReleaseNotesHelper.parseReleaseNotes

Target "Nuget" <| fun () ->
    let toNotes = List.map (fun x -> x + System.Environment.NewLine) >> List.fold (+) ""
    Fake.DotNet.Paket.pack (fun p -> 
        { p with 
            Version = release.NugetVersion
            OutputPath = appSrc
            ReleaseNotes = (release.Notes |> toNotes)
        })

Target "Clean" (fun _ -> 
    !! "src/*/bin"
    ++ "src/*/obj"
    ++ "tests/*/bin"
    ++ "tests/*/obj"
    |> DeleteDirs
)

"Clean" ==> "BuildApp" ==> "RunTests" ==> "Nuget"

// start build
RunTargetOrDefault "BuildApp"