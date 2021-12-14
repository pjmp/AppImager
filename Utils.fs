module AppImager.Utils

open System
open System.IO

let AppDirectory =
    // use this: Environment.SpecialFolder.Personal
    Path.Join(Environment.GetEnvironmentVariable("HOME"), ".AppImager")

let AppDBDirectory = Path.Join(AppDirectory, "cache")

let AppBinDirectory = Path.Join(AppDirectory, "bin")

let AppDBFile =
    Path.Join(AppDirectory, "cache", "db.json")
