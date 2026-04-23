### An Open Source Reader For Microsoft Outlook `.ost` And `.pst` Files

Xst Reader is an open source reader for Microsoft Outlook `.ost` and `.pst` files, written entirely in C#.

This repository originally used `.NET Framework 4`. It has now been modernized to `.NET 10`.

Current version: `2.1.1`

Current applications:

- `XstReader`
  A Windows desktop viewer for browsing mail, attachments, recipients, and properties
- `XstExport`
  A command-line exporter for messages, attachments, and CSV property dumps

![Home screenshot](Home_screenshot0.9.JPG)

## What Changed From The Original Project

The original project:

- targeted `.NET Framework 4`
- used legacy Visual Studio project files
- relied on the old desktop configuration/settings model
- had a separate portability story from the main CLI tooling

The current project:

- targets `.NET 10`
- uses SDK-style project files
- keeps the desktop app on `net10.0-windows`
- uses `XstExport` as the single CLI exporter target
- supports self-contained single-file release builds
- includes fixes needed for real PST compatibility on modern .NET, including code-page registration and stricter runtime casting behavior

That modernization was the defining change in version `2.0`. Version `2.1.1` adds follow-on parser/spec alignment and OST compatibility work.

## Core Capabilities

- open `.ost` and `.pst` files without Outlook
- view message bodies in plain text, HTML, and RTF
- inspect recipients, attachments, and raw message properties
- export messages in native format
- export attachments or CSV properties from the command line

## Runtime Notes

- the desktop viewer is Windows-only
- the CLI exporter source target is `net10.0`
- current packaged deliverables are prepared as self-contained `win-x64` executables

## Background

Xst Reader exists because `.ost` access without Outlook is still awkward, and open source tools in this space are limited.

The parser is based on Microsoft’s published Outlook file format documentation: [MS-PST](https://msdn.microsoft.com/en-us/library/ff385210(v=office.12).aspx).

For current build and release details, see the top-level [README](../README.md) and [HowToBuild](../HowToBuild.md).
