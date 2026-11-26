#nullable enable
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Animation Editor's Tools window (see Claude/Documentation/UI.md (Tools window conventions)):
/// Animation → Clip, Root Motion, Batch; Transform → Bones; Viewport → Configuration, Weapon Assignment,
/// Grid Configuration (the shared ViewportGridTool, saved in InterfaceSettings.Interface_AnimationEditor_Grids).
/// </summary>
public class AnimationToolWindow
{
    private readonly AnimationEditorView View;
    private readonly ToolSections Sections = new();

    public AnimationToolWindow(AnimationEditorView view)
    {
        View = view;
    }

    public void Display()
    {
        Sections.Begin();

        // Animation: the clip as a whole
        if (CFG.Current.Interface.Interface_AnimationEditor_Tool_Clip ||
            CFG.Current.Interface.Interface_AnimationEditor_Tool_Root_Motion ||
            CFG.Current.Interface.Interface_AnimationEditor_Tool_Batch)
        {
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Animation"));

            if (CFG.Current.Interface.Interface_AnimationEditor_Tool_Clip)
                View.ClipView.OnToolWindow();

            if (CFG.Current.Interface.Interface_AnimationEditor_Tool_Root_Motion)
                View.RootMotionTool.OnToolWindow();

            if (CFG.Current.Interface.Interface_AnimationEditor_Tool_Batch)
                View.BatchTool.OnToolWindow();
        }

        // Transform: moving and turning bones
        if (CFG.Current.Interface.Interface_AnimationEditor_Tool_Bones)
        {
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Transform"));
            View.BonesView.OnToolWindow();
        }

        // Viewport: how the character is shown
        if (CFG.Current.Interface.Interface_AnimationEditor_Tool_Configuration ||
            CFG.Current.Interface.Interface_AnimationEditor_Tool_Weapon_Assignment ||
            CFG.Current.Interface.Interface_AnimationEditor_Tool_Grid)
        {
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));

            if (CFG.Current.Interface.Interface_AnimationEditor_Tool_Configuration)
                View.ConfigurationTool.OnToolWindow();

            if (CFG.Current.Interface.Interface_AnimationEditor_Tool_Weapon_Assignment)
                View.WeaponAssignmentTool.OnToolWindow();

            if (CFG.Current.Interface.Interface_AnimationEditor_Tool_Grid)
                View.GridTool.OnToolWindow();
        }

        Sections.End();
    }
}
