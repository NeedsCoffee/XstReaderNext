## Multi-platform standalone .NET 10 release!

- Build solution using .NET 10
- Fix hardcoded paths to allow multi-platform use.
- Adapt PST/OST parsing code for latest PST spec (11.2)
- Fix PST CRC calculation to match the MS-PST CRC variant.
- Validate block CRCs against stored block bytes before decoding.
- Produce x64 and arm64 builds for windows, linux and osx.

Downloads:
- `XstReader-win-x64.zip`
- `XstReader-win-arm64.zip`
- `XstExport-win-x64.zip`
- `XstExport-win-arm64.zip`
- `XstExport-linux-x64.tar.gz`
- `XstExport-linux-arm64.tar.gz`
- `XstExport-osx-x64.tar.gz`
- `XstExport-osx-arm64.tar.gz`
- `SHA256SUMS.txt`

Notes:
- Windows packages are zip archives.
- macOS and Linux packages are tarballs.
- The non-Windows `XstExport` builds and the new Windows ARM64 packages are publish-verified but not yet runtime-tested on native target systems.
- Checksums are published in `SHA256SUMS.txt`.
