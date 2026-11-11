using Hexa.NET.ImGui;
using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// MaterialChecks for a file in the bank, plus what only the bank can see, and how findings are shown
/// </summary>
public static class MaterialEditorChecks
{
    public static List<MaterialFinding> CheckFile(MaterialBank bank, IMaterialBinder binder, MaterialFileWrapper file,
        MaterialEditorScreen editor = null)
    {
        if (file == null)
            return new();

        object material = file switch
        {
            MTDFileWrapper m => m.MTD,
            MATBINFileWrapper m => m.MATBIN,
            _ => null
        };

        var findings = MaterialChecks.Check(material);

        // A short name held by another binder too: only one copy is found by models
        if (bank != null && binder != null)
        {
            var shortName = MaterialBank.ShortNameOf(file.Key);
            var winner = bank.FindReference(binder.SourceType, shortName);
            var sharedElsewhere = bank.CountBindersWithShortName(binder.SourceType, shortName) > 1;

            if (sharedElsewhere && winner != null)
            {
                findings.Add(new(MaterialCheckSeverity.Warning, "Shared_Name", "", winner.BinderEntry.Filename));
            }
        }

        if (editor != null && binder != null)
            CheckTexturePaths(editor, binder, file, material, findings);

        return findings;
    }

    /// <summary>
    /// Texture paths the project set that no texture in the project has. Vanilla materials name many textures the
    /// game doesn't ship (ER 16% of names, NR and DB_NT more), so a path is only checked when it differs from the
    /// game's file, or the game's file doesn't have it: the check stays silent on vanilla.
    /// </summary>
    private static void CheckTexturePaths(MaterialEditorScreen editor, IMaterialBinder binder, MaterialFileWrapper file,
        object material, List<MaterialFinding> findings)
    {
        var index = editor.Textures;
        if (!index.HasScan)
            return;

        var vanillaFile = editor.Vanilla.FindVanilla(binder, file.Key);

        // Without the vanilla files loaded the check can't tell vanilla paths apart, so it waits
        if (!editor.Vanilla.IsReady)
            return;

        var vanillaPaths = vanillaFile == null
            ? new Dictionary<string, string>()
            : MaterialQueryMatcher.Entries(MaterialVanilla.MaterialOf(vanillaFile))
                .Select(e => (name: MaterialVanilla.EntryName(e.entry), path: PathOf(e.entry)))
                .Where(e => e.path != null)
                .GroupBy(e => e.name).ToDictionary(g => g.Key, g => g.First().path);

        foreach (var (entry, _) in MaterialQueryMatcher.Entries(material))
        {
            var path = PathOf(entry);
            if (string.IsNullOrWhiteSpace(path))
                continue;

            var name = MaterialVanilla.EntryName(entry);
            if (vanillaPaths.TryGetValue(name, out var vanillaPath) && string.Equals(vanillaPath, path, StringComparison.OrdinalIgnoreCase))
                continue;

            var texture = MaterialTextureIndex.Stem(path);
            if (!index.Contains(texture))
                findings.Add(new(MaterialCheckSeverity.Warning, "Texture_Not_Found", name, name, texture));
        }
    }

    private static string PathOf(object entry) => entry switch
    {
        MATBIN.Sampler s => s.Path,
        MTD.Texture t => t.Path,
        _ => null
    };

    public static string Message(MaterialFinding finding) => LOC.Get($"MAT_Check_{finding.Code}", finding.Args);

    public static Vector4 Color(MaterialFinding finding) => finding.Severity is MaterialCheckSeverity.Error
        ? UI.Current.ImGui_Warning_Text_Color
        : UI.Current.ImGui_Invalid_Text_Color;

    /// <summary>
    /// One finding as a clickable line. Returns true when clicked.
    /// </summary>
    public static bool DisplayFinding(MaterialFinding finding, string id)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Color(finding));
        var clicked = ImGui.Selectable($"{Icons.ExclamationTriangle} {Message(finding)}##{id}");
        ImGui.PopStyleColor();

        GUI.Tooltip(LOC.Get("MAT_Check_Finding_TT"));

        return clicked;
    }
}
