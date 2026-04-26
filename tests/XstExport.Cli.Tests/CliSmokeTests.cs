using System.Diagnostics;

namespace XstExport.Cli.Tests;

public class CliSmokeTests
{
    [Fact]
    public async Task Help_PrintsUsageAndExitsSuccessfully()
    {
        CommandResult result = await RunCliAsync("--help");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Usage:", result.StandardOutput);
        Assert.Contains("--email", result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
    }

    [Fact]
    public async Task MissingCommand_ReturnsInvalidParameterError()
    {
        CommandResult result = await RunCliAsync();

        Assert.Equal(87, result.ExitCode);
        Assert.Contains("You must specify exactly one of --email, --properties, --attachments or --help.", result.StandardError);
    }

    [Fact]
    public async Task MissingOutlookFile_ReturnsInvalidParameterError()
    {
        CommandResult result = await RunCliAsync("--email");

        Assert.Equal(87, result.ExitCode);
        Assert.Contains("You must specify exactly one Outlook file to export from.", result.StandardError);
    }

    [Fact]
    public async Task MissingInputFile_ReturnsFileNotFoundError()
    {
        string missingFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pst");

        CommandResult result = await RunCliAsync("--email", missingFile);

        Assert.Equal(2, result.ExitCode);
        Assert.Contains($"Cannot find Outlook file '{missingFile}'", result.StandardError);
    }

    private static async Task<CommandResult> RunCliAsync(params string[] args)
    {
        string repositoryRoot = FindRepositoryRoot();
        string dllPath = Path.Combine(repositoryRoot, "src", "XstExport", "bin", "Debug", "net10.0", "XstExport.dll");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        startInfo.ArgumentList.Add(dllPath);
        foreach (string arg in args)
            startInfo.ArgumentList.Add(arg);

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);

        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return new CommandResult(
            process.ExitCode,
            NormalizeLineEndings(await stdoutTask),
            NormalizeLineEndings(await stderrTask));
    }

    private static string FindRepositoryRoot()
    {
        string? current = AppContext.BaseDirectory;

        while (current != null)
        {
            if (File.Exists(Path.Combine(current, "XstReader.sln")))
                return current;

            current = Directory.GetParent(current)?.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test output directory.");
    }

    private static string NormalizeLineEndings(string value)
    {
        return value.Replace("\r\n", "\n").Trim();
    }

    private sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError);
}
