# Xst Reader

Xst Reader is an open source reader for Microsoft Outlook `.ost` and `.pst` files, written entirely in C# and now modernized to `.NET 10`.

This repository is a fork of the original XstReader project by Dijji:
[Dijji/XstReader](https://github.com/Dijji/XstReader).

The original codebase, application design, and PST/OST reader implementation were created by Dijji. This fork continues that work with modernization, packaging, and compatibility updates.

Current version: `2.1.2`

It includes:

- `XstReader`: a Windows desktop viewer for browsing mail, recipients, attachments, and message properties
- `XstExport`: a command-line exporter for emails, attachments, and CSV property dumps

The project has no dependency on Microsoft Office components.

![Xst Reader screenshot](screenshot5.png)

## Project Lineage

The upstream project was authored by Dijji and published at [Dijji/XstReader](https://github.com/Dijji/XstReader).

This repository is a maintained fork of that codebase. It keeps the original reader/exporter model intact while updating the implementation for modern .NET and current Windows packaging.

## What Changed From The Original .NET Framework 4 Project

The original project was built around `.NET Framework 4` and older-style Visual Studio project files. This repository has now been updated to a modern SDK-style layout and `.NET 10`.

Version `2.0` was the line in the sand for that modernization. Version `2.1.2` builds on that work with parser/spec alignment, OST compatibility updates, and PST CRC validation fixes.

The main changes are:

- the solution now uses SDK-style `.csproj` files instead of legacy .NET Framework project files
- the desktop app now targets `net10.0-windows`
- the command-line exporter now targets `net10.0`
- the old separate `XstPortableExport` project has been removed because `XstExport` is now the portable CLI
- the desktop app no longer depends on the old `Properties.Settings` / `App.config` setup; it now uses a small JSON-backed settings file
- release builds can now be produced as self-contained single-file executables
- the shared parsing code was updated to work cleanly with modern trimming and modern stream-read correctness rules
- exporter compatibility issues found during real PST testing on `.NET 10` were fixed, including legacy code page handling and stricter numeric conversions

What did not change:

- the core PST/OST reading model and feature set remain based on the original project
- `XstReader` is still the Windows desktop UI
- `XstExport` still exports native email bodies (`.html`, `.rtf`, `.txt`), attachments, and CSV properties

## Current Runtime Model

- `XstReader` targets `net10.0-windows`
- `XstExport` targets `net10.0`
- `XstReader.Base` multi-targets `net10.0` and `net10.0-windows`

In practice this means:

- the desktop viewer is Windows-only
- the CLI exporter is the cross-platform-oriented build target in the source tree
- release packages can be published per runtime identifier, including `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, and `osx-arm64`
- the Linux and macOS `XstExport` builds are not yet runtime-tested on native Linux/macOS systems

## Features

`XstReader` provides:

- three-pane Outlook-style browsing
- viewing of plain text, HTML, and RTF message bodies
- recipient and attachment inspection
- message property inspection
- export of messages and attachments from the UI
- support for signed or encrypted message handling when the required certificate material is available

`XstExport` provides:

- export of email bodies in native format
- export of attachments only
- export of message properties to CSV
- export from the whole mailbox or from a selected subtree
- optional preservation of Outlook subfolder structure

## XstExport Usage

```text
XstExport.exe {-e|-p|-a|-h} [-f=<Outlook folder>] [-o] [-s] [-t=<target directory>] <Outlook file name>
```

Where:

- `-e`, `--email`
  Export in native body format (`.html`, `.rtf`, `.txt`) with attachments in an associated folder
- `-p`, `--properties`
  Export properties only in CSV format
- `-a`, `--attachments`
  Export attachments only
- `-h`, `--help`
  Display help
- `-f=<Outlook folder>`, `--folder=<Outlook folder>`
  Export from a specific Outlook folder or subtree, for example `Week1\Sent`
- `-o`, `--only`
  Export only the nominated folder, not its subfolders
- `-s`, `--subfolders`
  Preserve Outlook subfolder structure in the output
- `-t=<target directory>`, `--target=<target directory>`
  Write output to a specific target directory

## Release Artifacts

Published release assets for version `2.1.2` currently include:

- `XstReader-win-x64.zip`
- `XstReader-win-arm64.zip`
- `XstExport-win-x64.zip`
- `XstExport-win-arm64.zip`
- `XstExport-linux-x64.tar.gz`
- `XstExport-linux-arm64.tar.gz`
- `XstExport-osx-x64.tar.gz`
- `XstExport-osx-arm64.tar.gz`

These packages contain self-contained single-file executables, so they do not require a separate .NET runtime installation on the target machine.

Important:

- `XstReader` is only supported on Windows
- the Linux and macOS `XstExport` packages are publishable and packaged, but they are not yet runtime-tested on native Linux/macOS environments

Runtime behavior note:

- `XstReader` creates a user settings file under `%LocalAppData%\XstReader\settings.json` after first launch
- `XstExport` writes exported mail/attachment output wherever you point it

## Building

See [HowToBuild.md](HowToBuild.md) for build details.

At a high level:

```powershell
dotnet build XstReader.sln
dotnet run --project XstReader.csproj
dotnet run --project XstExport\XstExport.csproj -- --help
```

Cross-platform self-contained exporter publishes:

```powershell
dotnet publish XstExport\XstExport.csproj -c Release -r linux-x64
dotnet publish XstExport\XstExport.csproj -c Release -r linux-arm64
dotnet publish XstExport\XstExport.csproj -c Release -r osx-x64
dotnet publish XstExport\XstExport.csproj -c Release -r osx-arm64
dotnet publish XstExport\XstExport.csproj -c Release -r win-arm64
```

Without an explicit `-o`, `dotnet publish` uses the normal SDK publish folders under `bin\Release\<tfm>\<rid>\publish\`.

Those non-Windows exporter builds are intended for packaging and evaluation, but they should currently be treated as untested until they have been exercised on native Linux/macOS machines with real `.pst` or `.ost` files.

## Notes For Developers

- `XstReader.Base` contains the shared PST/OST parsing functionality used by both the desktop app and the CLI exporter
- the Windows target keeps the WPF-specific desktop integration
- the `net10.0` target is what `XstExport` consumes
- publish/build output paths now use the normal .NET SDK defaults unless you pass an explicit output path
- the repo intentionally keeps `test/` out of cleanup passes because it is used for real PST validation

## Background

Xst Reader is based on Microsoft’s documentation of the Outlook file formats in [MS-PST](https://msdn.microsoft.com/en-us/library/ff385210(v=office.12).aspx), first published in 2010 as part of the anti-trust settlement with the DOJ and the EU.

The original motivation for the project was simple: open `.ost` files without Outlook. That remains one of the main reasons the tool is useful.

Credit for the original XstReader application and parser belongs to Dijji. This fork builds on that foundation.

## Release History

- `2.1.2`
  Fixes PST integrity validation to use the MS-PST CRC variant and validates block CRCs against stored block bytes before decoding; also removes the abandoned cross-platform UI experiment and keeps the CLI exporter as the cross-platform-oriented target in the source tree
- `2.1.1`
  Follow-on maintenance release: updated the PST/OST parser to better align with the current MS-PST specification where relevant, including stricter integrity checks, broader supported header/version handling, improved `PtypString8` decoding, generic BTH key-size handling, clearer WIP-protected PST/OST messaging, OST 4K compatibility handling, and refreshed packaging/metadata cleanup
- `2.0`
  Major modernization release: migrated from `.NET Framework 4` to `.NET 10`, converted to SDK-style projects, removed the separate `XstPortableExport` project, added self-contained single-file packaging, replaced legacy desktop settings/config handling, and fixed modern runtime compatibility issues uncovered during real PST validation
- `1.14`
  Added command-line export of email contents, attachments, and properties
- `1.12`
  Added support for viewing encrypted or signed messages and attachments when matching certificate material is available
- `1.8`
  Added searching within `Cc` and `Bcc`, improved message time display, and added the Info button
- `1.7`
  Added export of email contents from the UI
- `1.6`
  Added searching through listed message headers
- `1.4`
  Added CSV export of message and related properties
- `1.2`
  Added inline attachment display in HTML bodies
- `1.1`
  Added Recipient and Attachment property display
- `1.0`
  First release

## License

Distributed under the MS-PL license. See [license.md](license.md) for more information.
