using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The tool's header, drawn above the tabs whichever is open (Navigation Editor UX Analysis, R2): the overlay toggle and
/// set, which viewport mode takes the clicks, the picked face and selected model, and uncommitted vertex edits. So the
/// state that changes what a click in the viewport does is always visible.
/// </summary>
public partial class NavigationTool
{
    private void DisplayHeader()
    {
        // Viewport
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Viewport_Header"), LOC.Get("MAP_Navigation_Viewport_Header_TT"));

        DisplayOverlayToggle();
        DisplayModeStatus();
        DisplaySelectionStatus();
        DisplayPendingStatus();
        DisplayCheckStatus();
    }

    /// <summary>The last check of the chosen map, so its result is seen from any tab; nothing before a check.</summary>
    private void DisplayCheckStatus()
    {
        if (Current == null || Current.MapId != SelectedMap)
            return;

        int errors = Current.Issues.Count(i => !i.Inherited && i.Severity == NavIssueSeverity.Error);
        int warnings = Current.Issues.Count(i => !i.Inherited && i.Severity == NavIssueSeverity.Warning);

        string text = errors == 0 && warnings == 0
            ? LOC.Get("MAP_Navigation_Check_Status_Clean")
            : LOC.Get("MAP_Navigation_Check_Status", errors, warnings);

        if (Current.Stale)
            text += " " + LOC.Get("MAP_Navigation_Check_Status_Stale");

        // A stale result is a note whatever it says; otherwise the colours of the Check tab's counts
        var color = Current.Stale ? UI.Current.ImGui_Invalid_Text_Color
            : errors > 0 ? UI.Current.ImGui_Warning_Text_Color
            : warnings > 0 ? UI.Current.ImGui_Invalid_Text_Color
            : UI.Current.ImGui_Benefit_Text_Color;

        GUI.WrappedTextColored(color, text);
    }

    /// <summary>Show overlay, and the n/o set where the game has an o set.</summary>
    private void DisplayOverlayToggle()
    {
        DisplayGlossaryButton();
        ImGui.SameLine();

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Overlay_Show")}##navigationOverlayShow", ref ShowOverlay))
        {
            if (ShowOverlay)
                OverlayDirty = true;
            else
                ClearOverlay();
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Show_TT"));

        // The set toggle only where the game has an o set (not AC6)
        if (Bank.Profile.HasOSet)
        {
            ImGui.SameLine();
            int set = (int)OverlaySet;
            if (ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Overlay_Set_N")}##navigationSetN", ref set, (int)NavmeshSet.N))
                OverlayDirty = true;
            GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Set_N_TT"));
            ImGui.SameLine();
            if (ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Overlay_Set_O")}##navigationSetO", ref set, (int)NavmeshSet.O))
                OverlayDirty = true;
            GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Set_O_TT"));
            OverlaySet = (NavmeshSet)set;
        }
        else if (OverlaySet != NavmeshSet.N)
        {
            OverlaySet = NavmeshSet.N;
            OverlayDirty = true;
        }
    }

    /// <summary>The mode that takes the viewport's clicks, if any, with a button that stops it.</summary>
    private void DisplayModeStatus()
    {
        string mode = VertexEditActive ? EditModeKey
            : GateMode ? "MAP_Navigation_Mode_Gates"
            : LinkMode ? "MAP_Navigation_Mode_Links"
            : PaintMode ? "MAP_Navigation_Mode_Paint"
            : PickMode ? "MAP_Navigation_Mode_Pick"
            : null;

        if (mode == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Mode_None"));
            return;
        }

        if (ImGui.Button($"{Icons.Stop}##navigationModeStop"))
            StopModes();
        GUI.Tooltip(LOC.Get("MAP_Navigation_Mode_Stop_TT"));
        ImGui.SameLine();

        // The modes only act on the overlay; without it, the clicks are taken but do nothing
        if (!ShowOverlay)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Mode_No_Overlay", LOC.Get(mode)));
        else
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get(mode));
    }

    /// <summary>Turns off every viewport mode, so clicks select map objects again. The picked face stays picked.</summary>
    private void StopModes()
    {
        PickMode = false;
        PaintMode = false;
        LinkMode = false;
        LinkStart = null;
        StopGateMode();
        StopVertexEdit();
    }

    /// <summary>The picked face and the selected model, which the tabs act on.</summary>
    private void DisplaySelectionStatus()
    {
        if (Picked != null)
        {
            if (ImGui.Button($"{Icons.Times}##navigationPickedClear"))
                SetPicked(null);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Picked_Clear_TT"));
            ImGui.SameLine();
            GUI.WrappedText(LOC.Get("MAP_Navigation_Picked_Status", Picked.Face, Picked.File.Name, Picked.Entry.ModelID));
        }

        if (SelectedModelID >= 0)
        {
            if (ImGui.Button($"{Icons.Times}##navigationModelClear"))
            {
                SelectedModelID = -1;
                if (OverlayOnlySelectedModel)
                    OverlayDirty = true;
            }
            GUI.Tooltip(LOC.Get("MAP_Navigation_Model_Clear_TT"));
            ImGui.SameLine();
            GUI.WrappedText(LOC.Get("MAP_Navigation_Model_Status", SelectedModelID));
        }
    }

    /// <summary>Vertex edits that haven't been committed, shown on every tab, as only the Edit tab lists them.</summary>
    private void DisplayPendingStatus()
    {
        if (!HasVertexEdits)
            return;

        int files = VertexFiles.Values.Count(f => f.IsEdited);
        GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color,
            LOC.Get("MAP_Navigation_Pending_Status", files, VertexMap?.Name ?? "-"));
    }
}
