using Andre.Formats;
using StudioCore.Application;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.ParamEditor.Upgrader;

/// <summary>A diff file on disk: bundled with Smithbox, or made by the user in the Param Upgrader.</summary>
public sealed class RegulationDiffFile
{
    public ulong FromVersion { get; init; }
    public ulong ToVersion { get; init; }
    public string Path { get; init; }
    public bool Bundled { get; init; }
}

/// <summary>A copy of the game's regulation.bin that Smithbox kept for a version with no newer diff yet.</summary>
public sealed class RegulationSnapshotFile
{
    public ulong Version { get; init; }
    public string Path { get; init; }
}

/// <summary>
/// The version history of a project's game regulation: the diff files, the game's own regulation.bin (the newest
/// version, which the diffs are rebuilt from), and the local snapshots. Smithbox side of the upgrader; the version
/// logic itself is in RegulationUpgradeChain and friends.
/// </summary>
public sealed class RegulationHistory
{
    public const string RegulationFile = "regulation.bin";

    private static readonly Regex DiffName = new(@"^(\d+)-(\d+)\.json$", RegexOptions.IgnoreCase);

    public ProjectEntry Project { get; }
    public UpgraderGame Game { get; }

    private RegulationHistory(ProjectEntry project, UpgraderGame game)
    {
        Project = project;
        Game = game;
    }

    /// <summary>The history for the project, or null when its game has no upgrader support.</summary>
    public static RegulationHistory For(ProjectEntry project)
    {
        var game = GameFor(project.Descriptor.ProjectType);
        return game == null ? null : new RegulationHistory(project, game.Value);
    }

    public static UpgraderGame? GameFor(ProjectType type)
    {
        return type switch
        {
            ProjectType.ER => UpgraderGame.ER,
            ProjectType.AC6 => UpgraderGame.AC6,
            ProjectType.NR => UpgraderGame.NR,
            _ => null
        };
    }

    public string BundledDiffDir => Path.Join(AppContext.BaseDirectory, "Assets", "PARAM",
        ProjectUtils.GetGameDirectory(Project), "Upgrader Diffs");

    private string LocalRoot => Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Smithbox", "Param Upgrader", ProjectUtils.GetGameDirectory(Project));

    /// <summary>Where diffs made in the Param Upgrader are written</summary>
    public string UserDiffDir => Path.Join(LocalRoot, "Diffs");

    public string SnapshotDir => Path.Join(LocalRoot, "Snapshots");

    public RegulationLoadContext CreateContext()
    {
        var paramData = Project.Handler.ParamData;
        var typeInfo = paramData.ParamTypeInfo;

        return new RegulationLoadContext
        {
            Game = Game,
            ParamDefs = paramData.ParamDefs,
            TypeMapping = typeInfo?.Mapping ?? new Dictionary<string, string>(),
            TypeExceptions = typeInfo?.Exceptions ?? new List<string>()
        };
    }

    /// <summary>All diff files, bundled ones first (they win when both cover a step).</summary>
    public List<RegulationDiffFile> ListDiffs()
    {
        var result = new List<RegulationDiffFile>();
        AddDiffs(result, BundledDiffDir, true);
        AddDiffs(result, UserDiffDir, false);
        return result;
    }

