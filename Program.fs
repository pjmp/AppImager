module AppImager.Main

open System
open System.IO

type Author = { name: string; url: string }

type Link = { ``type``: string; url: string }

type Item =
    { name: string
      description: string
      categories: string List
      authors: Author List
      license: Option<string>
      links: Link List
      icons: string List
      screenshots: string List }

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
        async {
            use client = new Net.Http.HttpClient()

            let! response =
                client.GetStringAsync("https://appimage.github.io/feed.json")
                |> Async.AwaitTask

            File.WriteAllTextAsync(getAppDBPath (), response)
            |> Async.AwaitTask
            |> ignore
        }
        |> Async.RunSynchronously

[<EntryPoint>]
let main args =
    try
        let cmd = Cli.runApp args
        
        match cmd with
        | Some(r) ->
            match r with
            | Cli.Search (query) -> printfn $"Searching: {query}"
            | Cli.Install (apps) -> printfn $"install: {apps}"
            | Cli.Uninstall (apps) -> printfn $"uninstall: {apps}"
            | Cli.List -> printfn "listing.."
            | Cli.Version -> printfn "version"
        | None -> ()

        initApp ()

        getData ()

        printfn "Bye.."
    with
    | e -> printfn $"{e.Message}"

    0
