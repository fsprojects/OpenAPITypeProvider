// include Fake lib
#I "packages/FAKE/tools/"

#r "FakeLib.dll"

open Fake
open System.IO

let appSrc = "src/OpenAPITypeProvider"

Target "BuildApp" (fun _ ->
    Fake.DotNetCli.Build (fun p -> { p with Project = appSrc; Configuration = "Debug";})
)

Target "Publish" (fun _ ->
    Fake.DotNetCli.Publish (fun p -> { p with Project = appSrc; Configuration = "Release"; Output = "../../deploy" })
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
            "PackageIconUrl=\"https://avatars2.githubusercontent.com/u/851307?v=3&amp;s=64\""
            "PackageTags=\"F# FSharp OpenAPI Swagger TypeProvider\""
            "Copyright=\"Roman Provazník - 2018\""
            "Authors=\"Roman Provazník\""
        ] |> List.map (fun x -> "/p:" + x)

    Fake.DotNetCli.Pack (fun p -> { p with Project = appSrc; OutputPath = "../../nuget"; AdditionalArgs = args })

Target "Clean" (fun _ -> 
    DeleteDir "deploy" 
    !! "src/*/bin"
    ++ "src/*/obj"
    ++ "tests/*/bin"
    ++ "tests/*/obj"
    |> DeleteDirs
)

"Clean" ==> "Publish"
"Clean" ==> "Nuget"

// start build
RunTargetOrDefault "BuildApp"