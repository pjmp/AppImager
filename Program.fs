module AppImager.Main

open System
open System.IO

type Author = { name: string; url: string }

type Link = { ``type``: string; url: string }

type Item =
    { name: string
      description: string
      categories: List<string>
      authors: List<Author>
      license: Option<string>
      links: List<Link>
      icons: List<string>
      screenshots: List<string> }

let getAppDirectory () =
    Path.Join(Environment.GetEnvironmentVariable("HOME"), ".cache", "AppImager")

let getAppDBPath () =
    Path.Join(getAppDirectory (), "db.json")

let rec initApp () =
    let dir = getAppDirectory ()

    match Directory.Exists(dir) with
    | true -> ()
    | false ->
        Directory.CreateDirectory(dir) |> ignore
        initApp ()

let getData () =
    match File.Exists(getAppDBPath ()) with
    | true -> ()
    | false ->
        use client = new Net.Http.HttpClient()

        task {
            let! response = client.GetStringAsync("https://appimage.github.io/feed.json")

            File.WriteAllTextAsync(getAppDBPath (), response)
            |> ignore
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously

[<EntryPoint>]
let main args =
    try
        match Cli.runApp args with
        | Some cmd ->
            match cmd with
            | Cli.Search query -> Cli.install query
            | Cli.Install apps -> Cli.install apps
            | Cli.Uninstall apps -> Cli.uninstall apps
            | Cli.List -> Cli.list
            | Cli.Version -> printfn "v0.1"
        | None -> exit 1

        initApp ()

        getData ()
    with
    | err -> eprintfn $"{err.Message}"

    0
