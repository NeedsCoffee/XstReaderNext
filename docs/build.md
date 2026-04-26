## Build Prerequisites

- .NET SDK `10.0` or later
- Windows for the WPF desktop app (`XstReader`)

## What Is In The Solution

- `XstReader`
  Windows desktop viewer targeting `net10.0-windows`
- `XstExport`
  Command-line exporter targeting `net10.0`
- `XstReader.Base`
  Shared PST/OST parsing library used by both apps

Compared with the original `.NET Framework 4` codebase:

- the projects are now SDK-style
- the separate `XstPortableExport` project is gone
- `XstExport` is now the single CLI exporter project
- the desktop app uses JSON-backed local settings instead of the old `App.config` / `Properties.Settings` model

This modernization was the basis of version `2.0`. The current release line is `2.1.2`.

## Build The Solution

```powershell
dotnet build XstReader.sln
```

## Run Tests

```powershell
dotnet test tests\XstReader.Base.Tests\XstReader.Base.Tests.csproj
```

## Run From Source

Desktop app:

```powershell
dotnet run --project src\XstReader\XstReader.csproj
```

Command-line exporter:

```powershell
dotnet run --project src\XstExport\XstExport.csproj -- --help
```

Example exporter run:

```powershell
dotnet run --project src\XstExport\XstExport.csproj -- -e C:\mail\archive.pst
```

## Create Release Packages

The current release process produces self-contained single-file desktop executables for `XstReader` on:

- `win-x64`
- `win-arm64`

And self-contained single-file `XstExport` binaries for:

- `win-x64`
- `win-arm64`
- `linux-x64`
- `linux-arm64`
- `osx-x64`
- `osx-arm64`

With default SDK behavior, publish outputs go under each project's `bin\Release\<tfm>\<rid>\publish\` directory unless you pass `-o`.

For cross-platform CLI exporter publishes, run:

```powershell
dotnet publish src\XstExport\XstExport.csproj -c Release -r linux-x64
dotnet publish src\XstExport\XstExport.csproj -c Release -r linux-arm64
dotnet publish src\XstExport\XstExport.csproj -c Release -r osx-x64
dotnet publish src\XstExport\XstExport.csproj -c Release -r osx-arm64
dotnet publish src\XstExport\XstExport.csproj -c Release -r win-arm64
```

Current validation status:

- the Windows desktop app and Windows exporter are the primary tested release targets
- the Linux and macOS `XstExport` publishes build successfully, but they are not yet runtime-tested on native Linux/macOS systems

## Verification Notes

Recent modernization work also included:

- fixing code-page registration needed by real PST files on modern .NET
- fixing stricter numeric conversion issues exposed by real-world PST data
- fixing partial-read assumptions in low-level stream parsing code

So if you are validating a migration from the original `.NET Framework 4` version, use a real `.pst` or `.ost` sample rather than only `--help` smoke tests.
