using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.ModelEditor;


public class ModelExportTool
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public ModelSelectionMenu SelectionMenu;

    private string CollisionToolDir = "";

    public ModelExportTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        SelectionMenu = new(view, project);
    }

    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader($"{LOC.Get("MODEL_ModelExport_Header")}##modelExportHeader"))
        {
            ImGui.BeginChild("ModelExportOverallSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("MODEL_ModelExport_Hint"));
            GUI.Spacer();

            ImGui.BeginChild("ModelExportSection");
            Display();
            ImGui.EndChild();

            ImGui.EndChild();
        }
    }

    public void Display()
    {
        GUI.MultiButtonInput("flverActions",
            "selectFlver",
            LOC.Get("HAVOK_CollisionGen_Select_Flver_Action"),
            LOC.Get("HAVOK_CollisionGen_Select_Flver_Action_TT"),
            SelectFlverSource);

        GUI.Spacer();
        if (SelectionMenu.FlverPath != "")
        {
            GUI.WrappedText(LOC.Get("MODEL_ModelExport_Flver_Current_Source_FLVER", Path.GetFileName(SelectionMenu.FlverPath)));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MODEL_ModelExport_Flver_Current_Source_FLVER_None"));
        }

        if (SelectionMenu.SourceFlver != null)
        {
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("MODEL_ModelExport_Flver_Meshes_Header"),
                LOC.Get("MODEL_ModelExport_Flver_Meshes_Header_TT"));

            if (ImGui.Button($"{LOC.Get("MODEL_ModelExport_Select_All_Meshes")}##flverMeshSelectAll"))
            {
                Array.Fill(SelectionMenu.FlverMeshSelection, true);
            }
            ImGui.SameLine();
            if (ImGui.Button($"{LOC.Get("MODEL_ModelExport_Select_No_Meshes")}##flverMeshSelectNone"))
            {
                Array.Fill(SelectionMenu.FlverMeshSelection, false);
            }

            ImGui.BeginChild("flverMeshSelectionList", new Vector2(0, 150), ImGuiChildFlags.Borders);

            for (int i = 0; i < SelectionMenu.SourceFlver.Meshes.Count; i++)
            {
                var mesh = SelectionMenu.SourceFlver.Meshes[i];
                var material = SelectionMenu.SourceFlver.Materials[mesh.MaterialIndex];
                var label = LOC.Get("MODEL_ModelExport_Flver_Mesh_Entry", i, material.Name, mesh.Vertices.Count);

                var selected = SelectionMenu.FlverMeshSelection[i];
                if (ImGui.Checkbox($"{label}##flverMesh{i}", ref selected))
                {
                    SelectionMenu.FlverMeshSelection[i] = selected;
                }
            }

            ImGui.EndChild();
        }

        // Collision Tool Directory
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_ModelExport_ColTool_Header"),
            LOC.Get("MODEL_ModelExport_ColTool_Header_TT"));

        GUI.SinglelineTextInputWithHint("colToolDir", ref CollisionToolDir,
            LOC.Get("MODEL_ColTool_Dir_Hint"));

        GUI.MultiButtonInput("csvImportDirActions",
            "setDirectory",
            LOC.Get("MODEL_ModelExport_SetColToolDirectory"),
            LOC.Get("MODEL_ModelExport_SetColToolDirectory_TT"),
            SetColToolDirectory,

            "openDirectory",
            LOC.Get("MODEL_ModelExport_OpenColToolDirectory"),
            LOC.Get("MODEL_ModelExport_OpenColToolDirectory_TT"),
            OpenColToolDirectory);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_ModelExport_Action_Header"),
            LOC.Get("MODEL_ModelExport_Action_Header_TT"));

        GUI.MultiButtonInput("actions",
            "generateOBJ",
            LOC.Get("MODEL_ModelExport_GenerateOBJ_Action"),
            LOC.Get("MODEL_ModelExport_GenerateOBJ_Action_TT"),
            GenerateOBJ,

            "generateHKX",
            LOC.Get("MODEL_ModelExport_GenerateHKX_Action"),
            LOC.Get("MODEL_ModelExport_GenerateHKX_Action_TT"),
            GenerateHKX);
    }
    public void SetColToolDirectory()
    {
        string path;
        var result = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("MODEL_ModelExport_Select_Destination"), out path);

        if (result)
        {
            CollisionToolDir = path;
        }
    }

    public void OpenColToolDirectory()
    {
        Process.Start("explorer.exe", CollisionToolDir);
    }

    public void DetectFlverSelectionMenu()
    {
        if (SelectionMenu.TriggerFlverSelectionMenu)
        {
            SelectionMenu.TriggerFlverSelectionMenu = false;

            ImGui.OpenPopup("flverSelectionMenuPopup");
        }
    }

    public void SelectFlverSource()
    {
        SelectionMenu.TriggerFlverSelectionMenu = true;
    }

    public void DisplayFlverSelectionMenu()
    {
        if (ImGui.BeginPopup("flverSelectionMenuPopup"))
        {
            SelectionMenu.DisplayFlverSelectionTabs();

            ImGui.EndPopup();
        }
    }

    public void GenerateOBJ()
    {

    }

    public void GenerateHKX()
    {

    }
}
