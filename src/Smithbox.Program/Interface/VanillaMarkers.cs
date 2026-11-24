using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Application;

/// <summary>
/// How every editor shows a <see cref="VanillaStatus" />: the shared Modified / Added text colour and the tooltip.
/// By the user's choice there are no icons: a modified or added item is marked by its colour alone. All of it is off
/// when Interface_Vanilla_Highlight is off, so one toggle covers every editor.
/// Plan: Claude/Documentation/Vanilla Modified-Added Parity.md.
/// </summary>
public static class VanillaMarkers
{
    /// <summary>Whether vanilla highlighting is on (the global toggle, and an editor's own toggle if it has one)</summary>
    public static bool Enabled(bool editorToggle = true) => CFG.Current.Interface.Interface_Vanilla_Highlight && editorToggle;

    /// <summary>Whether this status gets a colour and tooltip at all</summary>
    public static bool IsMarked(VanillaStatus status) => status is VanillaStatus.Modified or VanillaStatus.Added;

    /// <summary>The theme colour for a Modified or Added status; null for the rest</summary>
    public static Vector4? Colour(VanillaStatus status) => status switch
    {
        VanillaStatus.Modified => UI.Current.ImGui_Vanilla_Modified_Text,
        VanillaStatus.Added => UI.Current.ImGui_Vanilla_Added_Text,
        _ => null
    };

    /// <summary>The shared tooltip line for a status, or null when there is nothing to say</summary>
    public static string? TooltipText(VanillaStatus status) => status switch
    {
        VanillaStatus.Modified => LOC.Get("EDITOR_Vanilla_Modified_TT"),
        VanillaStatus.Added => LOC.Get("EDITOR_Vanilla_Added_TT"),
        VanillaStatus.NoVanilla => LOC.Get("EDITOR_Vanilla_NoVanilla_TT"),
        _ => null
    };

    /// <summary>
    /// Pushes the status colour as ImGuiCol.Text for the next item. True if a colour was pushed: pop it with
    /// <see cref="PopColour" /> after the item. Nothing is pushed when highlighting is off or the status is unmarked.
    /// </summary>
    public static bool PushColour(VanillaStatus status, bool editorToggle = true)
    {
        if (!Enabled(editorToggle) || Colour(status) is not { } colour)
            return false;

        ImGui.PushStyleColor(ImGuiCol.Text, colour);
        return true;
    }

    /// <summary>Pops the colour <see cref="PushColour" /> pushed, if it did</summary>
    public static void PopColour(bool pushed)
    {
        if (pushed)
            ImGui.PopStyleColor();
    }

    /// <summary>The shared tooltip for the previous item, when it is hovered and the status has one</summary>
    public static void Tooltip(VanillaStatus status)
    {
        if (TooltipText(status) is { } text)
            GUI.Tooltip(text);
    }

    /// <summary>
    /// A container's status from its items, for a container the game has: Modified when any item is Modified or
    /// Added (an added entry modifies the file, it doesn't make the file new), Unchanged when every item is, and
    /// None while any is still unknown. A container the game lacks is Added on its own account, not through this.
    /// </summary>
    public static VanillaStatus RollUp(IEnumerable<VanillaStatus> items)
    {
        var unknown = false;

        foreach (var s in items)
        {
            if (IsMarked(s))
                return VanillaStatus.Modified;

            if (s != VanillaStatus.Unchanged)
                unknown = true;
        }

        return unknown ? VanillaStatus.None : VanillaStatus.Unchanged;
    }
}
