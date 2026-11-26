#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
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

        ImGui.BeginDisabled(view.IsCheckingGravity);
        if (ImGui.Button($"{LOC.Get("ANIM_Gravity_Check_Again")}##animationGravityCheckAgain"))
            view.CheckGravityAgain();
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Gravity_Check_Again_TT"));

        GUI.Spacer();
    }
}
