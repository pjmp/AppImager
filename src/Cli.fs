module AppImager.Cli

open System

open Argu

[<NoAppSettings>]
type Arguments =
    // | Refresh
    | [<AltCommandLine("-v"); Unique>] Version
    | [<AltCommandLine("-l"); Unique>] ListApps
    | [<AltCommandLine("-i"); Unique>] Install of List<string>
    | [<AltCommandLine("-u"); Unique>] Uninstall of List<string>
    | [<AltCommandLine("-s"); EqualsAssignmentOrSpaced; Unique>] Search of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            // | Refresh -> "re download local db"
            | ListApps -> "list installed AppImages."
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

    let results = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)

    let cmd = results.GetAllResults()

    // printfn "%A" cmd

    if cmd.IsEmpty then
        printfn $"{parser.PrintUsage()}"
        None
    else
        Some(cmd.Head)
