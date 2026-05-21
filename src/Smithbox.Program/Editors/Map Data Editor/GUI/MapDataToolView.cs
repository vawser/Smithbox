using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.MapDataEditor;

public class MapDataToolView
{
    public MapDataEditorScreen Editor;
    public ProjectEntry Project;

    public MapDataToolView(MapDataEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Draw()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (!CFG.Current.Interface_MapDataEditor_ToolWindow)
            return;

        if (ImGui.Begin("Tools##ToolConfigureWindow_MapDataEditor", UIHelper.GetMainWindowFlags()))
        {
            FocusManager.SetFocus(EditorFocusContext.MapDataEditor_Tools);

            var windowHeight = ImGui.GetWindowHeight();
            var windowWidth = ImGui.GetWindowWidth();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            // Data Transfer
            //if(CFG.Current.MaterialEditor_Tool_Data_Transfer)
            //{
            //    if (ImGui.CollapsingHeader("Data Transfer"))
            //    {
            //        DataTransferTool.Display();
            //    }
            //}
        }

        ImGui.End();
    }
    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            //if (ImGui.MenuItem("Data Transfer"))
            //{
            //    CFG.Current.MaterialEditor_Tool_Data_Transfer = !CFG.Current.MaterialEditor_Tool_Data_Transfer;
            //}
            //UIHelper.ShowActiveStatus(CFG.Current.MaterialEditor_Tool_Data_Transfer);

            ImGui.EndMenu();
        }
    }
    public void ToolMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            // DataTransferTool.DisplayDropdown();

            ImGui.EndMenu();
        }
    }
}
