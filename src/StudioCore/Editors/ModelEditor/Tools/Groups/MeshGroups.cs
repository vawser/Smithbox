using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Actions.Mesh;
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

    public static class MeshGroups
    {
        public static string ExportBasePath = "";

        public static List<string> MeshGroupFiles = new List<string>();

        public static bool RefreshMeshGroupList = true;

        public static string _selectedMeshGroup = "";

        public static MeshList SelectedMeshList;

        public static void DisplaySubMenu(ModelEditorScreen editor)
        {
            ExportBasePath = $"{editor.Project.ProjectPath}\\.smithbox\\Workflow\\Mesh Groups\\";

            UpdateMeshGroupList();

            if (ImGui.BeginMenu("Replace"))
            {
                foreach (var entry in MeshGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedMeshGroup = entry;
                        SelectedMeshList = ReadMeshGroup(entry);

                        var action = new ReplaceMeshList(editor, SelectedMeshList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Append"))
            {
                foreach (var entry in MeshGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedMeshGroup = entry;
                        SelectedMeshList = ReadMeshGroup(entry);

                        var action = new AppendMeshList(editor, SelectedMeshList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }
        }

        public static void UpdateMeshGroupList()
        {
            if (RefreshMeshGroupList)
            {
                RefreshMeshGroupList = false;

                MeshGroupFiles = new List<string>();

                if (Directory.Exists(ExportBasePath))
                {
                    foreach (var file in Directory.EnumerateFiles(ExportBasePath, "*.json"))
                    {
                        var fileName = Path.GetFileName(file);
                        MeshGroupFiles.Add(fileName.Replace(".json", ""));
                    }
                }
            }
        }

        public static void DisplayConfiguration(ModelEditorScreen screen)
        {
            var sectionWidth = ImGui.GetWindowWidth();
            var sectionHeight = ImGui.GetWindowHeight();
            var defaultButtonSize = new Vector2(sectionWidth, 32);

            UIHelper.WrappedText("Create a stored Mesh Group from your current selection with the Meshes list.");
            UIHelper.WrappedText("A stored group can then be used to replace the existing Meshes list, or appended to the end.");
            UIHelper.WrappedText("");

            UpdateMeshGroupList();

            if (ImGui.Button("Create Mesh Group", defaultButtonSize))
            {
                if (screen.Selection._selectedMesh != -1 ||
                    screen.Selection.MeshMultiselect.StoredIndices.Count > 0)
                {
                    ImGui.OpenPopup($"##MeshGroupCreation");
                }
            }
            if (ImGui.BeginPopup("##MeshGroupCreation"))
            {
                DisplayCreationModal(screen);

                ImGui.EndPopup();
            }

            ImGui.Columns(2);

            ImGui.BeginChild("##MeshGroupSelection");

            foreach (var entry in MeshGroupFiles)
            {
                if (ImGui.Selectable($"{entry}##MeshGroup{entry}", entry == _selectedMeshGroup))
                {
                    _selectedMeshGroup = entry;
                    SelectedMeshList = ReadMeshGroup(entry);
                }
                if (_selectedMeshGroup == entry)
                {
                    if (ImGui.BeginPopupContextItem($"##MeshSelectionPopup{entry}"))
                    {
                        if (ImGui.Selectable("Delete"))
                        {
                            DeleteMeshGroup(entry);
                        }
                        UIHelper.Tooltip("Delete this Mesh group.");

                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("##MeshGroupActions");

            var width = ImGui.GetWindowWidth();
            var buttonWidth = width;

            if (_selectedMeshGroup != "" && SelectedMeshList != null)
            {
                if (ImGui.CollapsingHeader("Meshes in Group"))
                {
                    for (int i = 0; i < SelectedMeshList.List.Count; i++)
                    {
                        if (ImGui.Selectable($"Mesh {i}"))
                        {

                        }
                    }
                }

                if (ImGui.Button("Replace", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new ReplaceMeshList(screen, SelectedMeshList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Replace the existing Meshes with the Meshes within this Mesh group.");
                ImGui.SameLine();
                if (ImGui.Button("Append", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new AppendMeshList(screen, SelectedMeshList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Append to Meshs within this Mesh group to the existing Meshes.");
            }

            ImGui.EndChild();

            ImGui.Columns(1);
        }

        private static string _createMeshGroupName = "";

        private static void DisplayCreationModal(ModelEditorScreen screen)
        {
            var width = ImGui.GetWindowWidth();
            var buttonWidth = width / 100 * 95;

            ImGui.InputText("Name##MeshGroupName", ref _createMeshGroupName, 255);
            UIHelper.Tooltip("The name of the Mesh group.");

            if (ImGui.Button("Create Group", new Vector2(buttonWidth, 32)))
            {
                CreateMeshGroup(screen, _createMeshGroupName);
                ImGui.CloseCurrentPopup();
            }
        }

        public static void CreateMeshGroup(ModelEditorScreen screen, string filename)
        {
            if (!screen.ResManager.HasCurrentFLVER())
                return;

            MeshList newMeshList = new MeshList();

            for (int i = 0; i < screen.ResManager.GetCurrentFLVER().Meshes.Count; i++)
            {
                var curMesh = screen.ResManager.GetCurrentFLVER().Meshes[i];

                if (screen.Selection.MeshMultiselect.StoredIndices.Count > 0)
                {
                    for (int j = 0; j < screen.Selection.MeshMultiselect.StoredIndices.Count; j++)
                    {
                        if (j == i)
                        {
                            newMeshList.List.Add(curMesh);
                        }
                    }
                }
                else
                {
                    if (i == screen.Selection._selectedMesh)
                    {
                        newMeshList.List.Add(curMesh);
                    }
                }
            }

            var jsonString = JsonSerializer.Serialize(newMeshList, FLVERMeshListContext.Default.MeshList);

            WriteMeshGroup($"{filename}.json", jsonString);
        }

        public static MeshList ReadMeshGroup(string entry)
        {
            var newMeshList = new MeshList();
            var readPath = $"{ExportBasePath}\\{entry}.json";

            try
            {
                var jsonString = File.ReadAllText(readPath);
                newMeshList = JsonSerializer.Deserialize<MeshList>(jsonString, FLVERMeshListContext.Default.MeshList);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }

            return newMeshList;
        }

        public static void WriteMeshGroup(string filename, string jsonString)
        {
            var writePath = Path.Combine(ExportBasePath, $"{filename}");

            if (!Directory.Exists(ExportBasePath))
            {
                Directory.CreateDirectory(ExportBasePath);
            }

            var proceed = true;

            if (File.Exists(writePath))
            {
                var result = PlatformUtils.Instance.MessageBox($"{filename} already exists as a Mesh Group. Are you sure you want to overwrite it?", "Warning", MessageBoxButtons.OKCancel);

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

                    TaskLogs.AddLog($"Saved Mesh Group: {writePath}");
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{ex}");
                }

                RefreshMeshGroupList = true;
            }
        }

        public static void DeleteMeshGroup(string name)
        {
            var filepath = Path.Combine(ExportBasePath, $"{name}.json");

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            RefreshMeshGroupList = true;
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
    [JsonSerializable(typeof(MeshList))]
    [JsonSerializable(typeof(FLVER2.Mesh))]
    [JsonSerializable(typeof(FLVER.Vertex))]
    internal partial class FLVERMeshListContext : JsonSerializerContext
    {
    }
    public class MeshList
    {
        public List<FLVER2.Mesh> List { get; set; }

        public MeshList()
        {
            List = new List<FLVER2.Mesh>();
        }
    }
}
