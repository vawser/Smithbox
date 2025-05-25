using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Actions.GxList;
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

    public static class GXListGroups
    {
        public static string ExportBasePath = "";

        public static List<string> GXListGroupFiles = new List<string>();

        public static bool RefreshGXListGroupList = true;

        public static string _selectedGXListGroup = "";

        public static GXListList SelectedGXListList;

        public static void DisplaySubMenu(ModelEditorScreen editor)
        {
            ExportBasePath = $"{editor.Project.ProjectPath}\\.smithbox\\Workflow\\GX List Groups\\";

            UpdateGXListGroupList();

            if (ImGui.BeginMenu("Replace"))
            {
                foreach (var entry in GXListGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedGXListGroup = entry;
                        SelectedGXListList = ReadGXListGroup(entry);

                        var action = new ReplaceGxList(editor, SelectedGXListList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Append"))
            {
                foreach (var entry in GXListGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedGXListGroup = entry;
                        SelectedGXListList = ReadGXListGroup(entry);

                        var action = new AppendGxList(editor, SelectedGXListList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }
        }

        public static void UpdateGXListGroupList()
        {
            if (RefreshGXListGroupList)
            {
                RefreshGXListGroupList = false;

                GXListGroupFiles = new List<string>();

                if (Directory.Exists(ExportBasePath))
                {
                    foreach (var file in Directory.EnumerateFiles(ExportBasePath, "*.json"))
                    {
                        var fileName = Path.GetFileName(file);
                        GXListGroupFiles.Add(fileName.Replace(".json", ""));
                    }
                }
            }
        }

        public static void DisplayConfiguration(ModelEditorScreen screen)
        {
            var sectionWidth = ImGui.GetWindowWidth();
            var sectionHeight = ImGui.GetWindowHeight();
            var defaultButtonSize = new Vector2(sectionWidth, 32);

            UIHelper.WrappedText("Create a stored GX List Group from your current selection with the GX List list.");
            UIHelper.WrappedText("A stored group can then be used to replace the existing GX List list, or appended to the end.");
            UIHelper.WrappedText("");

            UpdateGXListGroupList();

            if (ImGui.Button("Create GX List Group", defaultButtonSize))
            {
                if (screen.Selection._selectedGXList != -1 ||
                    screen.Selection.GxListMultiselect.StoredIndices.Count > 0)
                {
                    ImGui.OpenPopup($"##GXListGroupCreation");
                }
            }
            if (ImGui.BeginPopup("##GXListGroupCreation"))
            {
                DisplayCreationModal(screen);

                ImGui.EndPopup();
            }

            ImGui.Columns(2);

            ImGui.BeginChild("##GXListGroupSelection");

            foreach (var entry in GXListGroupFiles)
            {
                if (ImGui.Selectable($"{entry}##GXListGroup{entry}", entry == _selectedGXListGroup))
                {
                    _selectedGXListGroup = entry;
                    SelectedGXListList = ReadGXListGroup(entry);
                }
                if (_selectedGXListGroup == entry)
                {
                    if (ImGui.BeginPopupContextItem($"##GXListSelectionPopup{entry}"))
                    {
                        if (ImGui.Selectable("Delete"))
                        {
                            DeleteGXListGroup(entry);
                        }
                        UIHelper.Tooltip("Delete this GX List group.");

                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("##GXListGroupActions");

            var width = ImGui.GetWindowWidth();
            var buttonWidth = width;

            if (_selectedGXListGroup != "" && SelectedGXListList != null)
            {
                if (ImGui.CollapsingHeader("GX Lists in Group"))
                {
                    for (int i = 0; i < SelectedGXListList.List.Count; i++)
                    {
                        if (ImGui.Selectable($"GX List {i}"))
                        {

                        }
                    }
                }

                if (ImGui.Button("Replace", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new ReplaceGxList(screen, SelectedGXListList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Replace the existing GX Lists with the GX Lists within this GX List group.");
                ImGui.SameLine();
                if (ImGui.Button("Append", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new AppendGxList(screen, SelectedGXListList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Append to GX Lists within this GX List group to the existing GX Lists.");
            }

            ImGui.EndChild();

            ImGui.Columns(1);
        }

        private static string _createGXListGroupName = "";

        private static void DisplayCreationModal(ModelEditorScreen screen)
        {
            var width = ImGui.GetWindowWidth();
            var buttonWidth = width / 100 * 95;

            ImGui.InputText("Name##GXListGroupName", ref _createGXListGroupName, 255);
            UIHelper.Tooltip("The name of the GX List group.");

            if (ImGui.Button("Create Group", new Vector2(buttonWidth, 32)))
            {
                CreateGXListGroup(screen, _createGXListGroupName);
                ImGui.CloseCurrentPopup();
            }
        }

        public static void CreateGXListGroup(ModelEditorScreen screen, string filename)
        {
            if (!screen.ResManager.HasCurrentFLVER())
                return;

            GXListList newGXListList = new GXListList();

            for (int i = 0; i < screen.ResManager.GetCurrentFLVER().GXLists.Count; i++)
            {
                var curGXList = screen.ResManager.GetCurrentFLVER().GXLists[i];

                if (screen.Selection.GxListMultiselect.StoredIndices.Count > 0)
                {
                    for (int j = 0; j < screen.Selection.GxListMultiselect.StoredIndices.Count; j++)
                    {
                        if (j == i)
                        {
                            newGXListList.List.Add(curGXList);
                        }
                    }
                }
                else
                {
                    if (i == screen.Selection._selectedGXList)
                    {
                        newGXListList.List.Add(curGXList);
                    }
                }
            }

            var jsonString = JsonSerializer.Serialize(newGXListList, FLVERGXListListContext.Default.GXListList);

            WriteGXListGroup($"{filename}.json", jsonString);
        }

        public static GXListList ReadGXListGroup(string entry)
        {
            var newGXListList = new GXListList();
            var readPath = $"{ExportBasePath}\\{entry}.json";

            try
            {
                var jsonString = File.ReadAllText(readPath);
                newGXListList = JsonSerializer.Deserialize<GXListList>(jsonString, FLVERGXListListContext.Default.GXListList);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }

            return newGXListList;
        }

        public static void WriteGXListGroup(string filename, string jsonString)
        {
            var writePath = Path.Combine(ExportBasePath, $"{filename}");

            if (!Directory.Exists(ExportBasePath))
            {
                Directory.CreateDirectory(ExportBasePath);
            }

            var proceed = true;

            if (File.Exists(writePath))
            {
                var result = PlatformUtils.Instance.MessageBox($"{filename} already exists as a GX List Group. Are you sure you want to overwrite it?", "Warning", MessageBoxButtons.OKCancel);

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

                    TaskLogs.AddLog($"Saved GX List Group: {writePath}");
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{ex}");
                }

                RefreshGXListGroupList = true;
            }
        }

        public static void DeleteGXListGroup(string name)
        {
            var filepath = Path.Combine(ExportBasePath, $"{name}.json");

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            RefreshGXListGroupList = true;
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
    [JsonSerializable(typeof(GXListList))]
    [JsonSerializable(typeof(FLVER2.GXList))]
    internal partial class FLVERGXListListContext : JsonSerializerContext
    {
    }
    public class GXListList
    {
        public List<FLVER2.GXList> List { get; set; }

        public GXListList()
        {
            List = new List<FLVER2.GXList>();
        }
    }
}
