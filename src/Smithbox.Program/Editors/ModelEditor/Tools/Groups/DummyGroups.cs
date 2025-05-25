using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Actions.Dummy;
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
    public static class DummyGroups
    {
        public static string ExportBasePath = "";

        public static List<string> DummyGroupFiles = new List<string>();

        public static bool RefreshDummyGroupList = true;

        public static string _selectedDummyGroup = "";

        public static DummyList SelectedDummyList;

        public static void DisplaySubMenu(ModelEditorScreen editor)
        {
            ExportBasePath = $"{editor.Project.ProjectPath}\\.smithbox\\Workflow\\Dummy Groups\\";

            UpdateDummyGroupList();

            if (ImGui.BeginMenu("Replace"))
            {
                foreach (var entry in DummyGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedDummyGroup = entry;
                        SelectedDummyList = ReadDummyGroup(entry);

                        var action = new ReplaceDummyList(editor, SelectedDummyList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Append"))
            {
                foreach (var entry in DummyGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedDummyGroup = entry;
                        SelectedDummyList = ReadDummyGroup(entry);

                        var action = new AppendDummyList(editor, SelectedDummyList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }
        }

        public static void UpdateDummyGroupList()
        {
            if (RefreshDummyGroupList)
            {
                RefreshDummyGroupList = false;

                DummyGroupFiles = new List<string>();

                if (Directory.Exists(ExportBasePath))
                {
                    foreach (var file in Directory.EnumerateFiles(ExportBasePath, "*.json"))
                    {
                        var fileName = Path.GetFileName(file);
                        DummyGroupFiles.Add(fileName.Replace(".json", ""));
                    }
                }
            }
        }

        public static void DisplayConfiguration(ModelEditorScreen screen)
        {
            var sectionWidth = ImGui.GetWindowWidth();
            var sectionHeight = ImGui.GetWindowHeight();
            var defaultButtonSize = new Vector2(sectionWidth, 32);

            UIHelper.WrappedText("Create a stored Dummy Group from your current selection with the Dummy list.");
            UIHelper.WrappedText("A stored group can then be used to replace the existing Dummy list, or appended to the end.");
            UIHelper.WrappedText("");

            UpdateDummyGroupList();

            if (ImGui.Button("Create Dummy Group", defaultButtonSize))
            {
                if(screen.Selection._selectedDummy != -1 || 
                    screen.Selection.DummyMultiselect.StoredIndices.Count > 0)
                {
                    ImGui.OpenPopup($"##dummyGroupCreation");
                }
            }
            if (ImGui.BeginPopup("##dummyGroupCreation"))
            {
                DisplayCreationModal(screen);

                ImGui.EndPopup();
            }

            ImGui.Columns(2);

            ImGui.BeginChild("##dummyGroupSelection");

            foreach(var entry in DummyGroupFiles)
            {
                if(ImGui.Selectable($"{entry}##dummyGroup{entry}", entry == _selectedDummyGroup))
                {
                    _selectedDummyGroup = entry;
                    SelectedDummyList = ReadDummyGroup(entry);
                }
                if (_selectedDummyGroup == entry)
                {
                    if (ImGui.BeginPopupContextItem($"##dummySelectionPopup{entry}"))
                    {
                        if (ImGui.Selectable("Delete"))
                        {
                            DeleteDummyGroup(entry);
                        }
                        UIHelper.Tooltip("Delete this dummy group.");

                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("##dummyGroupActions");

            var width = ImGui.GetWindowWidth();
            var buttonWidth = width;    

            if (_selectedDummyGroup != "" && SelectedDummyList != null)
            {
                if(ImGui.CollapsingHeader("Dummies in Group"))
                {
                    for(int i = 0; i < SelectedDummyList.List.Count; i++)
                    { 
                        if(ImGui.Selectable($"Dummy {i} - {SelectedDummyList.List[i].ReferenceID}"))
                        {

                        }
                    }
                }

                if (ImGui.Button("Replace", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new ReplaceDummyList(screen, SelectedDummyList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Replace the existing dummies with the dummies within this dummy group.");
                ImGui.SameLine();
                if (ImGui.Button("Append", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new AppendDummyList(screen, SelectedDummyList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Append to dummies within this dummy group to the existing dummies.");
            }

            ImGui.EndChild();

            ImGui.Columns(1);
        }

        private static string _createDummyGroupName = "";

        private static void DisplayCreationModal(ModelEditorScreen screen)
        {
            var width = ImGui.GetWindowWidth();
            var buttonWidth = width / 100 * 95;

            ImGui.InputText("Name##dummyGroupName", ref _createDummyGroupName, 255);
            UIHelper.Tooltip("The name of the dummy group.");

            if (ImGui.Button("Create Group", new Vector2(buttonWidth, 32)))
            {
                CreateDummyGroup(screen, _createDummyGroupName);
                ImGui.CloseCurrentPopup();
            }
        }

        public static void CreateDummyGroup(ModelEditorScreen screen, string filename)
        {
            if (!screen.ResManager.HasCurrentFLVER())
                return;

            DummyList newDummyList = new DummyList();

            for (int i = 0; i < screen.ResManager.GetCurrentFLVER().Dummies.Count; i++)
            {
                var curDummy = screen.ResManager.GetCurrentFLVER().Dummies[i];

                if (screen.Selection.DummyMultiselect.StoredIndices.Count > 0)
                {
                    for (int j = 0; j < screen.Selection.DummyMultiselect.StoredIndices.Count; j++)
                    {
                        if (j == i)
                        {
                            newDummyList.List.Add(curDummy);
                        }
                    }
                }
                else
                {
                    if (i == screen.Selection._selectedDummy)
                    {
                        newDummyList.List.Add(curDummy);
                    }
                }
            }

            var jsonString = JsonSerializer.Serialize(newDummyList, FLVERDummyListContext.Default.DummyList);

            WriteDummyGroup($"{filename}.json", jsonString);
        }

        public static DummyList ReadDummyGroup(string entry)
        {
            var newDummyList = new DummyList();
            var readPath = $"{ExportBasePath}\\{entry}.json";

            try
            {
                var jsonString = File.ReadAllText(readPath);
                newDummyList = JsonSerializer.Deserialize<DummyList>(jsonString, FLVERDummyListContext.Default.DummyList);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }

            return newDummyList;
        }

        public static void WriteDummyGroup(string filename, string jsonString)
        {
            var writePath = Path.Combine(ExportBasePath, $"{filename}");

            if(!Directory.Exists(ExportBasePath))
            {
                Directory.CreateDirectory(ExportBasePath);
            }

            var proceed = true;

            if(File.Exists(writePath))
            {
                var result = PlatformUtils.Instance.MessageBox($"{filename} already exists as a Dummy Group. Are you sure you want to overwrite it?", "Warning", MessageBoxButtons.OKCancel);

                if(result is DialogResult.Cancel)
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

                    TaskLogs.AddLog($"Saved Dummy Group: {writePath}");
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{ex}");
                }

                RefreshDummyGroupList = true;
            }
        }

        public static void DeleteDummyGroup(string name)
        {
            var filepath = Path.Combine(ExportBasePath, $"{name}.json");

            if(File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            RefreshDummyGroupList = true;
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
    [JsonSerializable(typeof(DummyList))]
    [JsonSerializable(typeof(FLVER.Dummy))]
    internal partial class FLVERDummyListContext : JsonSerializerContext
    {
    }
    public class DummyList
    {
        public List<FLVER.Dummy> List { get; set; }

        public DummyList()
        {
            List = new List<FLVER.Dummy>();
        }
    }
}
