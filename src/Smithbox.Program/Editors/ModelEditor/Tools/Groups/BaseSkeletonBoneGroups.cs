using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Actions.BaseSkeleton;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Tools
{

    public static class BaseSkeletonBoneGroups
    {
        public static string ExportBasePath = "";

        public static List<string> BaseSkeletonGroupFiles = new List<string>();

        public static bool RefreshBaseSkeletonGroupList = true;

        public static string _selectedBaseSkeletonGroup = "";

        public static BaseSkeletonList SelectedBaseSkeletonList;

        public static void DisplaySubMenu(ModelEditorScreen editor)
        {
            ExportBasePath = $"{editor.Project.ProjectPath}\\.smithbox\\Workflow\\Base Skeleton Groups\\";

            UpdateBaseSkeletonGroupList();

            if (ImGui.BeginMenu("Replace"))
            {
                foreach (var entry in BaseSkeletonGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedBaseSkeletonGroup = entry;
                        SelectedBaseSkeletonList = ReadBaseSkeletonGroup(entry);

                        var action = new ReplaceBaseSkeletonList(editor, SelectedBaseSkeletonList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Append"))
            {
                foreach (var entry in BaseSkeletonGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedBaseSkeletonGroup = entry;
                        SelectedBaseSkeletonList = ReadBaseSkeletonGroup(entry);

                        var action = new AppendBaseSkeletonList(editor, SelectedBaseSkeletonList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }
        }

        public static void UpdateBaseSkeletonGroupList()
        {
            if (RefreshBaseSkeletonGroupList)
            {
                RefreshBaseSkeletonGroupList = false;

                BaseSkeletonGroupFiles = new List<string>();

                if (Directory.Exists(ExportBasePath))
                {
                    foreach (var file in Directory.EnumerateFiles(ExportBasePath, "*.json"))
                    {
                        var fileName = Path.GetFileName(file);
                        BaseSkeletonGroupFiles.Add(fileName.Replace(".json", ""));
                    }
                }
            }
        }

        public static void DisplayConfiguration(ModelEditorScreen screen)
        {
            var sectionWidth = ImGui.GetWindowWidth();
            var sectionHeight = ImGui.GetWindowHeight();
            var defaultButtonSize = new Vector2(sectionWidth, 32);

            UIHelper.WrappedText("Create a stored Base Skeleton Bone Group from your current selection with the Base Skeleton Bone list.");
            UIHelper.WrappedText("A stored group can then be used to replace the existing Base Skeleton Bone list, or appended to the end.");
            UIHelper.WrappedText("");

            UpdateBaseSkeletonGroupList();

            if (ImGui.Button("Create Base Skeleton Bone Group", defaultButtonSize))
            {
                if (screen.Selection._selectedBaseSkeletonBone != -1 ||
                    screen.Selection.BaseSkeletonMultiselect.StoredIndices.Count > 0)
                {
                    ImGui.OpenPopup($"##BaseSkeletonGroupCreation");
                }
            }
            if (ImGui.BeginPopup("##BaseSkeletonGroupCreation"))
            {
                DisplayCreationModal(screen);

                ImGui.EndPopup();
            }

            ImGui.Columns(2);

            ImGui.BeginChild("##BaseSkeletonGroupSelection");

            foreach (var entry in BaseSkeletonGroupFiles)
            {
                if (ImGui.Selectable($"{entry}##BaseSkeletonGroup{entry}", entry == _selectedBaseSkeletonGroup))
                {
                    _selectedBaseSkeletonGroup = entry;
                    SelectedBaseSkeletonList = ReadBaseSkeletonGroup(entry);
                }
                if (_selectedBaseSkeletonGroup == entry)
                {
                    if (ImGui.BeginPopupContextItem($"##BaseSkeletonSelectionPopup{entry}"))
                    {
                        if (ImGui.Selectable("Delete"))
                        {
                            DeleteBaseSkeletonGroup(entry);
                        }
                        UIHelper.Tooltip("Delete this Base Skeleton Bone group.");

                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("##BaseSkeletonGroupActions");

            var width = ImGui.GetWindowWidth();
            var buttonWidth = width;

            if (_selectedBaseSkeletonGroup != "" && SelectedBaseSkeletonList != null)
            {
                if (ImGui.CollapsingHeader("Base Skeleton Bones in Group"))
                {
                    for (int i = 0; i < SelectedBaseSkeletonList.List.Count; i++)
                    {
                        var nodeIndex = SelectedBaseSkeletonList.List[i].NodeIndex;
                        var alias = "";

                        if (screen.ResManager.HasCurrentFLVER())
                        {
                            if (nodeIndex < screen.ResManager.GetCurrentFLVER().Nodes.Count && nodeIndex > -1)
                                alias = screen.ResManager.GetCurrentFLVER().Nodes[nodeIndex].Name;
                        }

                        if (ImGui.Selectable($"Base Skeleton Bone {i} - {alias}"))
                        {

                        }
                    }
                }

                if (ImGui.Button("Replace", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new ReplaceBaseSkeletonList(screen, SelectedBaseSkeletonList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Replace the existing Base Skeleton Bones with the Base Skeleton Bones within this Base Skeleton group.");
                ImGui.SameLine();
                if (ImGui.Button("Append", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new AppendBaseSkeletonList(screen, SelectedBaseSkeletonList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Append Base Skeleton Bones within this Base Skeleton Bone group to the existing Base Skeleton Bones list");
            }

            ImGui.EndChild();

            ImGui.Columns(1);
        }

        private static string _createBaseSkeletonGroupName = "";

        private static void DisplayCreationModal(ModelEditorScreen screen)
        {
            var width = ImGui.GetWindowWidth();
            var buttonWidth = width / 100 * 95;

            ImGui.InputText("Name##BaseSkeletonGroupName", ref _createBaseSkeletonGroupName, 255);
            UIHelper.Tooltip("The name of the Base Skeleton group.");

            if (ImGui.Button("Create Group", new Vector2(buttonWidth, 32)))
            {
                CreateBaseSkeletonGroup(screen, _createBaseSkeletonGroupName);
                ImGui.CloseCurrentPopup();
            }
        }

        public static void CreateBaseSkeletonGroup(ModelEditorScreen screen, string filename)
        {
            if (!screen.ResManager.HasCurrentFLVER())
                return;

            BaseSkeletonList newBaseSkeletonList = new BaseSkeletonList();

            for (int i = 0; i < screen.ResManager.GetCurrentFLVER().Skeletons.BaseSkeleton.Count; i++)
            {
                var curBaseSkeleton = screen.ResManager.GetCurrentFLVER().Skeletons.BaseSkeleton[i];

                if (screen.Selection.BaseSkeletonMultiselect.StoredIndices.Count > 0)
                {
                    for (int j = 0; j < screen.Selection.BaseSkeletonMultiselect.StoredIndices.Count; j++)
                    {
                        if (j == i)
                        {
                            newBaseSkeletonList.List.Add(curBaseSkeleton);
                        }
                    }
                }
                else
                {
                    if (i == screen.Selection._selectedBaseSkeletonBone)
                    {
                        newBaseSkeletonList.List.Add(curBaseSkeleton);
                    }
                }
            }

            var jsonString = JsonSerializer.Serialize(newBaseSkeletonList, FLVERBaseSkeletonListContext.Default.BaseSkeletonList);

            WriteBaseSkeletonGroup($"{filename}.json", jsonString);
        }

        public static BaseSkeletonList ReadBaseSkeletonGroup(string entry)
        {
            var newBaseSkeletonList = new BaseSkeletonList();
            var readPath = $"{ExportBasePath}\\{entry}.json";

            try
            {
                var jsonString = File.ReadAllText(readPath);
                newBaseSkeletonList = JsonSerializer.Deserialize<BaseSkeletonList>(jsonString, FLVERBaseSkeletonListContext.Default.BaseSkeletonList);
            }
            catch (Exception ex)
            {
                var filename = Path.GetFileNameWithoutExtension(readPath);
                TaskLogs.AddLog($"Failed to read Base Skeleton Group resource file: {filename} at {readPath}.\n{ex}", LogLevel.Error);
            }

            return newBaseSkeletonList;
        }

        public static void WriteBaseSkeletonGroup(string filename, string jsonString)
        {
            var writePath = Path.Combine(ExportBasePath, $"{filename}");

            if (!Directory.Exists(ExportBasePath))
            {
                Directory.CreateDirectory(ExportBasePath);
            }

            var proceed = true;

            if (File.Exists(writePath))
            {
                var result = PlatformUtils.Instance.MessageBox($"{filename} already exists as a Base Skeleton Bone Group. Are you sure you want to overwrite it?", "Warning", MessageBoxButtons.OKCancel);

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

                    var writeFilename = Path.GetFileNameWithoutExtension(writePath);
                    TaskLogs.AddLog($"Saved Base Skeleton Bone Group resource file: {writeFilename} at {writePath}");
                }
                catch (Exception ex)
                {
                    var writeFilename = Path.GetFileNameWithoutExtension(writePath);
                    TaskLogs.AddLog($"Failed to save Base Skeleton Bone Group resource file: {writeFilename} at {writePath}.\n{ex}", LogLevel.Error);
                }

                RefreshBaseSkeletonGroupList = true;
            }
        }

        public static void DeleteBaseSkeletonGroup(string name)
        {
            var filepath = Path.Combine(ExportBasePath, $"{name}.json");

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            RefreshBaseSkeletonGroupList = true;
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
    [JsonSerializable(typeof(BaseSkeletonList))]
    [JsonSerializable(typeof(FLVER2.SkeletonSet.Bone))]
    internal partial class FLVERBaseSkeletonListContext : JsonSerializerContext
    {
    }
    public class BaseSkeletonList
    {
        public List<FLVER2.SkeletonSet.Bone> List { get; set; }

        public BaseSkeletonList()
        {
            List = new List<FLVER2.SkeletonSet.Bone>();
        }
    }
}
