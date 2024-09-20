using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
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

    public static class BufferLayoutGroups
    {
        public static string ExportBasePath = $"{Smithbox.ProjectRoot}\\.smithbox\\Workflow\\Buffer Layout Groups\\";

        public static List<string> BufferLayoutGroupFiles = new List<string>();

        public static bool RefreshBufferLayoutGroupList = true;

        public static string _selectedBufferLayoutGroup = "";

        public static BufferLayoutList SelectedBufferLayoutList;

        public static void DisplaySubMenu(ModelEditorScreen screen)
        {
            UpdateBufferLayoutGroupList();

            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Replace"))
            {
                foreach (var entry in BufferLayoutGroupFiles)
                {
                    UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedBufferLayoutGroup = entry;
                        SelectedBufferLayoutList = ReadBufferLayoutGroup(entry);

                        var action = new ReplaceBufferLayoutList(screen, SelectedBufferLayoutList.List);
                        screen.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Append"))
            {
                foreach (var entry in BufferLayoutGroupFiles)
                {
                    UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedBufferLayoutGroup = entry;
                        SelectedBufferLayoutList = ReadBufferLayoutGroup(entry);

                        var action = new AppendBufferLayoutList(screen, SelectedBufferLayoutList.List);
                        screen.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }
        }

        public static void UpdateBufferLayoutGroupList()
        {
            if (RefreshBufferLayoutGroupList)
            {
                RefreshBufferLayoutGroupList = false;

                BufferLayoutGroupFiles = new List<string>();

                if (Directory.Exists(ExportBasePath))
                {
                    foreach (var file in Directory.EnumerateFiles(ExportBasePath, "*.json"))
                    {
                        var fileName = Path.GetFileName(file);
                        BufferLayoutGroupFiles.Add(fileName.Replace(".json", ""));
                    }
                }
            }
        }

        public static void DisplayConfiguration(ModelEditorScreen screen)
        {
            var sectionWidth = ImGui.GetWindowWidth();
            var sectionHeight = ImGui.GetWindowHeight();
            var defaultButtonSize = new Vector2(sectionWidth, 32);

            UIHelper.WrappedText("Create a stored Buffer Layout Group from your current selection with the Buffer Layout list.");
            UIHelper.WrappedText("A stored group can then be used to replace the existing Buffer Layout list, or appended to the end.");
            UIHelper.WrappedText("");

            UpdateBufferLayoutGroupList();

            if (ImGui.Button("Create Buffer Layout Group", defaultButtonSize))
            {
                if (screen.ModelHierarchy._selectedBufferLayout != -1 ||
                    screen.ModelHierarchy.BufferLayoutMultiselect.StoredIndices.Count > 0)
                {
                    ImGui.OpenPopup($"##BufferLayoutGroupCreation");
                }
            }
            if (ImGui.BeginPopup("##BufferLayoutGroupCreation"))
            {
                DisplayCreationModal(screen);

                ImGui.EndPopup();
            }

            ImGui.Columns(2);

            ImGui.BeginChild("##BufferLayoutGroupSelection");

            foreach (var entry in BufferLayoutGroupFiles)
            {
                if (ImGui.Selectable($"{entry}##BufferLayoutGroup{entry}", entry == _selectedBufferLayoutGroup))
                {
                    _selectedBufferLayoutGroup = entry;
                    SelectedBufferLayoutList = ReadBufferLayoutGroup(entry);
                }
                if (_selectedBufferLayoutGroup == entry)
                {
                    if (ImGui.BeginPopupContextItem($"##BufferLayoutSelectionPopup{entry}"))
                    {
                        if (ImGui.Selectable("Delete"))
                        {
                            DeleteBufferLayoutGroup(entry);
                        }
                        UIHelper.ShowHoverTooltip("Delete this Buffer Layout group.");

                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("##BufferLayoutGroupActions");

            var width = ImGui.GetWindowWidth();
            var buttonWidth = width;

            if (_selectedBufferLayoutGroup != "" && SelectedBufferLayoutList != null)
            {
                if (ImGui.CollapsingHeader("Buffer Layouts in Group"))
                {
                    for (int i = 0; i < SelectedBufferLayoutList.List.Count; i++)
                    {
                        if (ImGui.Selectable($"Buffer Layout {i}"))
                        {

                        }
                    }
                }

                if (ImGui.Button("Replace", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new ReplaceBufferLayoutList(screen, SelectedBufferLayoutList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.ShowHoverTooltip("Replace the existing Buffer Layouts with the Buffer Layouts within this Buffer Layout group.");
                ImGui.SameLine();
                if (ImGui.Button("Append", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new AppendBufferLayoutList(screen, SelectedBufferLayoutList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.ShowHoverTooltip("Append to Buffer Layouts within this Buffer Layout group to the existing Buffer Layouts.");
            }

            ImGui.EndChild();

            ImGui.Columns(1);
        }

        private static string _createBufferLayoutGroupName = "";

        private static void DisplayCreationModal(ModelEditorScreen screen)
        {
            var width = ImGui.GetWindowWidth();
            var buttonWidth = width / 100 * 95;

            ImGui.InputText("Name##BufferLayoutGroupName", ref _createBufferLayoutGroupName, 255);
            UIHelper.ShowHoverTooltip("The name of the Buffer Layout group.");

            if (ImGui.Button("Create Group", new Vector2(buttonWidth, 32)))
            {
                CreateBufferLayoutGroup(screen, _createBufferLayoutGroupName);
                ImGui.CloseCurrentPopup();
            }
        }

        public static void CreateBufferLayoutGroup(ModelEditorScreen screen, string filename)
        {
            BufferLayoutList newBufferLayoutList = new BufferLayoutList();

            for (int i = 0; i < screen.ResourceHandler.CurrentFLVER.BufferLayouts.Count; i++)
            {
                var curBufferLayout = screen.ResourceHandler.CurrentFLVER.BufferLayouts[i];

                if (screen.ModelHierarchy.BufferLayoutMultiselect.StoredIndices.Count > 0)
                {
                    for (int j = 0; j < screen.ModelHierarchy.BufferLayoutMultiselect.StoredIndices.Count; j++)
                    {
                        if (j == i)
                        {
                            newBufferLayoutList.List.Add(curBufferLayout);
                        }
                    }
                }
                else
                {
                    if (i == screen.ModelHierarchy._selectedBufferLayout)
                    {
                        newBufferLayoutList.List.Add(curBufferLayout);
                    }
                }
            }

            var jsonString = JsonSerializer.Serialize(newBufferLayoutList, FLVERBufferLayoutListContext.Default.BufferLayoutList);

            WriteBufferLayoutGroup($"{filename}.json", jsonString);
        }

        public static BufferLayoutList ReadBufferLayoutGroup(string entry)
        {
            var newBufferLayoutList = new BufferLayoutList();
            var readPath = $"{ExportBasePath}\\{entry}.json";

            try
            {
                var jsonString = File.ReadAllText(readPath);
                newBufferLayoutList = JsonSerializer.Deserialize<BufferLayoutList>(jsonString, FLVERBufferLayoutListContext.Default.BufferLayoutList);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }

            return newBufferLayoutList;
        }

        public static void WriteBufferLayoutGroup(string filename, string jsonString)
        {
            var writePath = Path.Combine(ExportBasePath, $"{filename}");

            if (!Directory.Exists(ExportBasePath))
            {
                Directory.CreateDirectory(ExportBasePath);
            }

            var proceed = true;

            if (File.Exists(writePath))
            {
                var result = PlatformUtils.Instance.MessageBox($"{filename} already exists as a Buffer Layout Group. Are you sure you want to overwrite it?", "Warning", MessageBoxButtons.OKCancel);

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

                    TaskLogs.AddLog($"Saved Buffer Layout Group: {writePath}");
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{ex}");
                }

                RefreshBufferLayoutGroupList = true;
            }
        }

        public static void DeleteBufferLayoutGroup(string name)
        {
            var filepath = Path.Combine(ExportBasePath, $"{name}.json");

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            RefreshBufferLayoutGroupList = true;
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
    [JsonSerializable(typeof(BufferLayoutList))]
    [JsonSerializable(typeof(FLVER2.BufferLayout))]
    internal partial class FLVERBufferLayoutListContext : JsonSerializerContext
    {
    }
    public class BufferLayoutList
    {
        public List<FLVER2.BufferLayout> List { get; set; }

        public BufferLayoutList()
        {
            List = new List<FLVER2.BufferLayout>();
        }
    }
}
