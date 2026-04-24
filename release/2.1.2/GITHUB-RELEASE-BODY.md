## XstReader 2.1.2

- Fix PST CRC calculation to match the MS-PST CRC variant.
- Validate block CRCs against stored block bytes before decoding.
- Refresh the published 2.1.2 package set.
- Update the Windows app About dialog repository URL to `https://github.com/NeedsCoffee/XstReaderNext`.
- Add published `linux-arm64`, `win-arm64` `XstExport` packages, plus a `win-arm64` `XstReader` package.

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
