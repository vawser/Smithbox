using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.AnimEditor;

public class AnimToolWindow
{
    public AnimEditorScreen Editor;
    public ProjectEntry Project;

    public AnimToolWindow(AnimEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        if (!CFG.Current.Interface_AnimEditor_ToolWindow)
            return;

        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        ImGui.SetNextWindowClass(ref GUI.DockGroup_AnimEditor);
        if (ImGui.Begin("Tool Window##animEditorTools", GUI.GetMainWindowFlags()))
        {
            FocusManager.SetFocus(EditorFocusContext.AnimEditor_Tools);

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            if(activeView.IsBehaviorView())
            {

            }

            if (activeView.IsTimeActView())
            {

            }
        }

        ImGui.End();
    }

    public void ViewMenu()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (ImGui.BeginMenu("View"))
        {
            if (activeView.IsBehaviorView())
            {

            }

            if (activeView.IsTimeActView())
            {

            }

            ImGui.EndMenu();
        }
    }

    public void OnMenubar()
    {

    }
}


