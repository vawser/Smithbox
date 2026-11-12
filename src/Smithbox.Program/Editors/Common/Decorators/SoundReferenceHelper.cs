using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.SoundEditor;
using StudioCore.Keybinds;
using System.Globalization;

namespace StudioCore.Editors.Common;

/// <summary>
/// Quick links from a field holding a sound ID (the fields the meta tags with the Sounds alias) to that sound in the
/// Sound Editor, as <see cref="FxReferenceHelper"/> does for effects: Play / Open entries in the field's context menu,
/// and Ctrl+click to open. The field gives only the number, so every type letter with an event is offered
/// (usually one). Uses the Sound Editor's index (soundbanksinfo.mobnkinfo / .fev names), built on first use.
/// Shared by the Param, Map and Time Act editors.
/// </summary>
public static class SoundReferenceHelper
{
    /// <summary>Whether the project has a Sound Editor to jump to.</summary>
    public static bool IsAvailable(ProjectEntry project) =>
        project?.Descriptor != null
        && project.Descriptor.EnableSoundEditor
        && EditorConditions.SupportsSoundEditor(project.Descriptor.ProjectType)
        && project.Handler?.SoundEditor != null
        && project.Handler.SoundData?.Index != null;

    /// <summary>The sound ID a field value holds; 0 and below (-1 is "none") are no sound.</summary>
    public static bool TryGetId(object value, out string id)
    {
        id = null;
        if (value == null)
            return false;

        try
        {
            var n = Convert.ToInt64(value, CultureInfo.InvariantCulture);
            if (n <= 0)
                return false;
            id = n.ToString(CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>The context menu entries: play or open each event of the sound ID. Call inside an open popup.</summary>
    public static void ContextMenu(ProjectEntry project, object value)
    {
        if (!IsAvailable(project) || !TryGetId(value, out var id))
            return;

        var index = project.Handler.SoundData.Index;
        index.EnsureBuilt();
        if (!index.IsBuilt)
        {
            ImGui.TextDisabled(LOC.Get(index.Error != null ? "SOUND_Reference_Index_Failed" : "SOUND_Reference_Indexing"));
            return;
        }

        var matches = index.Find(id);
        if (matches.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("SOUND_Reference_Missing", id));
            return;
        }

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
        int i = 0;
        foreach (var m in matches.Take(8))
        {
            if (ImGui.Selectable($"{LOC.Get("SOUND_Reference_Play", m.Label)}##soundRefPlay{i}"))
                EditorCommandQueue.AddCommand(SoundIndex.Command(m, true));
            GUI.Tooltip(LOC.Get("SOUND_Reference_Play_TT"));

            if (ImGui.Selectable($"{LOC.Get("SOUND_Reference_Go_To", m.Label)}##soundRefGoTo{i}"))
                EditorCommandQueue.AddCommand(SoundIndex.Command(m, false));
            GUI.Tooltip(LOC.Get("SOUND_Reference_Go_To_TT"));
            i++;
        }
        ImGui.PopStyleColor();

        if (matches.Count > 8)
        {
            if (ImGui.Selectable($"{LOC.Get("SOUND_Reference_More", matches.Count - 8)}##soundRefMore"))
                EditorCommandQueue.AddCommand($"sound/find/{id}");
        }
    }

    /// <summary>Ctrl+click on the last item opens the sound (its first event). Call right after the item it applies to.</summary>
    public static void Click(ProjectEntry project, object value)
    {
        if (!ImGui.IsItemClicked(ImGuiMouseButton.Left) || !InputManager.HasCtrlDown() || !IsAvailable(project) || !TryGetId(value, out var id))
            return;

        var index = project.Handler.SoundData.Index;
        index.EnsureBuilt();
        var matches = index.Find(id);
        if (matches.Count == 1)
            EditorCommandQueue.AddCommand(SoundIndex.Command(matches[0], false));
        else
            EditorCommandQueue.AddCommand($"sound/find/{id}");
    }
}
