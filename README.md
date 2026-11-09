# TxtToJson

A small Windows desktop app that turns a hand-written `.txt` file into JSON.

Write your data the easy way — no braces, no commas, no quotes required — and get valid, indented JSON out of it.

<!--
    Screenshot placeholder. Drop an image in the repo (for example docs/screenshot.png)
    and replace this comment with:
    ![TxtToJson](docs/screenshot.png)
-->

## Features

- **Drag & drop** up to 24 `.txt` files onto the window
- **Execute** converts them all at once and shows one card per resulting `.json`
- **Click a card** to save that JSON wherever you want
- **Download all** saves every JSON into a folder of your choice
- **Back** returns to the file list without losing anything
- **How to use** opens a help window with the rules and a worked example
- Errors are reported per file, including the line number

## The format

One `key:value` per line. Indentation builds the structure.

```text
name:John
age:28
active:true
hobbies: [read, cook]
address:
    city:Madrid
    cp: "28001"
pets:
    -dog:Conan
     age:2
    -cat:Fluffy
     age:1
```

That file produces:

```json
{
  "name": "John",
  "age": 28,
  "active": true,
  "hobbies": ["read", "cook"],
  "address": {
    "city": "Madrid",
    "cp": "28001"
  },
  "pets": [
    { "dog": "Conan", "age": 2 },
    { "cat": "Fluffy", "age": 1 }
  ]
}
```

### Rules

| You write | You get |
|---|---|
| `key:value` | One pair per line. Everything after the first `:` is the value. |
| `  indented` | Indented lines belong to the key above them. |
| `key:` | Nothing after the colon opens a nested object. |
| `-item` | A dash starts a list item. `-name:conan` makes a list of objects. |
| `[a, b, c]` | A short list on a single line. |
| `"28"` vs `28` | Quotes always win. `28` is a number, `"28"` is text. |
| `true` / `false` / `null` | Boolean and null, unquoted. |
| `1.68` | A decimal. `1,68` also works. |
| blank line | Ignored. |

A few consequences worth knowing:

- `08001` without quotes becomes the number `8001`, losing the leading zero. Quote it: `"08001"`.
- Keys are case-sensitive: `age` and `Age` are different keys.
- A duplicated key keeps the last value.

## Download

Grab a build from the [Releases](https://github.com/bernat-sanahuja/TxtToJson/releases) page:

| Build | Size | Requirements |
|---|---|---|
| Self-contained | ~133 MB | **None.** Extract and run. |
| Framework-dependent | ~1 MB | [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download) |

It is a portable app: no installer, no registry entries. Unzip and run `TxtToJson.exe`.

> The executable is not code signed, so Windows SmartScreen will warn you the first time. Choose **More info → Run anyway**. If you downloaded a `.zip`, right-click it → **Properties** → **Unblock** before extracting.

## Build from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```powershell
git clone https://github.com/bernat-sanahuja/TxtToJson.git
cd TxtToJson

# Run it
dotnet run

# Self-contained build (no .NET needed on the target machine)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o ./release/self-contained

# Small build (target machine needs the .NET 10 Desktop Runtime)
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false -o ./release/no-self-contained
```

## Project layout

```
MainWindow.xaml          main window: file list and results
MainWindow.xaml.cs       drag & drop, conversion, saving
HowToUseWindow.xaml      help window with the rules and the example
App.xaml                 shared brushes and button styles
TxtToJson.csproj         net10.0-windows, WPF, app icon
Models/
  Archivo.cs               one dropped .txt file
  ParserTxt.cs             the parser: text -> JSON tree
  ResultadoJson.cs         one generated .json
  FormatoInvalidoException.cs  parse error with its line number
```

## Limitations

- Windows only (WPF)
- `.txt` files only, up to 24 at a time
- No comments in the input format
- No multi-line strings: every value lives on a single line

## Privacy

Your files never leave your machine. The app makes no network requests, has no telemetry and no installer.
