using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Keybinds;
using System.Globalization;

namespace StudioCore.Editors.Common;

/// <summary>
/// Quick links from a field holding an effect id (the fields the meta tags with the Particles alias) to that effect in
/// the FX Editor, as <see cref="ParamReferenceHelper"/> does for param rows: a "Go to" entry in the field's context
/// menu, and Ctrl+click. The jump is the fx/select/&lt;id&gt; command, which FxEditorScreen handles.
/// Shared by the Param, Map and Time Act editors.
/// </summary>
public static class FxReferenceHelper
{
    /// <summary>Whether the project has an FX Editor to jump to.</summary>
    public static bool IsAvailable(ProjectEntry project) =>
        project?.Descriptor != null
        && project.Descriptor.EnableFxEditor
        && EditorConditions.SupportsFxEditor(project.Descriptor.ProjectType)
        && project.Handler?.FxEditor != null;

    /// <summary>The effect id a field value holds; 0 and below (-1 is "none" in most fields) are no effect.</summary>
    public static bool TryGetId(object value, out int id)
    {
        id = 0;
        if (value == null)
            return false;

        try
        {
            var n = Convert.ToInt64(value, CultureInfo.InvariantCulture);
            if (n <= 0 || n > int.MaxValue)
                return false;
            id = (int)n;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// The context menu entry: go to the effect, or say that no sfx binder holds it once the FX Editor has indexed them.
    /// Call inside an open popup.
    /// </summary>
    public static void ContextMenu(ProjectEntry project, object value)
    {
        if (!IsAvailable(project) || !TryGetId(value, out var id))
            return;

        var stem = $"f{id:D9}";
        var bank = project.Handler.FxData?.Bank;

        if (bank != null && bank.IsIndexed && !bank.Entries.ContainsKey(id))
        {
            ImGui.TextDisabled(LOC.Get("FX_Reference_Missing", stem));
            return;
        }

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
        if (ImGui.Selectable($"{LOC.Get("FX_Reference_Go_To", stem)}##fxRefGoTo"))
            JumpTo(id);
        ImGui.PopStyleColor();
        GUI.Tooltip(LOC.Get("FX_Reference_Go_To_TT"));
    }

    /// <summary>Ctrl+click on the last item jumps to the effect. Call right after the item it applies to.</summary>
    public static void Click(ProjectEntry project, object value)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && InputManager.HasCtrlDown()
            && IsAvailable(project) && TryGetId(value, out var id))
        {
            JumpTo(id);
        }
    }

    public static void JumpTo(int id) => EditorCommandQueue.AddCommand($"fx/select/{id}");
}