    private static void AddDiffs(List<RegulationDiffFile> list, string dir, bool bundled)
    {
        if (!Directory.Exists(dir))
            return;

        foreach (var path in Directory.GetFiles(dir, "*.json"))
        {
            var m = DiffName.Match(Path.GetFileName(path));
            if (!m.Success)
                continue;

            list.Add(new RegulationDiffFile
            {
                FromVersion = ulong.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
                ToVersion = ulong.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture),
                Path = path,
                Bundled = bundled
            });
        }
    }

    public List<RegulationSnapshotFile> ListSnapshots()
    {
        var result = new List<RegulationSnapshotFile>();
        if (!Directory.Exists(SnapshotDir))
            return result;

        foreach (var dir in Directory.GetDirectories(SnapshotDir))
        {
            var file = Path.Join(dir, RegulationFile);
            if (File.Exists(file) && ulong.TryParse(Path.GetFileName(dir), out var version))
                result.Add(new RegulationSnapshotFile { Version = version, Path = file });
        }

        return result.OrderBy(s => s.Version).ToList();
    }

    /// <summary>The game's own regulation.bin (not the project's).</summary>
    public byte[] ReadGameRegulation()
    {
        var fs = Project.VFS.VanillaRealFS;
        if (!fs.FileExists(RegulationFile))
            throw new FileNotFoundException(LOC.Get("PARAM_Upgrader_Error_No_Game_Regulation"));

        return fs.GetFile(RegulationFile).GetData().ToArray();
    }

    /// <summary>
    /// The diffs from <paramref name="from"/> to <paramref name="to"/>, oldest first; null when a step is missing
    /// (<paramref name="missingAt"/> is the version no diff leads to). Only the files on the chain are read.
    /// </summary>
    public List<RegulationDiff> LoadChain(ulong from, ulong to, out ulong missingAt)
    {
        var files = ListDiffs();
        var stubs = files.Select(f => new RegulationDiff { FromVersion = f.FromVersion, ToVersion = f.ToVersion }).ToList();
        var chain = RegulationUpgradeChain.FindChain(stubs, from, to, out missingAt);

        if (chain == null)
            return null;

        return chain.Select(stub =>
        {
            var file = files[stubs.IndexOf(stub)];
            return RegulationDiffIO.Deserialize(File.ReadAllText(file.Path));
        }).ToList();
    }

    /// <summary>
    /// Every vanilla version from <paramref name="from"/> to the game's version, rebuilt from the game's
    /// regulation.bin. Index 0 is <paramref name="from"/>, the last entry is the game's version.
    /// </summary>
    public (List<RegulationDiff> Chain, List<Dictionary<string, Param>> Versions) RebuildFrom(
        ulong from, RegulationLoadContext context, out ulong missingAt)
    {
        var game = RegulationSnapshot.Load(ReadGameRegulation(), context);
        var chain = LoadChain(from, game.Version, out missingAt);

        if (chain == null)
            return (null, null);

        return (chain, RegulationUpgradeChain.RebuildVersions(game.Params, chain, context));
    }

    /// <summary>The params of one older vanilla version, rebuilt from the game's regulation.bin.</summary>
    public Dictionary<string, Param> RebuildVersion(ulong version, out ulong missingAt)
    {
        var (_, versions) = RebuildFrom(version, CreateContext(), out missingAt);
        return versions?[0];
    }

    /// <summary>Writes a diff made in the Param Upgrader to the user diff folder and returns its path.</summary>
    public string SaveUserDiff(RegulationDiff diff)
    {
        Directory.CreateDirectory(UserDiffDir);
        var path = Path.Join(UserDiffDir, $"{diff.FromVersion}-{diff.ToVersion}.json");
        File.WriteAllText(path, RegulationDiffIO.Serialize(diff));
        return path;
    }

    /// <summary>
    /// Keeps a copy of the game's regulation.bin when no diff leads on from its version yet. After the game
    /// updates, the copy is the older side for the diff to the new version. Returns the path when one was made.
    /// </summary>
    public string TrySnapshot(ulong gameVersion)
    {
        if (!CFG.Current.ParamEditor.ParamEditor_Upgrader_Keep_Regulation_Snapshots || gameVersion == 0)
            return null;

        if (ListDiffs().Any(d => d.FromVersion == gameVersion))
            return null;

        var dir = Path.Join(SnapshotDir, gameVersion.ToString(CultureInfo.InvariantCulture));
        var path = Path.Join(dir, RegulationFile);
        if (File.Exists(path))
            return null;

        var data = ReadGameRegulation();

        // Only keep it when it really is that version
        using (var binder = RegulationParamLoader.ReadBinder(Game, data))
        {
            if (RegulationParamLoader.ReadVersion(binder) != gameVersion)
                return null;
        }

        Directory.CreateDirectory(dir);
        File.WriteAllBytes(path, data);
        return path;
    }
}
