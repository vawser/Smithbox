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

    private List<CollisionMaterialDef> CollisionMaterials = new();
    private int TargetCollisionMaterial = -1;

    private string PreviewColMaterial = "";
    private string PreviewColProperty = "";

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

        GUI.Spacer();
        ImGui.BeginTabBar("modelExportTabs");

        if(ImGui.BeginTabItem($"{LOC.Get("MODEL_ModelExport_Tab_OBJ")}##modelExportObjTab"))
        {
            DisplayObjTab();

            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem($"{LOC.Get("MODEL_ModelExport_Tab_HKX")}##modelExportHkxTab"))
        {
            DisplayHkxTab();

            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();

    }

    public void DisplayObjTab()
    {
        // Actions
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

        // Use Single Collision Material
        ImGui.Checkbox($"{LOC.Get("MODEL_ModelExport_UseColMaterials")}##useColMat", ref CFG.Current.ModelExport_UseCollisionMaterial);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            RefreshCollisionMateriaList();
        }

        // Use Single Collision Material
        ImGui.Checkbox($"{LOC.Get("MODEL_ModelExport_UseSingleColMat")}##useSingleColMat", ref CFG.Current.ModelExport_UseSingleCollisionMaterial);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            RefreshCollisionMateriaList();
        }

        // Collision Materials
        if (CFG.Current.ModelExport_UseCollisionMaterial)
        {
            DisplayCollisionMaterialList();
        }
    }

    private void DisplayCollisionMaterialList()
    {
        GUI.Spacer();

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable($"colMatTable", 2, tblFlags))
        {
            ImGui.TableSetupColumn("FlverMat", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("ColMat", ImGuiTableColumnFlags.WidthStretch);

            var index = 0;
            foreach (var entry in CollisionMaterials)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text(entry.FlverMaterial);

                ImGui.TableSetColumnIndex(1);

                var curVal = entry.CollisionMateral;
                ImGui.InputText($"##curMat{index}", ref curVal, 255);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    entry.CollisionMateral = curVal;
                }

                if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.NR or ProjectType.DB)
                {
                    ImGui.SameLine();

                    if (ImGui.ArrowButton($"##colMatBuilderButton{index}", ImGuiDir.Right))
                    {
                        TargetCollisionMaterial = index;
                        ImGui.OpenPopup("colMatBuilder");
                    }
                }

                if (index == TargetCollisionMaterial)
                {
                    if (ImGui.BeginPopup("colMatBuilder"))
                    {
                        // Material
                        if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT)
                        {
                            if (ImGui.BeginCombo($"Material##colMatSelect", PreviewColMaterial))
                            {
                                foreach (var mat in ColMatConfig.DS3_MatNames)
                                {
                                    if (ImGui.Selectable($"{mat.Value}##entry{mat.Key}"))
                                    {
                                        PreviewColMaterial = mat.Value;
                                    }
                                }

                                ImGui.EndCombo();
                            }
                        }
                        else if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.DB)
                        {
                            if (ImGui.BeginCombo($"Material##colMatSelect", PreviewColMaterial))
                            {
                                foreach (var mat in ColMatConfig.ER_MatNames)
                                {
                                    if (ImGui.Selectable($"{mat.Value}##entry{mat.Key}"))
                                    {
                                        PreviewColMaterial = mat.Value;
                                    }
                                }

                                ImGui.EndCombo();
                            }
                        }

                        // Property
                        if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT)
                        {
                            if (ImGui.BeginCombo($"Property##colPropSelect", PreviewColProperty))
                            {
                                foreach (var mat in ColMatConfig.DS3_MatProperties)
                                {
                                    if (ImGui.Selectable($"{mat.Value}##entry{mat.Key}"))
                                    {
                                        PreviewColProperty = mat.Value;
                                    }
                                }

                                ImGui.EndCombo();
                            }
                        }
                        else if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.DB)
                        {
                            if (ImGui.BeginCombo($"Property##colPropSelect", PreviewColProperty))
                            {
                                foreach (var mat in ColMatConfig.ER_MatProperties)
                                {
                                    if (ImGui.Selectable($"{mat.Value}##entry{mat.Key}"))
                                    {
                                        PreviewColProperty = mat.Value;
                                    }
                                }

                                ImGui.EndCombo();
                            }
                        }

                        // Submit
                        if (ImGui.Selectable($"{LOC.Get("MODEL_ModelExport_ColBuilder_Submit")}##submitAction"))
                        {
                            if (PreviewColMaterial != "" && PreviewColProperty != "")
                            {
                                entry.CollisionMateral = $"hkm_{PreviewColMaterial}_{PreviewColProperty}";
                                if(CFG.Current.ModelExport_UseSingleCollisionMaterial)
                                {
                                    foreach(var mat in CollisionMaterials)
                                    {
                                        mat.CollisionMateral = entry.CollisionMateral;
                                    }
                                }
                            }
                            else
                            {
                                entry.CollisionMateral = GetDefaultCollisionMaterial();
                            }
                        }

                        ImGui.EndPopup();
                    }
                }

                index++;
            }

            ImGui.EndTable();
        }
    }

    public void DisplayHkxTab()
    {
        // Actions
        GUI.SimpleHeader(
            LOC.Get("MODEL_ModelExport_Action_Header"),
            LOC.Get("MODEL_ModelExport_Action_Header_TT"));

        GUI.MultiButtonInput("hkxActions",
            "generateHKX",
            LOC.Get("MODEL_ModelExport_GenerateHKX_Action"),
            LOC.Get("MODEL_ModelExport_GenerateHKX_Action_TT"),
            GenerateHKX);

        // Input Directory
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_ModelExport_Input_Directory_Header"),
            LOC.Get("MODEL_ModelExport_Input_Directory_Header_TT"));

        GUI.SinglelineTextInputWithHint("inputDir", ref CFG.Current.ModelExport_Input_Directory,
            LOC.Get("MODEL_Input_Dir_Hint"));

        GUI.MultiButtonInput("inputDirAction",
            "setDirectory",
            LOC.Get("MODEL_ModelExport_SetInputDirectory"),
            LOC.Get("MODEL_ModelExport_SetInputDirectory_TT"),
            SetInputDirectory,

            "openDirectory",
            LOC.Get("MODEL_ModelExport_OpenInputDirectory"),
            LOC.Get("MODEL_ModelExport_OpenInputDirectory_TT"),
            OpenInputDirectory);

        // Collision Tool Directory
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_ModelExport_ColTool_Header"),
            LOC.Get("MODEL_ModelExport_ColTool_Header_TT"));

        GUI.SinglelineTextInputWithHint("colToolDir", ref CFG.Current.ModelExport_CollisionTool_Directory,
            LOC.Get("MODEL_ColTool_Dir_Hint"));

        GUI.MultiButtonInput("colDirActions",
            "setDirectory",
            LOC.Get("MODEL_ModelExport_SetColToolDirectory"),
            LOC.Get("MODEL_ModelExport_SetColToolDirectory_TT"),
            SetColToolDirectory,

            "openDirectory",
            LOC.Get("MODEL_ModelExport_OpenColToolDirectory"),
            LOC.Get("MODEL_ModelExport_OpenColToolDirectory_TT"),
            OpenColToolDirectory);
    }

    public void SetInputDirectory()
    {
        string path;
        var result = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("MODEL_ModelExport_Select_Input_Destination"), out path);

        if (result)
        {
            CFG.Current.ModelExport_Input_Directory = path;
        }
    }

    public void OpenInputDirectory()
    {
        Process.Start("explorer.exe", CFG.Current.ModelExport_Input_Directory);
    }

    public void SetColToolDirectory()
    {
        string path;
        var result = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("MODEL_ModelExport_Select_Destination"), out path);

        if (result)
        {
            CFG.Current.ModelExport_CollisionTool_Directory = path;
        }
    }

    public void OpenColToolDirectory()
    {
        Process.Start("explorer.exe", CFG.Current.ModelExport_CollisionTool_Directory);
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

        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.DB)
        {
            matName = "Stock";
        }

        var matProp = "Safe1";

        return $"hkm_{matName}_{matProp}";
    }

    public void RefreshCollisionMateriaList()
    {
        CollisionMaterials = new();

        if (SelectionMenu.SourceFlver != null)
        {
            for (int i = 0; i < SelectionMenu.SourceFlver.Meshes.Count; i++)
            {
                var mesh = SelectionMenu.SourceFlver.Meshes[i];
                var material = SelectionMenu.SourceFlver.Materials[mesh.MaterialIndex];

                CollisionMaterials.Add(new CollisionMaterialDef()
                {
                    FlverMaterial = material.Name,
                    CollisionMateral = GetDefaultCollisionMaterial()
                });
            }
        }
    }

    public void GenerateOBJ()
    {
        var curFlver = SelectionMenu.SourceFlver;

        if (curFlver == null)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_ModelExport_Error_No_Source_FLVER"));
            return;
        }

        if (CFG.Current.ModelExport_Output_Directory == "")
        {
            Smithbox.LogError(this, LOC.Get("MODEL_ModelExport_Error_Empty_Output_Directory"));
            return;
        }

        var flverName = Path.GetFileNameWithoutExtension(SelectionMenu.FlverPath);
        var outputPath = Path.Combine(CFG.Current.ModelExport_Output_Directory, $"{flverName}.obj");

        if (CFG.Current.ModelExport_UseCollisionMaterial)
        {
            var materialNamingList = new List<MaterialNaming>();
            foreach(var entry in CollisionMaterials)
            {
                var parts = entry.CollisionMateral.Split("_");

                if (parts.Length >= 2)
                {
                    var newNaming = new MaterialNaming(parts[1], parts[2]);
                    materialNamingList.Add(newNaming);
                }
            }

            FlverToObj.Convert(curFlver, outputPath, materialNamingList, CFG.Current.ModelExport_FlipUVs);
        }
        else
        {
            FlverToObj.Convert(curFlver, outputPath, null, CFG.Current.ModelExport_FlipUVs);
        }
    }

    public void GenerateHKX()
    {
        var flverName = Path.GetFileNameWithoutExtension(SelectionMenu.FlverPath);

        var inputDir = CFG.Current.ModelExport_Input_Directory;
        var outputDir = CFG.Current.ModelExport_Output_Directory;
        var toolDir = CFG.Current.ModelExport_CollisionTool_Directory;

        var targetFile = Path.Combine(inputDir, $"{flverName}.obj");
        var tempTargetFile = Path.Combine(toolDir, $"{flverName}.obj");

        if (!File.Exists(targetFile))
        {
            Smithbox.LogError(this, LOC.Get("MODEL_ModelExport_Error_No_OBJ_for_HKX_File"));
            return;
        }

        if(File.Exists(tempTargetFile))
        {
            File.Delete(tempTargetFile);
        }

        // Copy file to the Tool dir so the bat file works correctly
        File.Copy(targetFile, tempTargetFile);

        var batFile = Path.Combine(toolDir, "obj2fsnp.bat");

        if (!File.Exists(batFile))
        {
            Smithbox.LogError(this, LOC.Get("MODEL_ModelExport_Error_Missing_OBJ2HKX_Bat_File"));
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            WorkingDirectory = toolDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("/c");
        startInfo.ArgumentList.Add(batFile);
        startInfo.ArgumentList.Add($"{flverName}.obj");

        using var process = Process.Start(startInfo);

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        var tempOutputFile = Path.Combine(toolDir, $"{flverName}.hkx");
        var outputFile = Path.Combine(outputDir, $"{flverName}.hkx");

        if (File.Exists(outputFile))
        {
            File.Delete(outputFile);
        }

        File.Copy(tempOutputFile, outputFile);

        // Cleanup
        if (File.Exists(tempTargetFile))
        {
            File.Delete(tempTargetFile);
        }

        if (File.Exists(tempOutputFile))
        {
            File.Delete(tempOutputFile);
        }
    }
}
