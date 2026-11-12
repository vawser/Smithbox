using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// How the comparison with the game's version is shown in the lists and the graph. Changed and new entries get the
/// colours the Param Editor uses for changed and added rows. The * after a name still means unsaved.
/// </summary>
public static class TimeActVanillaDisplay
{
    public static bool Enabled => CFG.Current.TimeActEditor_Display_Vanilla_Markers;

    public static Vector4? GetColor(TimeActVanillaStatus status) => status switch
    {
        TimeActVanillaStatus.Modified => UI.Current.ImGui_PrimaryChanged_Text,
        TimeActVanillaStatus.Added => UI.Current.ImGui_AuxAdded_Text,
        _ => null
    };

    public static string GetStatusText(TimeActVanillaStatus status, bool reading) => status switch
    {
        TimeActVanillaStatus.Same => LOC.Get("TAE_Vanilla_Status_Same"),
        TimeActVanillaStatus.Modified => LOC.Get("TAE_Vanilla_Status_Modified"),
        TimeActVanillaStatus.Added => LOC.Get("TAE_Vanilla_Status_Added"),
        _ => reading ? LOC.Get("TAE_Vanilla_Status_Reading") : LOC.Get("TAE_Vanilla_Status_Unknown")
    };

    /// <summary>
    /// Colours the next list item for its status. Returns whether a colour was pushed, to pop after the item.
    /// </summary>
    public static bool PushColor(TimeActVanillaStatus status)
    {
        if (!Enabled || GetColor(status) is not { } color)
            return false;

        ImGui.PushStyleColor(ImGuiCol.Text, color);

        return true;
    }
}
