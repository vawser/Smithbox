#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Configuration tool (Viewport category): how the viewport shows the character. The mesh's opacity, the skeleton
/// overlay (joints, bone trail, onion skin), the root motion path and Travel, and the Compare With skeleton.
/// Each setting is kept in the config, so it stays between sessions.
/// </summary>
public class AnimationConfigurationTool
{
    private readonly AnimationEditorView View;

    public AnimationConfigurationTool(AnimationEditorView view)
    {
        View = view;
    }

    /// <summary>As a tool of the Tools window</summary>
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ANIM_Tool_Configuration")}##animationConfigurationToolHeader"))
            return;

        ImGui.BeginChild("AnimationConfigurationToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        var cfg = CFG.Current.Interface;

        // Mesh
        GUI.SimpleHeader(LOC.Get("ANIM_Config_Mesh_Header"), "");

        ImGui.Text(LOC.Get("ANIM_Viewport_Mesh_Opacity"));
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##animationMeshOpacity", ref cfg.Interface_AnimationEditor_Mesh_Opacity, 0f, 1f, "%.2f");
        GUI.Tooltip(LOC.Get("ANIM_Viewport_Mesh_Opacity_TT"));

        // Skeleton
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("ANIM_Config_Skeleton_Header"), "");

        ImGui.Checkbox($"{LOC.Get("ANIM_Menu_Show_Skeleton")}##animationConfigSkeleton", ref cfg.Interface_AnimationEditor_Skeleton);

        // The overlay's parts are drawn with the skeleton only
        ImGui.BeginDisabled(!cfg.Interface_AnimationEditor_Skeleton);

        ImGui.Checkbox($"{LOC.Get("ANIM_Viewport_Show_Joints")}##animationConfigJoints", ref cfg.Interface_AnimationEditor_Skeleton_Joints);

        ImGui.Text(LOC.Get("ANIM_Viewport_Joint_Size"));
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##animationJointSize", ref cfg.Interface_AnimationEditor_Joint_Size, 0.25f, 4f, "%.2fx");
        GUI.Tooltip(LOC.Get("ANIM_Viewport_Joint_Size_TT"));

        ImGui.Checkbox($"{LOC.Get("ANIM_Bone_Trail")}##animationConfigBoneTrail", ref cfg.Interface_AnimationEditor_Bone_Trail);
        GUI.Tooltip(LOC.Get("ANIM_Bone_Trail_TT"));

        // Onion skin: the clip's own frames before (red) and after (green) the playing one
        ImGui.Checkbox($"{LOC.Get("ANIM_Onion_Skin")}##animationConfigOnionSkin", ref cfg.Interface_AnimationEditor_Onion_Skin);
        GUI.Tooltip(LOC.Get("ANIM_Onion_Skin_TT"));

        if (cfg.Interface_AnimationEditor_Onion_Skin)
        {
            ImGui.Indent();

            ImGui.Text(LOC.Get("ANIM_Onion_Count"));
            ImGui.SetNextItemWidth(-1);
            ImGui.SliderInt("##animationOnionCount", ref cfg.Interface_AnimationEditor_Onion_Count, 1, AnimationSkeletonOverlay.MaxOnion);
            GUI.Tooltip(LOC.Get("ANIM_Onion_Count_TT"));

            ImGui.Text(LOC.Get("ANIM_Onion_Step"));
            ImGui.SetNextItemWidth(-1);
            ImGui.SliderInt("##animationOnionStep", ref cfg.Interface_AnimationEditor_Onion_Step, 1, 30);
            GUI.Tooltip(LOC.Get("ANIM_Onion_Step_TT"));

            ImGui.Unindent();
        }

        ImGui.EndDisabled();

        // Root motion
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("ANIM_Config_Root_Header"), "");

        ImGui.Checkbox($"{LOC.Get("ANIM_Viewport_Show_Root_Path")}##animationConfigRootPath", ref cfg.Interface_AnimationEditor_Root_Path);
        GUI.Tooltip(LOC.Get("ANIM_Viewport_Show_Root_Path_TT"));

        ImGui.Checkbox($"{LOC.Get("ANIM_Viewport_Apply_Root_Motion")}##animationConfigApplyRoot", ref cfg.Interface_AnimationEditor_Apply_Root_Motion);
        GUI.Tooltip(LOC.Get("ANIM_Viewport_Apply_Root_Motion_TT"));

        // Compare With
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("ANIM_Compare_Menu"), LOC.Get("ANIM_Compare_Menu_TT"));
        DisplayCompare();
    }

    /// <summary>
    /// Compare With: the ghost skeleton played alongside the edited clip (W5), the clip as the anibnd has it or as the
    /// game's own file has it. Says so when the selected clip has no such version (a clip the game doesn't have).
    /// </summary>
    private void DisplayCompare()
    {
        void Option(AnimationCompareSource value, string key)
        {
            var current = CFG.Current.Interface.Interface_AnimationEditor_Compare;
            if (ImGui.RadioButton($"{LOC.Get(key)}##animationCompare_{value}", current == value))
                CFG.Current.Interface.Interface_AnimationEditor_Compare = value;
            GUI.Tooltip(LOC.Get($"{key}_TT"));
        }

        Option(AnimationCompareSource.None, "ANIM_Compare_None");
        ImGui.SameLine();
        Option(AnimationCompareSource.Anibnd, "ANIM_Compare_Anibnd");
        ImGui.SameLine();
        Option(AnimationCompareSource.Vanilla, "ANIM_Compare_Vanilla");

        var compare = CFG.Current.Interface.Interface_AnimationEditor_Compare;
        if (compare != AnimationCompareSource.None && View.SelectedClip is { } source && View.Bank.GetCompare(source, compare) == null)
            ImGui.TextDisabled(LOC.Get("ANIM_Compare_Missing"));

        if (compare != AnimationCompareSource.None && !CFG.Current.Interface.Interface_AnimationEditor_Skeleton)
            ImGui.TextDisabled(LOC.Get("ANIM_Compare_Needs_Skeleton"));
    }
}
