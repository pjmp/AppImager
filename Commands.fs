module AppImager.Commands

open System
open System.IO
open System.Net.Http
open System.Text.Json

open Kurukuru

type Item =
    { name: string
      description: Option<string>
      categories: List<string>
      authors: Option<List<{| name: string; url: string |}>>
      license: Option<string>
      links: Option<List<{| ``type``: string; url: string |}>> }

let Uninstall apps =
    let total =
        ({| NotRemoved = []; NotExist = [] |}, apps)
        ||> List.fold (fun acc curr ->
            let fullPath = Path.Join(Utils.AppBinDirectory, curr)

            if File.Exists(fullPath) then
                try
                    File.Delete(fullPath)
                    acc
                with
                | _ ->
                    {| acc with
                        NotRemoved = curr :: acc.NotRemoved |}
            else
                {| acc with
                    NotExist = curr :: acc.NotExist |})

    let mutable exitCode = 0

    if not total.NotRemoved.IsEmpty then
        eprintfn $"""Unable to remove "{total.NotRemoved |> String.concat ", "}"."""
        exitCode <- 1

    if not total.NotExist.IsEmpty then
        eprintfn $"""Unable to remove "{total.NotExist |> String.concat ", "}", package not installed."""

        exitCode <- 1

    exit exitCode

let ListApps () =
    let rows =
        List.ofArray (Directory.GetFiles(Utils.AppBinDirectory))
        |> List.map (fun file ->
            [ File.GetCreationTime(file).ToString()
              Path.GetFileName(file) ])

    Utils.PrettyPrint [ "Installed Date"; "Packages" ] rows

let private download (url: string, file: string) =
    printfn "download %A" url

    Spinner.StartAsync(
        "Starting...",
        (fun (spinner: Spinner) ->
            task {
                let timer = new Diagnostics.Stopwatch()
                timer.Start()
                (*
                    Doing all this crap becase DotNet doesnt follow redirects from HTTPS -> HTTP
                    https://github.com/dotnet/runtime/issues/23801#issuecomment-335642298

                    Thanks https://github.com/SpaceMonkeyForever for this
                    https://github.com/dotnet/runtime/issues/23697#issuecomment-361018812
                *)
                spinner.Text <- "Starting..."
                use request = new HttpRequestMessage(HttpMethod.Get, url)

                use handler = new HttpClientHandler()
                handler.AllowAutoRedirect <- false

                use client = new HttpClient(handler)

                client.DefaultRequestHeaders.Add("Accept", "application/octet-stream")
                client.DefaultRequestHeaders.Add("User-Agent", "FSharp/6 DotNet/6 AppImager/0.1.0")

                spinner.Text <- "Making request..."

                let! response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)

                spinner.Text <- "Making request... 2"
                let! response = client.GetAsync(response.Headers.Location)

                spinner.Text <- "Saving..."
                do! response.Content.CopyToAsync(File.Create(file))

                spinner.Text <- "Making it executable.."
                Utils.Chmod file

                spinner.Text <- $"Done, took {Utils.HumanizeTime timer.Elapsed}"
                timer.Stop()
            })
    )
    |> Async.AwaitTask
    |> Async.RunSynchronously

let Install (apps: List<string>) =
    Utils.GetDb.Force().items
    |> List.tryFind (fun item -> item.name = apps.Head)
    |> function
        | Some app ->
            if app.links.IsSome then
                let links = app.links.Value

                links
                |> List.tryFind (fun item -> item.``type`` = "GitHub")
                |> function
                    | Some link ->
                        task {
                            use client = new HttpClient()

                            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json")
                            client.DefaultRequestHeaders.Add("User-Agent", "FSharp/6 DotNet/6 AppImager/0.1.0")

                            let! response = client.GetStringAsync($"https://api.github.com/repos/{link.url}/releases")

                            let json =
                                JsonSerializer.Deserialize<List<{| assets: List<{| url: string; name: string |}> |}>>(
                                    response
                                )

                            let assets =
                                json.Head.assets
                                |> List.filter (fun asset -> asset.name.EndsWith(".AppImage"))

                            if not assets.IsEmpty then
                                download (assets.Head.url, $"{Utils.AppBinDirectory}/{assets.Head.name}")
                            else
                                eprintfn $"No assets to download found for {app.name}"
                                exit 1

                        }
                        |> Async.AwaitTask
                        |> Async.RunSynchronously

                    | None ->
                        let link =
                            links
                            |> List.tryFind (fun item ->
                                item.``type`` = "Download"
                                && item.url.StartsWith("https://download.opensuse.org/repositories"))

                        if link.IsSome then
                            let url = link.Value.url.TrimEnd(".mirrorlist".ToCharArray())
                            download (url, $"{Utils.AppBinDirectory}/{app.name}.AppImage")
                        else
                            eprintfn $"No link to download found for {app.name}"
                            exit 1
            else
                eprintfn $"No link to download found for {app.name}"
                exit 1
        | None ->
            eprintfn $"""{apps |> String.concat ", "}, not found in DB, try refreshing and try again."""
            exit 1

let Search (query: string) =
    let matching =
        List.filter
            (fun item -> item.name.Contains(query, StringComparison.CurrentCultureIgnoreCase))
            (Utils.GetDb.Force().items)

    if matching.IsEmpty then
        eprintfn "No matches found for: %s" query
        exit 1

    let headers =
        [ "Name"
          "Authors"
          "Categories"
          "License"
          "Description"
          "Website" ]

    let rows =
        matching
        |> List.map (fun item ->
            let authors =
                match item.authors with
                | Some authors ->
                    authors
                    |> List.map (fun author -> author.name)
                    |> String.concat ", "
                | None -> "-"

            let categories = item.categories |> String.concat ", "

            let description =
                match item.description with
                | Some description ->
                    if description.Length >= 40 then
                        $"{description[..40]}..."
                    else
                        description
                | None -> "-"

            let license =
                match item.license with
                | Some license -> license
                | None -> "-"

            let website =
                match item.links with
                | Some links ->
                    match links
                          // since this api only has project's GitHub repository
                          // that provides meaningful homepage
                          |> List.tryFind (fun link -> link.``type`` = "GitHub")
                        with
                    | Some link -> $"https://github.com/{link.url}"
                    | None -> "-"
                | None -> "-"

            [ item.name
              authors
              categories
              license
              description
              website ])

    Utils.PrettyPrint headers rows

    exit 0

let Version () = printfn "v0.1.0"
