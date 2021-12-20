module AppImager.Main

open System

[<EntryPoint>]
let main args =
    try
        match Cli.runApp args with
        | Some cmd ->
            match cmd with
            | Cli.Search query -> Commands.Search query
            | Cli.Install apps -> Commands.Install apps
            | Cli.Uninstall apps -> Commands.Uninstall apps
            | Cli.ListApps -> Commands.ListApps()
            | Cli.Version -> Commands.Version()
        // | Cli.Refresh -> printfn "need to refresh"
        | None -> exit 1

        Utils.Init()
    with
    | err ->
        Console.ForegroundColor <- ConsoleColor.DarkRed
        eprintfn $"Error: {err.Message}"

#if DEBUG
        eprintfn $"{err.StackTrace}"
#endif

        Console.ResetColor()
        exit 1

    0
