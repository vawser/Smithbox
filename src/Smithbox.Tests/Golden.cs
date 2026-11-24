namespace SmithboxTests;

/// <summary>
///     Golden-file checks: compares text a test renders with Golden/{name}.golden.txt, so a refactor can't change
///     behaviour unnoticed. When a change is intended, regenerate and review the file's diff:
///     <c>$env:SMITHBOX_UPDATE_GOLDEN = "1"; dotnet test --filter Golden</c>
/// </summary>
internal static class Golden
{
    public static void Verify(string name, string actual)
    {
        var file = Path.Combine(SourceFolder(), "Golden", $"{name}.golden.txt");

        if (Environment.GetEnvironmentVariable("SMITHBOX_UPDATE_GOLDEN") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, actual);
            return;
        }

        Assert.True(File.Exists(file), $"No golden file at {file}: set SMITHBOX_UPDATE_GOLDEN=1 and run the test to record it");

        var expectedLines = File.ReadAllText(file).Replace("\r\n", "\n").Split('\n');
        var actualLines = actual.Split('\n');

        // Line by line, so a failure names the first line that changed
        for (var n = 0; n < Math.Min(expectedLines.Length, actualLines.Length); n++)
            Assert.Equal(expectedLines[n], actualLines[n]);

        Assert.Equal(expectedLines.Length, actualLines.Length);
    }

    // Found from the test assembly: [CallerFilePath] is made relative by the PathMap in Directory.Build.props
    private static string SourceFolder()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Smithbox.Tests.csproj")))
                return dir.FullName;
        }

        throw new DirectoryNotFoundException("Smithbox.Tests.csproj wasn't found above " + AppContext.BaseDirectory);
    }
}
