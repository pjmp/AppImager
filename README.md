# AppImager [![NuGet version (AppImager)](https://img.shields.io/nuget/v/AppImager.svg?style=flat-square)](https://www.nuget.org/packages/AppImager/)
Tiny cli to manage AppImage packaged apps


# Installation

```bash
dotnet tool install --global AppImager --version 0.2.0
```

# Available options

```
USAGE: AppImager [--help] [--version] [--listapps] [--install [<string>...]] [--uninstall [<string>...]] [--search=<string>]

OPTIONS:

    --version, -v         displays version information.
    --listapps, -l        list installed AppImages.
    --install, -i [<string>...]
                          install the app.
    --uninstall, -u [<string>...]
                          uninstall the given app.
    --search, -s=<string> search app for the given string.
    --help                display this list of options.
```
