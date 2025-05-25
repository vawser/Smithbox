using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;
using StudioCore;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MapEditor.Tools.Prefabs;
using StudioCore.Interface;
using StudioCore.Scene;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

public class PrefabView
{
    private MapEditorScreen Editor;
    private ViewportActionManager EditorActionManager;
    private RenderScene RenderScene;


    Dictionary<string, PrefabAttributes> prefabs = new();
    Dictionary<string, Prefab> loadedPrefabs = new();
    PrefabAttributes selectedPrefab;

    string editName = "";
    string editFlags = "";

    string prefabDir = "";


    (string name, ObjectContainer map) comboMap;


    public PrefabView(MapEditorScreen screen) 
    { 
        Editor = screen;
        EditorActionManager = screen.EditorActionManager;
        RenderScene = screen.MapViewportView.RenderScene;
    }

    Prefab GetLoadedPrefab(string name)
    {
        var prefabPath = $@"{prefabDir}\{name}.json";
        var loadedPrefab = loadedPrefabs.GetValueOrDefault(name);

        if (loadedPrefab is not null)
            return loadedPrefab;

        loadedPrefab = Prefab.New(Editor);

        if (File.Exists(prefabPath))
        {
            loadedPrefab.ImportJson(prefabPath);
            loadedPrefabs[name] = loadedPrefab;
        }

        return loadedPrefab;
    }

    void CreateFromSelection(string name)
    {
        if (prefabs.ContainsKey(name))
        {
            TaskLogs.AddLog($"Failed to create prefab {name}: prefab already exists with this name.", LogLevel.Error);
            return;
        }
        var newPrefab = Prefab.New(Editor);

        newPrefab.ExportSelection($@"{prefabDir}\{name}.json", name, editFlags, Editor.Universe.Selection);

        prefabs.Add(name, newPrefab);
        selectedPrefab = newPrefab;
        loadedPrefabs.Remove(name);
    }

    void Delete(string name)
    {
        prefabs.Remove(name);
        File.Delete($@"{prefabDir}\{name}.json");
    }

