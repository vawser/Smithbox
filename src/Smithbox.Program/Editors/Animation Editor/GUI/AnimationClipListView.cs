#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using System;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Clips window: the open character's clips. An edited clip is marked *, a clip added under a new ID (not saved
/// yet) is marked (new).
/// </summary>
public class AnimationClipListView
{
    private readonly AnimationEditorView View;
    private string Filter = "";

    public AnimationClipListView(AnimationEditorView view)
    {
        View = view;
    }

    public void ClearFilter() => Filter = "";

    public void Display()
    {
        var bank = View.Bank;

        if (bank.Character is not { } character)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clips_No_Character"));
            return;
        }

        if (!character.SetName.Equals(character.Name, StringComparison.OrdinalIgnoreCase))
            GUI.WrappedText(LOC.Get("ANIM_Clips_Shared_Set", character.SetName));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##animationClipFilter", LOC.Get("ANIM_Filter_Hint"), ref Filter, 64);

        ImGui.TextDisabled(LOC.Get("ANIM_Clips_Count", character.Clips.Count));

        if (!ImGui.BeginChild("animationClipList", default, ImGuiChildFlags.Borders))
        {
            ImGui.EndChild();
            return;
        }

        // Characters have thousands of clips (the ER player over 1,300 in one anibnd), so only the rows in view are drawn
        var shown = Filter.Length == 0
            ? character.Clips
            : character.Clips.FindAll(c => c.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase));

        var clipper = new ImGuiListClipper();
        clipper.Begin(shown.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var clip = shown[i];
                var label = clip.IsNew
                    ? $"{clip.Name} {LOC.Get("ANIM_Clips_New_Marker")}"
                    : bank.IsEdited(clip) ? $"{clip.Name} *" : clip.Name;

                if (ImGui.Selectable($"{label}##animationClip_{clip.BinderId}", View.SelectedClip == clip))
                    View.SelectClip(clip);

                if (ImGui.IsItemHovered())
                    GUI.Tooltip(clip.BinderPath);
            }
        }

        clipper.End();
        ImGui.EndChild();
    }
}
