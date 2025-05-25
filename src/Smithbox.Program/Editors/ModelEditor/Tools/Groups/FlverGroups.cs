using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
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

    public static class FlverGroups
    {
        public static string ExportBasePath = "";

        public static List<string> FLVERGroupFiles = new List<string>();

        public static bool RefreshFLVERGroupList = true;

        public static string _selectedFLVERGroup = "";

        public static FLVERList SelectedFLVERList;

        public static void DisplaySubMenu(ModelEditorScreen editor)
        {
            ExportBasePath = $"{editor.Project.ProjectPath}\\.smithbox\\Workflow\\FLVER Groups\\";

            UpdateFLVERGroupList();

            if (ImGui.BeginMenu("Replace"))
            {
                foreach (var entry in FLVERGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedFLVERGroup = entry;
                        SelectedFLVERList = ReadFLVERGroup(entry);

                        var action = new ReplaceFLVERList(editor, SelectedFLVERList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }
        }

        public static void UpdateFLVERGroupList()
        {
            if (RefreshFLVERGroupList)
            {
                RefreshFLVERGroupList = false;

                FLVERGroupFiles = new List<string>();

                if (Directory.Exists(ExportBasePath))
                {
                    foreach (var file in Directory.EnumerateFiles(ExportBasePath, "*.json"))
                    {
                        var fileName = Path.GetFileName(file);
                        FLVERGroupFiles.Add(fileName.Replace(".json", ""));
                    }
                }
            }
        }

        public static void DisplayConfiguration(ModelEditorScreen screen)
        {
            var sectionWidth = ImGui.GetWindowWidth();
            var sectionHeight = ImGui.GetWindowHeight();
            var defaultButtonSize = new Vector2(sectionWidth, 32);

            UIHelper.WrappedText("Create a stored FLVER from your current model FLVER.");
            UIHelper.WrappedText("A stored group can then be used to replace an existing model's FLVER entirely.");
            UIHelper.WrappedText("");

            UpdateFLVERGroupList();

            if (ImGui.Button("Create Stored FLVER", defaultButtonSize))
            {
                if (screen.ResManager.GetCurrentFLVER() != null)
                {
                    ImGui.OpenPopup($"##FLVERGroupCreation");
                }
            }
            if (ImGui.BeginPopup("##FLVERGroupCreation"))
            {
                DisplayCreationModal(screen);

                ImGui.EndPopup();
            }

            ImGui.Columns(2);

            ImGui.BeginChild("##FLVERGroupSelection");

            foreach (var entry in FLVERGroupFiles)
            {
                if (ImGui.Selectable($"{entry}##FLVERGroup{entry}", entry == _selectedFLVERGroup))
                {
                    _selectedFLVERGroup = entry;
                    SelectedFLVERList = ReadFLVERGroup(entry);
                }
                if (_selectedFLVERGroup == entry)
                {
                    if (ImGui.BeginPopupContextItem($"##FLVERSelectionPopup{entry}"))
                    {
                        if (ImGui.Selectable("Delete"))
                        {
                            DeleteFLVERGroup(entry);
                        }
                        UIHelper.Tooltip("Delete this stored FLVER.");

                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("##FLVERGroupActions");

            var width = ImGui.GetWindowWidth();
            var buttonWidth = width;

            if (_selectedFLVERGroup != "" && SelectedFLVERList != null)
            {
                if (ImGui.Button("Import", new Vector2(buttonWidth, 32)))
                {
                    var action = new ReplaceFLVERList(screen, SelectedFLVERList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Replace the currently loaded FLVER with this stored FLVER.");
            }

            ImGui.EndChild();

            ImGui.Columns(1);
        }

        private static string _createFLVERGroupName = "";

        private static void DisplayCreationModal(ModelEditorScreen screen)
        {
            var width = ImGui.GetWindowWidth();
            var buttonWidth = width / 100 * 95;

            ImGui.InputText("Name##FLVERGroupName", ref _createFLVERGroupName, 255);
            UIHelper.Tooltip("The name of the stored FLVER.");

            if (ImGui.Button("Create Stored FLVEr", new Vector2(buttonWidth, 32)))
            {
                CreateFLVERGroup(screen, _createFLVERGroupName);
                ImGui.CloseCurrentPopup();
            }
        }

        public static void CreateFLVERGroup(ModelEditorScreen screen, string filename)
        {
            if (!screen.ResManager.HasCurrentFLVER())
                return;

            FLVERList newFLVERList = new FLVERList();

            newFLVERList.List.Add(screen.ResManager.GetCurrentFLVER());

            var jsonString = JsonSerializer.Serialize(newFLVERList, StoredFLVERListContext.Default.FLVERList);

            WriteFLVERGroup($"{filename}.json", jsonString);
        }

        public static FLVERList ReadFLVERGroup(string entry)
        {
            var newFLVERList = new FLVERList();
            var readPath = $"{ExportBasePath}\\{entry}.json";

            try
            {
                var jsonString = File.ReadAllText(readPath);
                newFLVERList = JsonSerializer.Deserialize<FLVERList>(jsonString, StoredFLVERListContext.Default.FLVERList);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }

            return newFLVERList;
        }

        public static void WriteFLVERGroup(string filename, string jsonString)
        {
            var writePath = Path.Combine(ExportBasePath, $"{filename}");

            if (!Directory.Exists(ExportBasePath))
            {
                Directory.CreateDirectory(ExportBasePath);
            }

            var proceed = true;

            if (File.Exists(writePath))
            {
                var result = PlatformUtils.Instance.MessageBox($"{filename} already exists as a stored FLVER. Are you sure you want to overwrite it?", "Warning", MessageBoxButtons.OKCancel);

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

                    TaskLogs.AddLog($"Saved stored FLVER: {writePath}");
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{ex}");
                }

                RefreshFLVERGroupList = true;
            }
        }

        public static void DeleteFLVERGroup(string name)
        {
            var filepath = Path.Combine(ExportBasePath, $"{name}.json");

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            RefreshFLVERGroupList = true;
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
    [JsonSerializable(typeof(FLVERList))]
    [JsonSerializable(typeof(FLVER2))]
    internal partial class StoredFLVERListContext : JsonSerializerContext
    {
    }
    public class FLVERList
    {
        public List<FLVER2> List { get; set; }

        public FLVERList()
        {
            List = new List<FLVER2>();
        }
    }
}
