using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

public class GparamToolView
{
    private GparamEditorScreen Editor;
    private ProjectEntry Project;

    private GparamDataFinder DataFinder;

    public GparamDataTransferTool DataTransferTool;

    public GparamToolView(GparamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        DataFinder = new(editor, project);

        DataTransferTool = new(editor, project);
    }

    public void DisplayDropdown()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            DataTransferTool.DisplayDropdown();

            ImGui.EndMenu();
        }
    }

    public void Display()
    {
        if (!CFG.Current.Interface_GparamEditor_ToolWindow)
            return;

        if (ImGui.Begin("Tools##ToolConfigureWindow_GparamEditor", UIHelper.GetMainWindowFlags()))
        {
            FocusManager.SetFocus(EditorFocusContext.GparamEditor_Tools);

            var windowWidth = ImGui.GetWindowWidth();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            var activeView = Editor.ViewHandler.ActiveView;

            if (activeView != null)
            {
                if (CFG.Current.Interface_GparamEditor_Tool_DataTransfer)
                {
                    if (ImGui.CollapsingHeader("Data Transfer"))
                    {
                        DataTransferTool.Display();
                    }
                }

                if (CFG.Current.Interface_GparamEditor_Tool_Finder)
                {
                    if (ImGui.CollapsingHeader("Data Finder"))
                    {
                        DataFinder.Display();
                    }
                }

                if (CFG.Current.Interface_GparamEditor_Tool_QuickEdit)
                {
                    if (ImGui.CollapsingHeader("Quick Edit"))
                    {
                        activeView.QuickEditHandler.DisplayInputWindow();
                    }

                    if (ImGui.CollapsingHeader("Quick Edit Commands"))
                    {
                        QuickEditCheatsheet.Display();
                    }
                }

            }

        }

        ImGui.End();
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Quick Edit"))
            {
                CFG.Current.Interface_GparamEditor_Tool_QuickEdit = !CFG.Current.Interface_GparamEditor_Tool_QuickEdit;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_QuickEdit);

            if (ImGui.MenuItem("Finder"))
            {
                CFG.Current.Interface_GparamEditor_Tool_Finder = !CFG.Current.Interface_GparamEditor_Tool_Finder;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_Finder);

            ImGui.EndMenu();
        }
    }
}
