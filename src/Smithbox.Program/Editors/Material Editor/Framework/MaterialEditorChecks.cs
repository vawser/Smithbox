using Hexa.NET.ImGui;
using System.Numerics;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// MaterialChecks for a file in the bank, plus what only the bank can see, and how findings are shown
/// </summary>
public static class MaterialEditorChecks
{
    public static List<MaterialFinding> CheckFile(MaterialBank bank, IMaterialBinder binder, MaterialFileWrapper file)
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

        return findings;
    }

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
