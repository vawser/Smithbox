using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Actions.Material;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Tools
{

    public static class MaterialGroups
    {
        public static string ExportBasePath = "";

        public static List<string> MaterialGroupFiles = new List<string>();

        public static bool RefreshMaterialGroupList = true;

        public static string _selectedMaterialGroup = "";

        public static MaterialList SelectedMaterialList;

        public static void DisplaySubMenu(ModelEditorScreen editor)
        {
            ExportBasePath = $"{editor.Project.ProjectPath}\\.smithbox\\Workflow\\Material Groups\\";

            UpdateMaterialGroupList();

            if (ImGui.BeginMenu("Replace"))
            {
                foreach (var entry in MaterialGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedMaterialGroup = entry;
                        SelectedMaterialList = ReadMaterialGroup(entry);

                        var action = new ReplaceMaterialList(editor, SelectedMaterialList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Append"))
            {
                foreach (var entry in MaterialGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedMaterialGroup = entry;
                        SelectedMaterialList = ReadMaterialGroup(entry);

                        var action = new AppendMaterialList(editor, SelectedMaterialList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }
        }

        public static void UpdateMaterialGroupList()
        {
            if (RefreshMaterialGroupList)
            {
                RefreshMaterialGroupList = false;

                MaterialGroupFiles = new List<string>();

                if (Directory.Exists(ExportBasePath))
                {
                    foreach (var file in Directory.EnumerateFiles(ExportBasePath, "*.json"))
                    {
                        var fileName = Path.GetFileName(file);
                        MaterialGroupFiles.Add(fileName.Replace(".json", ""));
                    }
                }
            }
        }

        public static void DisplayConfiguration(ModelEditorScreen screen)
        {
            var sectionWidth = ImGui.GetWindowWidth();
            var sectionHeight = ImGui.GetWindowHeight();
            var defaultButtonSize = new Vector2(sectionWidth, 32);

            UIHelper.WrappedText("Create a stored Material Group from your current selection with the Material list.");
            UIHelper.WrappedText("A stored group can then be used to replace the existing Material list, or appended to the end.");
            UIHelper.WrappedText("");

            UpdateMaterialGroupList();

            if (ImGui.Button("Create Material Group", defaultButtonSize))
            {
                if (screen.Selection._selectedMaterial != -1 ||
                    screen.Selection.MaterialMultiselect.StoredIndices.Count > 0)
                {
                    ImGui.OpenPopup($"##MaterialGroupCreation");
                }
            }
            if (ImGui.BeginPopup("##MaterialGroupCreation"))
            {
                DisplayCreationModal(screen);

                ImGui.EndPopup();
            }

            ImGui.Columns(2);

            ImGui.BeginChild("##MaterialGroupSelection");

            foreach (var entry in MaterialGroupFiles)
            {
                if (ImGui.Selectable($"{entry}##MaterialGroup{entry}", entry == _selectedMaterialGroup))
                {
                    _selectedMaterialGroup = entry;
                    SelectedMaterialList = ReadMaterialGroup(entry);
                }
                if (_selectedMaterialGroup == entry)
                {
                    if (ImGui.BeginPopupContextItem($"##MaterialSelectionPopup{entry}"))
                    {
                        if (ImGui.Selectable("Delete"))
                        {
                            DeleteMaterialGroup(entry);
                        }
                        UIHelper.Tooltip("Delete this Material group.");

                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("##MaterialGroupActions");

            var width = ImGui.GetWindowWidth();
            var buttonWidth = width;

            if (_selectedMaterialGroup != "" && SelectedMaterialList != null)
            {
                if (ImGui.CollapsingHeader("Materials in Group"))
                {
                    for (int i = 0; i < SelectedMaterialList.List.Count; i++)
                    {
                        if (ImGui.Selectable($"Material {i} - {SelectedMaterialList.List[i].Name}"))
                        {

                        }
                    }
                }

                if (ImGui.Button("Replace", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new ReplaceMaterialList(screen, SelectedMaterialList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Replace the existing Materials with the Materials within this Material group.");
                ImGui.SameLine();
                if (ImGui.Button("Append", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new AppendMaterialList(screen, SelectedMaterialList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Append to Materials within this Material group to the existing Materials.");
            }

            ImGui.EndChild();

            ImGui.Columns(1);
        }

        private static string _createMaterialGroupName = "";

        private static void DisplayCreationModal(ModelEditorScreen screen)
        {
            var width = ImGui.GetWindowWidth();
            var buttonWidth = width / 100 * 95;

            ImGui.InputText("Name##MaterialGroupName", ref _createMaterialGroupName, 255);
            UIHelper.Tooltip("The name of the Material group.");

            if (ImGui.Button("Create Group", new Vector2(buttonWidth, 32)))
            {
                CreateMaterialGroup(screen, _createMaterialGroupName);
                ImGui.CloseCurrentPopup();
            }
        }

        public static void CreateMaterialGroup(ModelEditorScreen screen, string filename)
        {
            if (!screen.ResManager.HasCurrentFLVER())
                return;

            MaterialList newMaterialList = new MaterialList();

            for (int i = 0; i < screen.ResManager.GetCurrentFLVER().Materials.Count; i++)
            {
                var curMaterial = screen.ResManager.GetCurrentFLVER().Materials[i];

                if (screen.Selection.MaterialMultiselect.StoredIndices.Count > 0)
                {
                    for (int j = 0; j < screen.Selection.MaterialMultiselect.StoredIndices.Count; j++)
                    {
                        if (j == i)
                        {
                            newMaterialList.List.Add(curMaterial);
                        }
                    }
                }
                else
                {
                    if (i == screen.Selection._selectedMaterial)
                    {
                        newMaterialList.List.Add(curMaterial);
                    }
                }
            }

            var jsonString = JsonSerializer.Serialize(newMaterialList, FLVERMaterialListContext.Default.MaterialList);

            WriteMaterialGroup($"{filename}.json", jsonString);
        }

        public static MaterialList ReadMaterialGroup(string entry)
        {
            var newMaterialList = new MaterialList();
            var readPath = $"{ExportBasePath}\\{entry}.json";

            try
            {
                var jsonString = File.ReadAllText(readPath);
                newMaterialList = JsonSerializer.Deserialize<MaterialList>(jsonString, FLVERMaterialListContext.Default.MaterialList);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }

            return newMaterialList;
        }

        public static void WriteMaterialGroup(string filename, string jsonString)
        {
            var writePath = Path.Combine(ExportBasePath, $"{filename}");

            if (!Directory.Exists(ExportBasePath))
            {
                Directory.CreateDirectory(ExportBasePath);
            }

            var proceed = true;

            if (File.Exists(writePath))
            {
                var result = PlatformUtils.Instance.MessageBox($"{filename} already exists as a Material Group. Are you sure you want to overwrite it?", "Warning", MessageBoxButtons.OKCancel);

                if (result is DialogResult.Cancel)
                {
                    proceed = false;
                }
            }

            if (proceed)
            {
                try
                {
                    var fs = new FileStream(writePath, System.IO.FileMode.Create);
                    var data = Encoding.ASCII.GetBytes(jsonString);
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                    fs.Dispose();

                    TaskLogs.AddLog($"Saved Material Group: {writePath}");
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{ex}");
                }

                RefreshMaterialGroupList = true;
            }
        }

        public static void DeleteMaterialGroup(string name)
        {
            var filepath = Path.Combine(ExportBasePath, $"{name}.json");

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            RefreshMaterialGroupList = true;
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
    [JsonSerializable(typeof(MaterialList))]
    [JsonSerializable(typeof(FLVER2.Material))]
    internal partial class FLVERMaterialListContext : JsonSerializerContext
    {
    }
    public class MaterialList
    {
        public List<FLVER2.Material> List { get; set; }

        public MaterialList()
        {
            List = new List<FLVER2.Material>();
        }
    }
}
