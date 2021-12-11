module AppImager.Cli

open System
open System.IO

open Argu

[<NoAppSettings>]
type Arguments =
    | [<AltCommandLine("-l"); Unique>] List
    | [<AltCommandLine("-i"); Unique>] Install of List<string>
    | [<AltCommandLine("-u"); Unique>] Uninstall of List<string>
    | [<AltCommandLine("-v"); Unique>] Version
    | [<AltCommandLine("-s"); EqualsAssignmentOrSpaced; Unique>] Search of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | List -> "list installed AppImages."
            | Version -> "displays version information."
            | Install _ -> "install the app."
            | Uninstall _ -> "uninstall the given app."
            | Search _ -> "search app for the given string."

let runApp argv =
    let errorHandler =
        ProcessExiter(
            colorizer =
                function
                | ErrorCode.HelpText -> None
                | _ -> Some ConsoleColor.Red
        )

    let parser =
        ArgumentParser.Create<Arguments>(
            errorHandler = errorHandler,
            helpTextMessage = "Tiny cli to manage AppImage packaged apps"
        )

    let results =
        parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)

    let cmd = results.GetAllResults()

    if cmd.IsEmpty then
        printfn $"{parser.PrintUsage()}"
        None
    else
        Some(cmd.Head)


let uninstall apps =
    let total =
        ({| NotRemoved = []; NotExist = [] |}, apps)
        ||> List.fold
                (fun acc curr ->
                    if File.Exists(curr) then
                        try
                            File.Delete(curr)
                            acc
                        with
                        | _ ->
                            {| acc with
                                   NotRemoved = curr :: acc.NotRemoved |}

                    else
                        {| acc with
                               NotExist = curr :: acc.NotExist |})

    if not total.NotRemoved.IsEmpty then
        eprintfn $"""Unable to remove: {total.NotExist |> String.concat ", "}"""

    if not total.NotExist.IsEmpty then
        eprintfn $"""Unable to remove: {total.NotExist |> String.concat ", "}"""

let install apps =
    //
    ()

let list =
    Directory.GetFiles(Utils.AppBinDirectory)
    |> Array.map Path.GetFileName
    |> Array.iter (fun bin -> printfn $"{bin}")

    ()

let search query =
    //
    ()
