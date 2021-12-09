module AppImager.Cli

open System

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
            | List -> "list installed AppImages"
            | Version -> "version"
            | Install (_) -> "install"
            | Uninstall (_) -> "uninstall"
            | Search (_) -> "search"

let runApp (argv: string []) =
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
    
    if cmd.IsEmpty then
        printfn "%A" <| parser.PrintUsage()
        None
    else
        Some(cmd.Head)
