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

        GUI.Spacer();

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

                if (Project.Descriptor.ProjectType is ProjectType.DS3 
                    or ProjectType.SDT 
                    or ProjectType.ER 
                    or ProjectType.NR
                    or ProjectType.DB_NT
                    or ProjectType.DB)
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
                        else if (Project.Descriptor.ProjectType is ProjectType.ER 
                            or ProjectType.NR
                            or ProjectType.DB_NT
                            or ProjectType.DB)
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
                        else if (Project.Descriptor.ProjectType is ProjectType.ER 
                            or ProjectType.NR
                            or ProjectType.DB_NT
                            or ProjectType.DB)
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

    public void RefreshCollisionMateriaList()
    {
        CollisionMaterials = new();

        if (SelectionMenu.SourceFlver != null)
        {
            for (int i = 0; i < SelectionMenu.SourceFlver.Meshes.Count; i++)
            {
                var mesh = SelectionMenu.SourceFlver.Meshes[i];

                CollisionMaterials.Add(new CollisionMaterialDef()
                {
                    FlverMaterial = GetMaterialName(mesh),
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

            FlverToObj.Convert(curFlver, outputPath, materialNamingList, CFG.Current.ModelExport_FlipUVs, transform,
                SelectionMenu.FlverMeshSelection);
        }
        else
        {
            FlverToObj.Convert(curFlver, outputPath, null, CFG.Current.ModelExport_FlipUVs, transform,
                SelectionMenu.FlverMeshSelection);
        }
    }

    /// <summary>
    /// Converts the source FLVER's OBJ (from the Input Directory) to HKX with the collision tool's obj2fsnp.bat, which
    /// only works on files in its own folder: the OBJ is copied there, and the HKX it makes is copied to the Output
    /// Directory.
    /// </summary>
    public void GenerateHKX()
    {
        var flverName = Path.GetFileNameWithoutExtension(SelectionMenu.FlverPath);
        var objName = $"{flverName}.obj";
        var hkxName = $"{flverName}.hkx";

        // Every path is checked before anything is copied, so a bad setting leaves nothing behind
        if (!ModelExportPaths.TryResolveFilePath(CFG.Current.ModelExport_Input_Directory, objName,
                ModelExportPaths.InputDirectoryName, out var targetFile, out var pathError)
            || !ModelExportPaths.TryResolveFilePath(CFG.Current.ModelExport_CollisionTool_Directory, objName,
                ModelExportPaths.ToolDirectoryName, out var tempTargetFile, out pathError)
            || !ModelExportPaths.TryResolveFilePath(CFG.Current.ModelExport_CollisionTool_Directory, "obj2fsnp.bat",
                ModelExportPaths.ToolDirectoryName, out var batFile, out pathError)
            || !ModelExportPaths.TryResolveFilePath(CFG.Current.ModelExport_Output_Directory, hkxName,
                ModelExportPaths.OutputDirectoryName, out var outputFile, out pathError))
        {
            Smithbox.LogError(this, pathError);
            return;
        }

        if (!File.Exists(targetFile))
        {
            Smithbox.LogError(this, LOC.Get("MODEL_ModelExport_Error_No_OBJ_for_HKX_File"));
            return;
        }

        if (!File.Exists(batFile))
        {
            Smithbox.LogError(this, LOC.Get("MODEL_ModelExport_Error_Missing_OBJ2HKX_Bat_File"));
            return;
        }

        var toolDir = Path.GetDirectoryName(batFile);
        var tempOutputFile = Path.Combine(toolDir, hkxName);

        // When a directory is the tool's own folder, its file is already where it needs to be. Copying the OBJ onto
        // itself, or cleaning up an HKX that is the output, would delete the user's file.
        var copyInput = !ModelExportPaths.IsSameFile(targetFile, tempTargetFile);
        var copyOutput = !ModelExportPaths.IsSameFile(tempOutputFile, outputFile);

        // Set while the HKX exists only in the tool's folder, so a failed copy to the output doesn't lose it
        var keepGeneratedFile = false;

        try
        {
            if (copyInput)
            {
                File.Copy(targetFile, tempTargetFile, overwrite: true);
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
            startInfo.ArgumentList.Add(objName);

            // An HKX from an earlier run is told apart from this one's by its time
            var started = DateTime.UtcNow;

            using var process = Process.Start(startInfo);

            if (process == null)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_ModelExport_Error_HKX_Tool_Not_Started"));
                return;
            }

            // Both streams are read at once: reading one to its end first stalls the tool if it fills the other
            var errorTask = process.StandardError.ReadToEndAsync();
            var output = process.StandardOutput.ReadToEnd();
            var errorOutput = errorTask.Result;
            process.WaitForExit();

            if (!File.Exists(tempOutputFile) || File.GetLastWriteTimeUtc(tempOutputFile) < started)
            {
                var toolOutput = string.IsNullOrWhiteSpace(errorOutput) ? output : errorOutput;
                Smithbox.LogError(this, LOC.Get("MODEL_ModelExport_Error_HKX_Not_Generated",
                    process.ExitCode, toolOutput.Trim()));
                return;
            }

            if (copyOutput)
            {
                keepGeneratedFile = true;
                File.Copy(tempOutputFile, outputFile, overwrite: true);
                keepGeneratedFile = false;
            }

            Smithbox.Log(this, LOC.Get("MODEL_ModelExport_Generated_HKX", outputFile));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception)
        {
            Smithbox.LogError(this, keepGeneratedFile
                ? LOC.Get("MODEL_ModelExport_Error_HKX_Copy_Failed", outputFile, tempOutputFile)
                : LOC.Get("MODEL_ModelExport_Error_HKX_Failed", hkxName), e);
        }
        finally
        {
            // Only the copies made here are removed, never the user's OBJ or the output
            if (copyInput)
            {
                TryDeleteTemporary(tempTargetFile);
            }

            if (copyOutput && !keepGeneratedFile)
            {
                TryDeleteTemporary(tempOutputFile);
            }
        }
    }

    /// <summary>
    /// Removes a file the HKX generation put in the tool's folder. Failing to is reported, but isn't an error in the
    /// generation itself.
    /// </summary>
    private void TryDeleteTemporary(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_ModelExport_Error_Temp_Cleanup_Failed", path), e);
        }
    }
}
