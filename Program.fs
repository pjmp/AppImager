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
            | Cli.Search query -> Commands.search query
            | Cli.Install apps -> Commands.install apps
            | Cli.Uninstall apps -> Commands.uninstall apps
            | Cli.List -> Commands.list ()
            | Cli.Version -> Commands.version ()
        | None -> exit 1

        initApp ()

        getData ()
    with
    | err ->
        eprintfn $"{err.Message}"
        exit 1

    0
