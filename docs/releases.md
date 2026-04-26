# Releases

## Current Release

The current release line is `2.1.2`.

Release metadata is stored under:

- `docs/releases/2.1.2/GITHUB-RELEASE-BODY.md`
- `docs/releases/2.1.2/RELEASE-NOTES.txt`
- `docs/releases/2.1.2/SHA256SUMS.txt`

Published binaries are distributed through GitHub Releases rather than committed to the repository.

The parser work in this repository tracks Microsoft’s published [MS-PST specification](https://learn.microsoft.com/en-us/openspecs/office_file_formats/ms-pst/141923d5-15ab-4ef1-a524-6dce75aae546).

## Release History

- `2.1.2`
  Fixes PST integrity validation to use the MS-PST CRC variant, validates block CRCs against stored block bytes before decoding, and keeps the CLI exporter as the cross-platform-oriented target in the source tree
- `2.1.1`
  Follow-on maintenance release with parser/spec alignment, stricter integrity checks, broader header/version handling, improved `PtypString8` decoding, generic BTH key-size handling, clearer WIP-protected messaging, OST 4K compatibility handling, and packaging cleanup
- `2.0`
  Major modernization release: migration from `.NET Framework 4` to `.NET 10`, SDK-style projects, removal of the separate `XstPortableExport` project, self-contained single-file packaging, updated settings handling, and modern runtime compatibility fixes
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
  Added recipient and attachment property display
- `1.0`
  First release
