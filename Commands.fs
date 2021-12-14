module AppImager.Commands

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

let uninstall apps =
    let total =
        ({| NotRemoved = []; NotExist = [] |}, apps)
        ||> List.fold (fun acc curr ->
            if File.Exists(curr) then
                try
                    File.Delete(curr)
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
        eprintfn $"""Unable to remove: {total.NotExist |> String.concat ", "}"""
        exitCode <- 1

    if not total.NotExist.IsEmpty then
        eprintfn $"""Unable to remove: {total.NotExist |> String.concat ", "}"""
        exitCode <- 1

    exit exitCode

let install apps = exit 0

let list () =
    Directory.GetFiles(Utils.AppBinDirectory)
    |> Array.map Path.GetFileName
    |> Array.iter (fun bin -> printfn $"{bin}")

    printfn "done"
    exit 0

let search query = exit 0

let version () = printfn "v0.1.0"
