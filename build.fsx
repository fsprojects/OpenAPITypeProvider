// include Fake lib
#r "paket:
source release/dotnetcore
source https://api.nuget.org/v3/index.json
nuget BlackFox.Fake.BuildTask
nuget Fake.Core.ReleaseNotes
nuget Fake.Core.Target
nuget Fake.DotNet.Cli
nuget Fake.DotNet.Paket
nuget Fake.DotNet.Testing.NUnit
nuget Fake.IO.FileSystem
nuget FSharp.Core ~> 4.5.0 //"
#load "./.fake/build.fsx/intellisense.fsx"
#if !FAKE
#r "Facades/netstandard"
#r "netstandard"
#endif

open System.IO
open BlackFox.Fake
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators

let appSrc = "src/OpenAPITypeProvider.Runtime"
let testsSrc = "tests/OpenAPITypeProvider.Tests"

let cleanTask =
    BuildTask.create "Clean" [] {
        !! "src/*/bin"
        ++ "src/*/obj"
        ++ "tests/*/bin"
        ++ "tests/*/obj"
        |> Shell.deleteDirs
    }

let buildTask =
    BuildTask.create "BuildApp" [cleanTask] {
        appSrc
        |> DotNet.build (fun p -> { p with Configuration = DotNet.BuildConfiguration.Release })
    }

let testTask =
    BuildTask.create "RunTests" [buildTask] {
        testsSrc
        |> DotNet.test (fun p -> { p with Configuration = DotNet.BuildConfiguration.Release })
    }

// Read release notes & version info from RELEASE_NOTES.md
let release = ReleaseNotes.load (Path.Combine(__SOURCE_DIRECTORY__, "RELEASE_NOTES.md"))

let packTask =
    BuildTask.create "Nuget" [buildTask] {
        let toNotes = List.map (fun x -> x + System.Environment.NewLine) >> List.fold (+) ""
        Fake.DotNet.Paket.pack (fun p -> 
            { p with 
                Version = release.NugetVersion
                OutputPath = appSrc
                ReleaseNotes = (release.Notes |> toNotes)
            })
    }

let helpTask =
    BuildTask.create "Help" [] {
        Trace.logfn "build [--target <target>] [<options>]"
        BuildTask.listAvailable()
    }

let defaultTask =
    BuildTask.createEmpty "All" [testTask; packTask]

// start build
BuildTask.runOrDefault defaultTask
