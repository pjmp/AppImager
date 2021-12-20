module AppImager.Utils

open System
open System.IO
open System.Text.Json
open System.Diagnostics

open PrettyTable

let AppDirectory =
    // use this: Environment.SpecialFolder.Personal
    Path.Join(Environment.GetEnvironmentVariable("HOME"), ".AppImager")

let AppDBDirectory = Path.Join(AppDirectory, "cache")

let AppBinDirectory = Path.Join(AppDirectory, "bin")

let AppDBFile =
    Path.Join(AppDirectory, "cache", "db.json")

let private CreateDirectories =
    let dirs =
        [ AppDirectory
          AppDBDirectory
          AppBinDirectory ]

    match List.forall Directory.Exists dirs with
    | true -> ()
    | false ->
        List.iter (fun dir -> Directory.CreateDirectory(dir) |> ignore) dirs
        ()

let private CreateDb =
    match File.Exists(AppDBFile) with
    | true -> ()
    | false ->
        use client = new Net.Http.HttpClient()

        task {
            let! response = client.GetStringAsync("https://appimage.github.io/feed.json")

            do! File.WriteAllTextAsync(AppDBFile, response)
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously

let Init () =
    CreateDirectories
    CreateDb

let HumanizeTime (span: TimeSpan) =
    let format = "G3"

    if span.TotalMilliseconds < 1000 then
        span.TotalMilliseconds.ToString(format) + " ms"
    elif span.TotalSeconds < 60 then
        span.TotalSeconds.ToString(format) + " secs"
    elif span.TotalMinutes < 60 then
        span.TotalMinutes.ToString(format) + " mins"
    elif span.TotalHours < 24 then
        span.TotalHours.ToString(format) + " hrs"
    else
        span.TotalDays.ToString(format) + " days"

let PrettyPrint headers rows =
    prettyTable rows
    |> withHeaders headers
    |> verticalRules FsPrettyTable.Types.NoRules
    |> horizontalAlignment FsPrettyTable.Types.Left
    |> printTable

let GetDb<'T> =
    lazy (JsonSerializer.Deserialize<{| items: List<'T> |}>(File.ReadAllText(AppDBFile)))

let Chmod file =
    Process
        .Start("/usr/bin/env", [ "-S"; "sh"; "-c"; $"chmod +x {file}" ])
        .WaitForExitAsync()
    |> Async.AwaitTask
    |> Async.RunSynchronously

let AppNameVersion () =
    let info =
        Reflection
            .Assembly
            .GetExecutingAssembly()
            .GetName()

    (info.Name, info.Version)
