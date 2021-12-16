module AppImager.Commands

open System
open System.IO
open System.Text.Json

open PrettyTable

type Author = { name: string; url: string }

type Link = { ``type``: string; url: string }

type Item =
    { name: string
      description: Option<string>
      categories: List<string>
      authors: Option<List<Author>>
      license: Option<string>
      links: Option<List<Link>> }

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

let Install apps = exit 0

let ListApps () =
    let rows =
        List.ofArray (Directory.GetFiles(Utils.AppBinDirectory))
        |> List.map (fun file ->
            [ File.GetCreationTime(file).ToString()
              Path.GetFileName(file) ])

    prettyTable rows
    |> withHeaders [ "Installed Date"; "App" ]
    |> verticalRules FsPrettyTable.Types.NoRules
    |> horizontalAlignment FsPrettyTable.Types.Left
    |> printTable


let Search (query: string) =
    let db =
        JsonSerializer.Deserialize<{| items: List<Item> |}>(File.ReadAllText(Utils.AppDBFile))

    let matching =
        List.filter (fun item -> item.name.Contains(query, StringComparison.CurrentCultureIgnoreCase)) db.items

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

    prettyTable rows
    |> withHeaders headers
    |> verticalRules FsPrettyTable.Types.NoRules
    |> horizontalAlignment FsPrettyTable.Types.Left
    |> printTable

    exit 0

let Version () = printfn "v0.1.0"
