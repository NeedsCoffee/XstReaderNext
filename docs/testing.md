# Testing

## Automated Tests

The current automated test projects are:

- `tests/XstReader.Base.Tests`
- `tests/XstExport.Cli.Tests`
- `tests/XstReader.Tests`

It currently focuses on deterministic shared-library behavior in `XstReader.Base`, including:

- utility extensions
- CRC, signature, and alignment helpers
- crypto behavior that can be validated without PST fixtures

It also includes CLI smoke coverage for `XstExport`, including:

- `--help` output
- invalid argument combinations
- missing input file handling and exit codes

It also includes unit coverage for extracted `XstReader` desktop services, including:

- email export duplicate-name handling
- continue/cancel behavior when batch export hits errors
- message search section matching and next/previous navigation
- sort-direction resolution for repeated/default sorts
- mailbox session open/load/select orchestration

Run the tests with:

```powershell
dotnet test XstReader.sln
```

## CI

GitHub Actions runs the Windows CI workflow in `.github/workflows/ci.yml`. It:

- restores the solution
- builds the solution
- runs both automated test projects

## Manual Validation

Automated coverage is still light compared with the parser surface. For parser or export changes, manual validation with real `.pst` or `.ost` samples is still important.

Recommended manual checks:

- open representative `.pst` and `.ost` files in `XstReader`
- verify folder browsing, message viewing, and attachment inspection
- run `XstExport` against real data and inspect exported output
- validate error behavior on unusual or partially corrupt files

## Platform Notes

- `XstReader` is Windows-only and should be validated on Windows
- non-Windows `XstExport` publishes currently build successfully, but they are not yet treated as runtime-validated on native Linux or macOS systems

## Future Test Targets

Good next candidates for additional coverage:

- parser fixtures around representative PST/OST structures
- exporter behavior and option parsing
- regression tests for integrity and decompression edge cases