    void CreateButton(Vector2 buttonSize)
    {
        bool selectedEntities = Editor.Universe.Selection.GetFilteredSelection<MsbEntity>().Any();

        var isDisabled = !selectedEntities || selectedPrefab is not null || !editName.Any();

        if (isDisabled)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button("Create##createPrefab", buttonSize))
            {
                CreateFromSelection(editName);
            }
            UIHelper.Tooltip("Create a new prefab from the selected entities.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button("Create##createPrefab", buttonSize))
            {
                CreateFromSelection(editName);
            }
            UIHelper.Tooltip("Create a new prefab from the selected entities.");
        }
    }

    void DeleteButton(Vector2 buttonSize)
    {
        ImGui.BeginDisabled(selectedPrefab is null);

        if (ImGui.Button("Delete##deletePrefab", buttonSize))
        {
            Delete(selectedPrefab.PrefabName);
            selectedPrefab = null;
            editName = "";
            editFlags = "";
        };
        UIHelper.Tooltip("Delete the selected prefab.");

        ImGui.EndDisabled();
    }

    void ImportButton(Vector2 buttonSize)
    {
        ImGui.BeginDisabled(selectedPrefab is null || comboMap.map is not MapContainer);

        if (ImGui.Button("Import##importPrefab", buttonSize))
        {
            string prefixName = null;
            if (CFG.Current.Prefab_ApplyOverrideName)
                prefixName = CFG.Current.Prefab_OverrideName;

            var loadedPrefab = GetLoadedPrefab(selectedPrefab.PrefabName);

            if (loadedPrefab != null)
                loadedPrefab.ImportToMap(Editor, comboMap.map as MapContainer, RenderScene, EditorActionManager, prefixName);
        }
        UIHelper.Tooltip("Import the selected prefab into a loaded map.");

        ImGui.EndDisabled();
    }


    void ReplaceButton(Vector2 buttonSize)
    {
        bool selectedEntities = Editor.Universe.Selection.GetFilteredSelection<MsbEntity>().Any();

        ImGui.BeginDisabled(selectedPrefab is null || !selectedEntities);

        if (ImGui.Button("Replace##replacePrefab", buttonSize))
        {
            Delete(selectedPrefab.PrefabName);
            CreateFromSelection(editName);
        }
        UIHelper.Tooltip("Replace the selected prefab with the selected entities.");

        ImGui.EndDisabled();
    }

    void ExportConfig()
    {
        ImGui.Checkbox("Retain Entity ID##prefabRetainEntityID", ref CFG.Current.Prefab_IncludeEntityID);
        UIHelper.Tooltip("Saved objects within a prefab will retain their Entity ID. If false, their Entity ID is set to 0.");

        ImGui.Checkbox("Retain Entity Group IDs##prefabRetainGroupEntityIDs", ref CFG.Current.Prefab_IncludeEntityGroupIDs);
        UIHelper.Tooltip("Saved objects within a prefab will retain their Entity Group IDs. If false, their Entity Group IDs will be set to 0.");
    }

    void ImportConfig()
    {
        ImGui.Checkbox("Override import name##prefabOverrideImportName", ref CFG.Current.Prefab_ApplyOverrideName);
        UIHelper.Tooltip("Spawned prefab objects will be prepended with this instead of the prefab name");

        if (!CFG.Current.Prefab_ApplyOverrideName)
            CFG.Current.Prefab_OverrideName = "";

        ImGui.SameLine();
        ImGui.BeginDisabled(!CFG.Current.Prefab_ApplyOverrideName);
        ImGui.PushItemWidth(-1);
        ImGui.InputText("##PrefabOverrideName", ref CFG.Current.Prefab_OverrideName, 32);
        ImGui.PopItemWidth();
        ImGui.EndDisabled();

        ImGui.Checkbox("Apply Unique Entity ID##prefabApplyUniqueEntityID", ref CFG.Current.Prefab_ApplyUniqueEntityID);
        UIHelper.Tooltip("Spawned prefab objects will be given unique Entity IDs.");

        if (Editor.Project.ProjectType == ProjectType.ER || Editor.Project.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Apply Unique Instance ID##prefabApplyUniqueInstanceID", ref CFG.Current.Prefab_ApplyUniqueInstanceID);
            UIHelper.Tooltip("Spawned prefab objects will be given unique Instance IDs.");
        }

        if (Editor.Project.ProjectType == ProjectType.DS3 || Editor.Project.ProjectType == ProjectType.SDT || Editor.Project.ProjectType == ProjectType.ER || Editor.Project.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Apply Entity Group ID##prefabApplyEntityGroupID", ref CFG.Current.Prefab_ApplySpecificEntityGroupID);

            if (!CFG.Current.Prefab_ApplySpecificEntityGroupID)
                CFG.Current.Prefab_SpecificEntityGroupID = 0;

            UIHelper.Tooltip("Spawned prefab objects will be given this specific Entity Group ID within an empty Entity Group ID slot.");

            ImGui.BeginDisabled(!CFG.Current.Prefab_ApplySpecificEntityGroupID);
            ImGui.SameLine();
            ImGui.PushItemWidth(-1);
            ImGui.InputInt("##entityGroupIdInput", ref CFG.Current.Prefab_SpecificEntityGroupID);
            ImGui.PopItemWidth();
            ImGui.EndDisabled();
        }
    }

    public void ImportPrefabMenu()
    {
        var width = ImGui.GetWindowWidth();
        var defaultSize = new Vector2(width * 0.975f, 32);

        ImGui.Text("Map:");
        ImGui.SameLine();

        var container = Editor.GetMapContainerFromMapID(comboMap.name);

        if (comboMap.name != null && container == null)
            comboMap = (null, null);

        ImGui.PushItemWidth(-1);
        if (ImGui.BeginCombo("##PrefabMapCombo", comboMap.name))
        {
            foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
            {
                if (entry.Value.MapContainer == null)
                    continue;

                if (ImGui.Selectable(entry.Key.Filename))
                {
                    comboMap = (entry.Key.Filename, entry.Value.MapContainer);
                }
            }
            ImGui.EndCombo();
        }
        ImGui.PopItemWidth();

        ImportButton(defaultSize);

        ImGui.Text("Import Options:");
        ImportConfig();
    }

    public void ExportPrefabMenu()
    {
        var width = ImGui.GetWindowWidth();
        var defaultSize = new Vector2(width * 0.975f, 32);
        var thirdSizeButton = new Vector2(defaultSize.X * 0.33f, 32);

        ImGui.Text("Name:");
        ImGui.SetNextItemWidth(defaultSize.X);
        ImGui.InputText("##PrefabName", ref editName, 64);

        ImGui.Text("Flags:");
        ImGui.SetNextItemWidth(defaultSize.X);
        ImGui.InputText("##PrefabFlags", ref editFlags, 64);

        CreateButton(thirdSizeButton);
        ImGui.SameLine();
        DeleteButton(thirdSizeButton);
        ImGui.SameLine();
        ReplaceButton(thirdSizeButton);

        ImGui.Text("Export Options:");
        ExportConfig();
        ImGui.Text("");
    }

    public void PrefabTree()
    {
        ImGui.Text("Prefabs:");
        if (ImGui.BeginChild("PrefabEditorTree"))
        {
            foreach (var (name, prefab) in prefabs)
            {
                bool selected = selectedPrefab == prefab;
                var flag = ImGuiTreeNodeFlags.OpenOnArrow;
                if (selected)
                    flag |= ImGuiTreeNodeFlags.Selected;

                bool opened = ImGui.TreeNodeEx($"{name}##PrefabTreeNode", flag);

                if (ImGui.IsItemClicked())
                {
                    selectedPrefab = prefab;
                    editName = name;
                    editFlags = "";
                    if (prefab.TagList != null)
                    {
                        editFlags = string.Join(',', prefab.TagList);
                    }
                }

                if (opened)
                {
                    ImGui.Indent();
                    var loadedPrefab = GetLoadedPrefab(name);
                    if (loadedPrefab != null)
                    {
                        foreach (var entName in loadedPrefab.GetSelectedPrefabObjects())
                        {
                            ImGui.Text(entName);
                        }
                    }
                    ImGui.Unindent();
                    ImGui.TreePop();
                }
                else
                {
                    if (loadedPrefabs.ContainsKey(name))
                        loadedPrefabs.Remove(name);
                }

            }
        }

        ImGui.EndChild();
        if (ImGui.IsItemClicked() && selectedPrefab is not null)
        {
            selectedPrefab = null;
            editName = "";
            editFlags = "";
        }
    }

    public void OnProjectChanged()
    {
        if (Editor.Project.ProjectType == ProjectType.Undefined)
            return;

        RefreshPrefabList();
        selectedPrefab = null;
        editName = "";
        editFlags = "";
        comboMap = (null, null);
    }

    public void RefreshPrefabList()
    {
        prefabs = new();
        prefabDir = $"{Editor.Project.ProjectPath}\\.smithbox\\{ProjectUtils.GetGameDirectory(Editor.Project)}\\prefabs\\";
        if (!Directory.Exists(prefabDir))
        {
            try { Directory.CreateDirectory(prefabDir); } catch { }
        }

        if(Directory.Exists(prefabDir))
        {
            string[] files = Directory.GetFiles(prefabDir, "*.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var prefab = JsonConvert.DeserializeObject<PrefabAttributes>(File.ReadAllText(file));
                prefabs.Add(prefab.PrefabName, prefab);
            }
        }
    }
}