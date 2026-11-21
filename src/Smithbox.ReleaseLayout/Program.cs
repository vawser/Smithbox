using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SmithboxReleaseLayout;

/// <summary>
/// Usage: Smithbox.ReleaseLayout &lt;release folder&gt; &lt;app name&gt;[;&lt;app name&gt;...]
///
/// Moves every supporting binary at the top of the release folder into Libs, and gives each
/// deps.json asset that moved a "localPath" of "Libs/&lt;file&gt;". The .NET host (9 and later)
/// resolves app-local assets through localPath, so the runtime, NuGet packages and native
/// libraries load from Libs while AppContext.BaseDirectory stays the top-level folder.
///
/// What has to stay at the top level, for each app: the apphost (.exe), the entry assembly
/// (the host looks for it beside the apphost), its deps.json and runtimeconfig.json, and the
/// host libraries hostfxr and hostpolicy, which the apphost loads before reading deps.json.
/// Folders (Assets, Res, Licenses, ...) and non-binary files are left where they are.
/// </summary>
public static class Program
{
    private const string LibsFolderName = "Libs";

    private static readonly HashSet<string> HostLibraries = new(StringComparer.OrdinalIgnoreCase)
    {
        "hostfxr.dll", "hostpolicy.dll",
        "libhostfxr.so", "libhostpolicy.so",
        "libhostfxr.dylib", "libhostpolicy.dylib",
    };

    // Binaries that are not in deps.json but still belong with the libraries:
    // native DLLs copied as content, symbol files, and import libraries.
    private static readonly HashSet<string> BinaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll", ".so", ".dylib", ".pdb", ".lib", ".exp",
    };

    // Asset groups in deps.json that resolve to app-local files. "resources" (satellite
    // assemblies) live in culture subfolders and are left alone.
    private static readonly string[] AssetGroups = { "runtime", "native" };

    public static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Usage: Smithbox.ReleaseLayout <release folder> <app name>[;<app name>...]");
            return 1;
        }

        try
        {
            Run(Path.GetFullPath(args[0]), args[1].Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            return 0;
        }
        catch (Exception e)
        {
            // MSBuild's Exec task turns "error :" lines into build errors
            Console.Error.WriteLine($"error : Smithbox.ReleaseLayout: {e.Message}");
            return 1;
        }
    }

    private static void Run(string releaseDir, string[] appNames)
    {
        if (!Directory.Exists(releaseDir))
            throw new DirectoryNotFoundException($"Release folder not found: {releaseDir}");

        var libsDir = Path.Join(releaseDir, LibsFolderName);
        if (Directory.Exists(libsDir))
            throw new InvalidOperationException($"{libsDir} already exists: the release folder should be freshly assembled.");

        var depsFiles = new List<string>();
        var keepAtRoot = new HashSet<string>(HostLibraries, StringComparer.OrdinalIgnoreCase);

        foreach (var app in appNames)
        {
            var depsFile = Path.Join(releaseDir, $"{app}.deps.json");
            if (!File.Exists(depsFile))
                throw new FileNotFoundException($"No deps.json for '{app}' in the release folder: {depsFile}");

            depsFiles.Add(depsFile);

            keepAtRoot.Add($"{app}.exe");
            keepAtRoot.Add(app); // apphost on Linux and macOS
            keepAtRoot.Add($"{app}.dll");
            keepAtRoot.Add($"{app}.pdb");
            keepAtRoot.Add($"{app}.deps.json");
            keepAtRoot.Add($"{app}.runtimeconfig.json");
        }

        var documents = depsFiles.ToDictionary(f => f, f => JsonNode.Parse(File.ReadAllText(f))!);

        // Files the host resolves from the top level of the folder, according to any app's deps.json
        var listedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var asset in documents.Values.SelectMany(EnumerateAssets))
            listedFiles.Add(Path.GetFileName(asset.Path));

        var toMove = Directory.EnumerateFiles(releaseDir)
            .Select(Path.GetFileName)
            .OfType<string>()
            .Where(name => !keepAtRoot.Contains(name))
            .Where(name => listedFiles.Contains(name) || BinaryExtensions.Contains(Path.GetExtension(name)))
            .ToList();

        Directory.CreateDirectory(libsDir);
        foreach (var name in toMove)
            File.Move(Path.Join(releaseDir, name), Path.Join(libsDir, name));

        var moved = new HashSet<string>(toMove, StringComparer.OrdinalIgnoreCase);

        foreach (var (depsFile, document) in documents)
        {
            var patched = 0;
            foreach (var asset in EnumerateAssets(document))
            {
                var fileName = Path.GetFileName(asset.Path);
                if (!moved.Contains(fileName))
                    continue;

                asset.Properties["localPath"] = $"{LibsFolderName}/{fileName}";
                patched++;
            }

            File.WriteAllText(depsFile, document.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Smithbox.ReleaseLayout: {Path.GetFileName(depsFile)}: {patched} assets now resolve from {LibsFolderName}/");
        }

        Console.WriteLine($"Smithbox.ReleaseLayout: moved {toMove.Count} files into {libsDir}");

        // An executable that is not one of the apps is usually left over from an old build
        // (for example an earlier assembly name) and would no longer start without its files.
        foreach (var exe in Directory.EnumerateFiles(releaseDir, "*.exe").Select(Path.GetFileName))
        {
            if (!keepAtRoot.Contains(exe!))
                Console.WriteLine($"warning : Smithbox.ReleaseLayout: '{exe}' is not one of the release apps; it is probably left over from an older build. Clean the project's bin folder.");
        }
    }

    private readonly record struct Asset(string Path, JsonObject Properties);

    /// <summary>
    /// Every runtime and native asset in a deps.json: targets -> library -> group -> asset path.
    /// </summary>
    private static IEnumerable<Asset> EnumerateAssets(JsonNode document)
    {
        if (document["targets"] is not JsonObject targets)
            yield break;

        foreach (var (_, target) in targets)
        {
            if (target is not JsonObject libraries)
                continue;

            foreach (var (_, library) in libraries)
            {
                if (library is not JsonObject libraryObject)
                    continue;

                foreach (var group in AssetGroups)
                {
                    if (libraryObject[group] is not JsonObject assets)
                        continue;

                    foreach (var (path, properties) in assets)
                    {
                        if (properties is JsonObject propertiesObject)
                            yield return new Asset(path, propertiesObject);
                    }
                }
            }
        }
    }
}
