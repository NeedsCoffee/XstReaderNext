# Contributing

## Before You Start

- Open an issue for significant changes before starting implementation work.
- Keep pull requests focused on a single logical change.
- Do not commit generated output, local IDE settings, or packaged release artifacts.

## Development Setup

1. Install the .NET 10 SDK.
2. Clone the repository.
3. Build the solution:

```powershell
dotnet build XstReader.sln
```

4. Run the test project:

```powershell
dotnet test tests\XstReader.Base.Tests\XstReader.Base.Tests.csproj
```

5. Run the desktop app:

```powershell
dotnet run --project src\XstReader\XstReader.csproj
```

6. Run the CLI help:

```powershell
dotnet run --project src\XstExport\XstExport.csproj -- --help
```

## Change Guidelines

- Prefer small, reviewable commits with clear messages.
- Preserve the existing parser behavior unless the change is intentional and documented.
- Keep Windows UI changes isolated from shared parsing logic where possible.
- Add or update documentation when behavior, packaging, or build steps change.

## Pull Requests

Before opening a pull request:

- build the solution locally
- verify the relevant app or CLI path you changed
- summarize user-visible behavior changes
- note any platform limitations or untested runtime combinations

## Repository Hygiene

- Do not commit files under `bin/` or `obj/`.
- Do not commit `*.user` or `*.csproj.user` files.
- Do not commit packaged release archives or staged release executables under `release/`.

## Questions

If you are unsure whether a change should affect `XstReader`, `XstExport`, or `XstReader.Base`, open an issue first and outline the intended scope.
