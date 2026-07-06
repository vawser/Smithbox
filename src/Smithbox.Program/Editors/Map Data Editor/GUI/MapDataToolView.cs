using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.MapDataEditor;

public class MapDataToolView
{
    public MapDataEditorView View;
    public ProjectEntry Project;

    public MapDataToolView(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Draw()
    {
        if (!CFG.Current.Interface_MapDataEditor_ToolWindow)
            return;

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
