#nullable enable
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Animation Editor's Tools window (see Claude/Documentation/UI.md (Tools window conventions)):
/// Animation → Clip, Root Motion; Transform → Bones.
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
            CFG.Current.Interface.Interface_AnimationEditor_Tool_Root_Motion)
        {
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Animation"));

            if (CFG.Current.Interface.Interface_AnimationEditor_Tool_Clip)
                View.ClipView.OnToolWindow();

            if (CFG.Current.Interface.Interface_AnimationEditor_Tool_Root_Motion)
                View.RootMotionTool.OnToolWindow();
        }

        // Transform: moving and turning bones
        if (CFG.Current.Interface.Interface_AnimationEditor_Tool_Bones)
        {
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Transform"));
            View.BonesView.OnToolWindow();
        }

        Sections.End();
    }
}
