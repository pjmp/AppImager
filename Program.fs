module AppImager.Main

open System
open System.IO

let initApp () =
    let dirs =
        [ Utils.AppDirectory
          Utils.AppDBDirectory
          Utils.AppBinDirectory ]

    match List.forall Directory.Exists dirs with
    | true -> ()
    | false ->
        List.iter (fun dir -> Directory.CreateDirectory(dir) |> ignore) dirs
        ()

let getData () =
    match File.Exists(Utils.AppDBFile) with
    | true -> ()
    | false ->
        use client = new Net.Http.HttpClient()

        task {
            let! response = client.GetStringAsync("https://appimage.github.io/feed.json")

            File.WriteAllText(Utils.AppDBFile, response)
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously

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
        | None -> exit 1

        initApp ()

        getData ()
    with
    | err ->
        Console.ForegroundColor <- ConsoleColor.DarkRed
        eprintfn $"{err.Message}"

#if DEBUG
        eprintfn $"{err.StackTrace}"
#endif

        Console.ResetColor()
        exit 1

    0
