// include Fake lib
#I "packages/FAKE/tools/"

#r "FakeLib.dll"

open Fake
open System.IO

let appSrc = "src/OpenAPITypeProvider"
let testsSrc = "tests/OpenAPITypeProvider.Tests"

Target "BuildApp" (fun _ ->
    Fake.DotNetCli.Build (fun p -> { p with Project = appSrc; Configuration = "Debug";})
)

Target "RunTests" (fun _ ->
    Fake.DotNetCli.Test (fun p -> { p with Project = testsSrc; Configuration = "Debug"; })
)

// Read release notes & version info from RELEASE_NOTES.md
let release = File.ReadLines "RELEASE_NOTES.md" |> ReleaseNotesHelper.parseReleaseNotes

Target "Nuget" <| fun () ->
    let toNotes = List.map (fun x -> x + System.Environment.NewLine) >> List.fold (+) ""
    let args = 
        [
            "PackageId=\"OpenAPITypeProvider\""
            "Title=\"OpenAPITypeProvider\""
            "Description=\"F# Type ProviderOpen for API Specification\""
            "Summary=\"F# Type ProviderOpen for API Specification\""
            sprintf "PackageVersion=\"%s\"" release.NugetVersion
            sprintf "PackageReleaseNotes=\"%s\"" (release.Notes |> toNotes)
            "PackageLicenseUrl=\"http://github.com/dzoukr/OpenAPITypeProvider/blob/master/LICENSE.md\""
            "PackageProjectUrl=\"http://github.com/dzoukr/OpenAPITypeProvider\""
            "PackageIconUrl=\"https://raw.githubusercontent.com/Dzoukr/OpenAPITypeProvider/master/logo.jpg\""
            "PackageTags=\"F# FSharp OpenAPI Swagger TypeProvider\""
            "Copyright=\"Roman Provazník - 2018\""
            "Authors=\"Roman Provazník\""
        ] |> List.map (fun x -> "/p:" + x)

    Fake.DotNetCli.Pack (fun p -> { p with Project = appSrc; OutputPath = "../../nuget"; AdditionalArgs = args })

Target "Clean" (fun _ -> 
    !! "src/*/bin"
    ++ "src/*/obj"
    ++ "tests/*/bin"
    ++ "tests/*/obj"
    |> DeleteDirs
)

"Clean" ==> "RunTests" ==> "Nuget"

// start build
RunTargetOrDefault "BuildApp"