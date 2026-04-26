# Architecture

## Overview

The repository is split into three production projects under `src/`:

- `src/XstReader`
  Windows WPF desktop application
- `src/XstExport`
  Command-line exporter
- `src/XstReader.Base`
  Shared PST/OST parsing and supporting logic

## Responsibilities

### `XstReader`

- presents the Windows desktop UI
- displays folders, messages, recipients, attachments, and properties
- hosts desktop-specific settings and resources

### `XstExport`

- provides the non-UI export surface
- exports email bodies, attachments, and CSV property data
- is the cross-platform-oriented application in the repo

### `XstReader.Base`

- contains the shared PST/OST reader implementation
- holds low-level file-format parsing, mapping, crypto, integrity, and decompression code
- is consumed by both the desktop app and the CLI exporter

## Target Frameworks

- `XstReader` targets `net10.0-windows`
- `XstExport` targets `net10.0`
- `XstReader.Base` targets both `net10.0` and `net10.0-windows`

The shared library multi-targets so the Windows desktop app can use the Windows-specific target while the CLI stays on the plain `net10.0` path.

## Repository Layout

```text
src/
  XstReader/
  XstExport/
  XstReader.Base/
tests/
  XstReader.Base.Tests/
docs/
  build.md
  architecture.md
  testing.md
  releases.md
  media/
  specs/
```

## Design Notes

- UI code should stay in `XstReader` unless it is reusable parsing or model logic.
- Parser and file-format correctness changes should usually happen in `XstReader.Base`.
- Export behavior that does not belong in the desktop UI should live in `XstExport`.
