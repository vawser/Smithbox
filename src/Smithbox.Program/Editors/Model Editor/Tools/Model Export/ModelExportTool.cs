using Hexa.NET.ImGui;
using StudioCore.Editors.ParamEditor;
using StudioCore.Utilities;
using System.Diagnostics;
using System.Numerics;
using static StudioCore.Editors.ModelEditor.FlverToObj;

namespace StudioCore.Editors.ModelEditor;


public class ModelExportTool
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public ModelSelectionMenu SelectionMenu;

    public ModelExportTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        SelectionMenu = new(view, project);
    }

    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ToolSections.Header($"{LOC.Get("MODEL_ModelExport_Header")}##modelExportHeader"))
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
            "selectCurrentFlver",
            LOC.Get("MODEL_ModelExport_Select_Current_Flver_Action"),
            LOC.Get("MODEL_ModelExport_Select_Current_Flver_Action_TT"),
            SelectCurrentFlver,

            "selectFlverFromList",
            LOC.Get("MODEL_ModelExport_Select_Flver_Action"),
            LOC.Get("MODEL_ModelExport_Select_Flver_Action_TT"),
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

        // The current FLVER is the loaded model's own, which gains or loses meshes when it is saved
        SelectionMenu.SyncMeshSelection();

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
                var label = LOC.Get("MODEL_ModelExport_Flver_Mesh_Entry", i, GetMaterialName(mesh), mesh.Vertices.Count);

                var selected = SelectionMenu.FlverMeshSelection[i];
                if (ImGui.Checkbox($"{label}##flverMesh{i}", ref selected))
                {
                    SelectionMenu.FlverMeshSelection[i] = selected;
                }
            }

            ImGui.EndChild();
        }

        // Output Directory
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_ModelExport_SetOutputDirectory"),
            LOC.Get("MODEL_ModelExport_SetOutputDirectory_TT"));

        GUI.SinglelineTextInputWithHint("outputDirInput", ref CFG.Current.ModelExport_Output_Directory,
            LOC.Get("MODEL_Output_Dir_Hint"));

        GUI.MultiButtonInput("outputDirActions",
            "setDirectory",
            LOC.Get("MODEL_ModelExport_SetOutputDirectory"),
            LOC.Get("MODEL_ModelExport_SetOutputDirectory_TT"),
            SetOutputDirectory,

            "openDirectory",
            LOC.Get("MODEL_ModelExport_OpenOutputDirectory"),
            LOC.Get("MODEL_ModelExport_OpenOutputDirectory_TT"),
            OpenOutputDirectory);

        DisplayObjOptions();
    }

    public void DisplayObjOptions()
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_ModelExport_Action_Header"),
            LOC.Get("MODEL_ModelExport_Action_Header_TT"));

        GUI.MultiButtonInput("objActions",
            "generateOBJ",
            LOC.Get("MODEL_ModelExport_GenerateOBJ_Action"),
            LOC.Get("MODEL_ModelExport_GenerateOBJ_Action_TT"),
            GenerateOBJ);

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_ModelExport_Options_Header"),
            LOC.Get("MODEL_ModelExport_Options_Header_TT"));

        // Flip UVs in OBJ
        ImGui.Checkbox($"{LOC.Get("MODEL_ModelExport_FlipOBJUVs")}##flipUvs", ref CFG.Current.ModelExport_FlipUVs);

        // Mirroring
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_ModelExport_Mirror_Header"),
            LOC.Get("MODEL_ModelExport_Mirror_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MODEL_ModelExport_Mirror_X")}##mirrorX", ref CFG.Current.ModelExport_MirrorX);
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("MODEL_ModelExport_Mirror_Y")}##mirrorY", ref CFG.Current.ModelExport_MirrorY);
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("MODEL_ModelExport_Mirror_Z")}##mirrorZ", ref CFG.Current.ModelExport_MirrorZ);

        // Forward Axis
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_ModelExport_ForwardAxis_Header"),
            LOC.Get("MODEL_ModelExport_ForwardAxis_Header_TT"));

        DisplayAxisCombo("##forwardAxis", ref CFG.Current.ModelExport_ForwardAxis);

        // Up Axis
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_ModelExport_UpAxis_Header"),
            LOC.Get("MODEL_ModelExport_UpAxis_Header_TT"));

        DisplayAxisCombo("##upAxis", ref CFG.Current.ModelExport_UpAxis);

    }

    private void DisplayAxisCombo(string id, ref ModelExportAxis axis)
    {
        DPI.ApplyInputWidth();
        if (ImGui.BeginCombo(id, LOC.Get(axis.GetDisplayName())))
        {
            foreach (var entry in Enum.GetValues<ModelExportAxis>())
            {
                if (ImGui.Selectable(LOC.Get(entry.GetDisplayName())))
                {
                    axis = entry;
                }
            }

            ImGui.EndCombo();
        }
    }

    public void SetOutputDirectory()
    {
        string path;
        var result = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("MODEL_ModelExport_Select_Output_Destination"), out path);

        if (result)
        {
            CFG.Current.ModelExport_Output_Directory = path;
        }
    }

    public void OpenOutputDirectory()
    {
        Process.Start("explorer.exe", CFG.Current.ModelExport_Output_Directory);
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

    public void SelectCurrentFlver()
    {
        SelectionMenu.SelectAndLoadCurrentFlver(View.Selection.SelectedModelWrapper);
    }

    public void DisplayFlverSelectionMenu()
    {
        if (ImGui.BeginPopup("flverSelectionMenuPopup"))
        {
            SelectionMenu.DisplayFlverSelectionTabs();

            ImGui.EndPopup();
        }
    }

    public string GetDefaultCollisionMaterial()
    {
        var matName = "Cobblestone";

        if (Project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            matName = "Stock";
        }

        var matProp = "Safe1";

        return $"hkm_{matName}_{matProp}";
    }

    // A mesh may point at a material that doesn't exist, which the export writes with the default material
    private string GetMaterialName(SoulsFormats.FLVER2.Mesh mesh)
    {
        var materials = SelectionMenu.SourceFlver.Materials;

        return mesh.MaterialIndex >= 0 && mesh.MaterialIndex < materials.Count
            ? materials[mesh.MaterialIndex].Name
            : "material_default";
    }

    public void GenerateOBJ()
    {
        var curFlver = SelectionMenu.SourceFlver;

        if (curFlver == null)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_ModelExport_Error_No_Source_FLVER"));
            return;
        }

        var flverName = Path.GetFileNameWithoutExtension(SelectionMenu.FlverPath);

        if (!ModelExportPaths.TryResolveFilePath(CFG.Current.ModelExport_Output_Directory, $"{flverName}.obj",
                ModelExportPaths.OutputDirectoryName, out var outputPath, out var pathError))
        {
            Smithbox.LogError(this, pathError);
            return;
        }

        if (!FlverToObj.AreAxesValid(CFG.Current.ModelExport_ForwardAxis, CFG.Current.ModelExport_UpAxis))
        {
            Smithbox.LogError(this, LOC.Get("MODEL_ModelExport_Error_Invalid_Axes"));
            return;
        }

        var transform = FlverToObj.BuildTransform(
            CFG.Current.ModelExport_ForwardAxis,
            CFG.Current.ModelExport_UpAxis,
            CFG.Current.ModelExport_MirrorX,
            CFG.Current.ModelExport_MirrorY,
            CFG.Current.ModelExport_MirrorZ);

        FlverToObj.Convert(curFlver, outputPath, null, CFG.Current.ModelExport_FlipUVs, transform,
            SelectionMenu.FlverMeshSelection);
    }
}
