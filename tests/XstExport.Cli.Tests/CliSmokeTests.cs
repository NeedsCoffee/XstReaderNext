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
        string dllPath = FindBuiltCliPath(repositoryRoot);

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

    private static string FindBuiltCliPath(string repositoryRoot)
    {
        string configuration = GetBuildConfiguration();
        string configurationDirectory = Path.Combine(repositoryRoot, "src", "XstExport", "bin", configuration);

        if (!Directory.Exists(configurationDirectory))
            throw new DirectoryNotFoundException($"Expected build output directory '{configurationDirectory}' was not found.");

        string[] candidates = Directory.GetFiles(configurationDirectory, "XstExport.dll", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}ref{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path.Count(ch => ch == Path.DirectorySeparatorChar))
            .ToArray();

        if (candidates.Length == 0)
            throw new FileNotFoundException($"Could not locate built XstExport.dll under '{configurationDirectory}'.");

        return candidates[0];
    }

    private static string GetBuildConfiguration()
    {
        string? current = AppContext.BaseDirectory;

        while (current != null)
        {
            string directoryName = Path.GetFileName(current);
            if (string.Equals(directoryName, "Debug", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(directoryName, "Release", StringComparison.OrdinalIgnoreCase))
            {
                return directoryName;
            }

            current = Directory.GetParent(current)?.FullName;
        }

        return "Debug";
    }

    private static string NormalizeLineEndings(string value)
    {
        return value.Replace("\r\n", "\n").Trim();
    }

    private sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError);
}
