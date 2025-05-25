using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Actions.Node;
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

    public static class NodeGroups
    {
        public static string ExportBasePath = "";

        public static List<string> NodeGroupFiles = new List<string>();

        public static bool RefreshNodeGroupList = true;

        public static string _selectedNodeGroup = "";

        public static NodeList SelectedNodeList;

        public static void DisplaySubMenu(ModelEditorScreen editor)
        {
            ExportBasePath = $"{editor.Project.ProjectPath}\\.smithbox\\Workflow\\Node Groups\\";

            UpdateNodeGroupList();

            if (ImGui.BeginMenu("Replace"))
            {
                foreach (var entry in NodeGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedNodeGroup = entry;
                        SelectedNodeList = ReadNodeGroup(entry);

                        var action = new ReplaceNodeList(editor, SelectedNodeList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Append"))
            {
                foreach (var entry in NodeGroupFiles)
                {
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedNodeGroup = entry;
                        SelectedNodeList = ReadNodeGroup(entry);

                        var action = new AppendNodeList(editor, SelectedNodeList.List);
                        editor.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }
        }

        public static void UpdateNodeGroupList()
        {
            if (RefreshNodeGroupList)
            {
                RefreshNodeGroupList = false;

                NodeGroupFiles = new List<string>();

                if (Directory.Exists(ExportBasePath))
                {
                    foreach (var file in Directory.EnumerateFiles(ExportBasePath, "*.json"))
                    {
                        var fileName = Path.GetFileName(file);
                        NodeGroupFiles.Add(fileName.Replace(".json", ""));
                    }
                }
            }
        }

        public static void DisplayConfiguration(ModelEditorScreen screen)
        {
            var sectionWidth = ImGui.GetWindowWidth();
            var sectionHeight = ImGui.GetWindowHeight();
            var defaultButtonSize = new Vector2(sectionWidth, 32);

            UpdateNodeGroupList();

            UIHelper.WrappedText("Create a stored Node Group from your current selection with the Node list.");
            UIHelper.WrappedText("A stored group can then be used to replace the existing Node list, or appended to the end.");
            UIHelper.WrappedText("");

            if (ImGui.Button("Create Node Group", defaultButtonSize))
            {
                if (screen.Selection._selectedNode != -1 ||
                    screen.Selection.NodeMultiselect.StoredIndices.Count > 0)
                {
                    ImGui.OpenPopup($"##NodeGroupCreation");
                }
            }
            if (ImGui.BeginPopup("##NodeGroupCreation"))
            {
                DisplayCreationModal(screen);

                ImGui.EndPopup();
            }

            ImGui.Columns(2);

            ImGui.BeginChild("##NodeGroupSelection");

            foreach (var entry in NodeGroupFiles)
            {
                if (ImGui.Selectable($"{entry}##NodeGroup{entry}", entry == _selectedNodeGroup))
                {
                    _selectedNodeGroup = entry;
                    SelectedNodeList = ReadNodeGroup(entry);
                }
                if (_selectedNodeGroup == entry)
                {
                    if (ImGui.BeginPopupContextItem($"##NodeSelectionPopup{entry}"))
                    {
                        if (ImGui.Selectable("Delete"))
                        {
                            DeleteNodeGroup(entry);
                        }
                        UIHelper.Tooltip("Delete this Node group.");

                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("##NodeGroupActions");

            var width = ImGui.GetWindowWidth();
            var buttonWidth = width;

            if (_selectedNodeGroup != "" && SelectedNodeList != null)
            {
                if (ImGui.CollapsingHeader("Nodes in Group"))
                {
                    for (int i = 0; i < SelectedNodeList.List.Count; i++)
                    {
                        if (ImGui.Selectable($"Node {i} - {SelectedNodeList.List[i].Name}"))
                        {

                        }
                    }
                }

                if (ImGui.Button("Replace", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new ReplaceNodeList(screen, SelectedNodeList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Replace the existing Nodes with the Nodes within this Node group.");
                ImGui.SameLine();
                if (ImGui.Button("Append", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new AppendNodeList(screen, SelectedNodeList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Append to Nodes within this Node group to the existing Nodes.");
            }

            ImGui.EndChild();

            ImGui.Columns(1);
        }

        private static string _createNodeGroupName = "";

        private static void DisplayCreationModal(ModelEditorScreen screen)
        {
            var width = ImGui.GetWindowWidth();
            var buttonWidth = width / 100 * 95;

            ImGui.InputText("Name##NodeGroupName", ref _createNodeGroupName, 255);
            UIHelper.Tooltip("The name of the Node group.");

            if (ImGui.Button("Create Group", new Vector2(buttonWidth, 32)))
            {
                CreateNodeGroup(screen, _createNodeGroupName);
                ImGui.CloseCurrentPopup();
            }
        }

        public static void CreateNodeGroup(ModelEditorScreen screen, string filename)
        {
            if (!screen.ResManager.HasCurrentFLVER())
                return;

            NodeList newNodeList = new NodeList();

            for (int i = 0; i < screen.ResManager.GetCurrentFLVER().Nodes.Count; i++)
            {
                var curNode = screen.ResManager.GetCurrentFLVER().Nodes[i];

                if (screen.Selection.NodeMultiselect.StoredIndices.Count > 0)
                {
                    for (int j = 0; j < screen.Selection.NodeMultiselect.StoredIndices.Count; j++)
                    {
                        if (j == i)
                        {
                            newNodeList.List.Add(curNode);
                        }
                    }
                }
                else
                {
                    if (i == screen.Selection._selectedNode)
                    {
                        newNodeList.List.Add(curNode);
                    }
                }
            }

            var jsonString = JsonSerializer.Serialize(newNodeList, FLVERNodeListContext.Default.NodeList);

            WriteNodeGroup($"{filename}.json", jsonString);
        }

        public static NodeList ReadNodeGroup(string entry)
        {
            var newNodeList = new NodeList();
            var readPath = $"{ExportBasePath}\\{entry}.json";

            try
            {
                var jsonString = File.ReadAllText(readPath);
                newNodeList = JsonSerializer.Deserialize<NodeList>(jsonString, FLVERNodeListContext.Default.NodeList);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }

            return newNodeList;
        }

        public static void WriteNodeGroup(string filename, string jsonString)
        {
            var writePath = Path.Combine(ExportBasePath, $"{filename}");

            if (!Directory.Exists(ExportBasePath))
            {
                Directory.CreateDirectory(ExportBasePath);
            }

            var proceed = true;

            if (File.Exists(writePath))
            {
                var result = PlatformUtils.Instance.MessageBox($"{filename} already exists as a Node Group. Are you sure you want to overwrite it?", "Warning", MessageBoxButtons.OKCancel);

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

                    TaskLogs.AddLog($"Saved Node Group: {writePath}");
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{ex}");
                }

                RefreshNodeGroupList = true;
            }
        }

        public static void DeleteNodeGroup(string name)
        {
            var filepath = Path.Combine(ExportBasePath, $"{name}.json");

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            RefreshNodeGroupList = true;
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
    [JsonSerializable(typeof(NodeList))]
    [JsonSerializable(typeof(FLVER.Node))]
    internal partial class FLVERNodeListContext : JsonSerializerContext
    {
    }
    public class NodeList
    {
        public List<FLVER.Node> List { get; set; }

        public NodeList()
        {
            List = new List<FLVER.Node>();
        }
    }
}
