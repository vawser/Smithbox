#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The warning shown with the root motion (Root Motion tool, the Bones tool's root) when the clip's root rises but its
/// time act doesn't disable gravity over the climb: in game the character stays on the ground then
/// (<see cref="AnimationGravityCheck" />). Check Again reads the time acts again, after saving them in the Time Act Editor.
/// </summary>
public static class AnimationGravityWarning
{
    public static void Display(AnimationEditorView view, AnimationClipSource source, EditableClip clip)
    {
        if (view.Bank.Character?.Gravity is not { } gravity)
            return;

        var result = gravity.Check(source.AnimationId, clip, out var rise);
        if (result == AnimationGravityCheck.Result.Fine)
            return;

        var height = rise.Height.ToString("F2");
        var text = result switch
        {
            AnimationGravityCheck.Result.NoTimeAct => LOC.Get("ANIM_Gravity_No_Time_Act", height, rise.First, rise.Peak),
            AnimationGravityCheck.Result.PartlyDisabled => LOC.Get("ANIM_Gravity_Partly", height, rise.First, rise.Peak,
                gravity.WindowsText(source.AnimationId, clip.FrameDuration)),
            _ => LOC.Get("ANIM_Gravity_Not_Disabled", height, rise.First, rise.Peak)
        };

        GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, text);
        GUI.Tooltip(LOC.Get("ANIM_Gravity_TT"));

        if (result != AnimationGravityCheck.Result.NoTimeAct)
            DisplayAddEvent(view, source, clip, gravity);

        ImGui.BeginDisabled(view.IsCheckingGravity);
        if (ImGui.Button($"{LOC.Get("ANIM_Gravity_Check_Again")}##animationGravityCheckAgain"))
            view.CheckGravityAgain();
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Gravity_Check_Again_TT"));

        GUI.Spacer();
    }

    // The clip and window a Disable Gravity event was last sent for, so the button says so until the time acts are read again
    private static (AnimationClipSource Source, int Start, int End)? Sent;

    /// <summary>
    /// Add Disable Gravity: sends the Time Act Editor (which owns the time acts) a ChrActionFlag event with gravity off
    /// over the frames <see cref="AnimationGravityCheck.SuggestedWindow" /> gives, as one undoable step there
    /// (<c>tae/addevent</c>). Not for an entry that imports its events, which belong to another animation.
    /// </summary>
    private static void DisplayAddEvent(AnimationEditorView view, AnimationClipSource source, EditableClip clip,
        AnimationGravityCheck gravity)
    {
        if (AnimationGravityCheck.SuggestedWindow(clip) is not { } window || view.Bank.Character is not { } character)
            return;

        if (Sent is { } sent && sent.Source == source && sent.Start == window.Start && sent.End == window.End)
        {
            ImGui.TextDisabled(LOC.Get("ANIM_Gravity_Add_Sent"));
            return;
        }

        var imported = gravity.TimeActs.ImportedFrom(source.AnimationId) is { } from
            ? character.Clips.Find(c => c.AnimationId == from)?.Name ?? from.ToString()
            : null;

        ImGui.BeginDisabled(imported != null);
        if (ImGui.Button($"{LOC.Get("ANIM_Gravity_Add", window.Start, window.End)}##animationGravityAdd"))
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            var start = window.Start * clip.FrameDuration;
            var end = window.End * clip.FrameDuration;
            EditorCommandQueue.AddCommand($"tae/addevent/{character.SetName}/{source.AnimationId}/" +
                $"{gravity.Flag.Action.ToString(culture)}/{gravity.Flag.Value.ToString(culture)}/" +
                $"{start.ToString("R", culture)}/{end.ToString("R", culture)}");

            Sent = (source, window.Start, window.End);
        }
        ImGui.EndDisabled();
        GUI.Tooltip(imported != null ? LOC.Get("ANIM_Gravity_Add_Imported", imported) : LOC.Get("ANIM_Gravity_Add_TT"));
    }
}
