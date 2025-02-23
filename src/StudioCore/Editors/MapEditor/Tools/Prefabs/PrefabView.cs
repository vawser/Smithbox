using ImGuiNET;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StudioCore;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MapEditor.Tools.Prefabs;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Resource.Locators;
using StudioCore.Scene;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

public class PrefabView
{
    private MapEditorScreen Screen;
    private Universe Universe;
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
        Screen = screen;
        Universe = screen.Universe;
        EditorActionManager = screen.EditorActionManager;
        RenderScene = screen.MapViewportView.RenderScene;
    }

    Prefab GetLoadedPrefab(string name)
    {
        var prefabPath = $@"{prefabDir}\{name}.json";
        var loadedPrefab = loadedPrefabs.GetValueOrDefault(name);

        if (loadedPrefab is not null)
            return loadedPrefab;

        loadedPrefab = Prefab.New(Smithbox.ProjectType);

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
        var newPrefab = Prefab.New(Smithbox.ProjectType);

        newPrefab.ExportSelection($@"{prefabDir}\{name}.json", name, editFlags, Universe.Selection);

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
        bool selectedEntities = Universe.Selection.GetFilteredSelection<MsbEntity>().Any();

        var isDisabled = !selectedEntities || selectedPrefab is not null || !editName.Any();

        if (isDisabled)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button("Create##createPrefab", buttonSize))
            {
                CreateFromSelection(editName);
            }
            UIHelper.ShowHoverTooltip("Create a new prefab from the selected entities.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button("Create##createPrefab", buttonSize))
            {
                CreateFromSelection(editName);
            }
            UIHelper.ShowHoverTooltip("Create a new prefab from the selected entities.");
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
        UIHelper.ShowHoverTooltip("Delete the selected prefab.");

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
                loadedPrefab.ImportToMap(comboMap.map as MapContainer, Universe, RenderScene, EditorActionManager, prefixName);
        }
        UIHelper.ShowHoverTooltip("Import the selected prefab into a loaded map.");

        ImGui.EndDisabled();
    }


    void ReplaceButton(Vector2 buttonSize)
    {
        bool selectedEntities = Universe.Selection.GetFilteredSelection<MsbEntity>().Any();

        ImGui.BeginDisabled(selectedPrefab is null || !selectedEntities);

        if (ImGui.Button("Replace##replacePrefab", buttonSize))
        {
            Delete(selectedPrefab.PrefabName);
            CreateFromSelection(editName);
        }
        UIHelper.ShowHoverTooltip("Replace the selected prefab with the selected entities.");

        ImGui.EndDisabled();
    }

    void ExportConfig()
    {
        ImGui.Checkbox("Retain Entity ID##prefabRetainEntityID", ref CFG.Current.Prefab_IncludeEntityID);
        UIHelper.ShowHoverTooltip("Saved objects within a prefab will retain their Entity ID. If false, their Entity ID is set to 0.");

        ImGui.Checkbox("Retain Entity Group IDs##prefabRetainGroupEntityIDs", ref CFG.Current.Prefab_IncludeEntityGroupIDs);
        UIHelper.ShowHoverTooltip("Saved objects within a prefab will retain their Entity Group IDs. If false, their Entity Group IDs will be set to 0.");
    }

    void ImportConfig()
    {
        ImGui.Checkbox("Override import name##prefabOverrideImportName", ref CFG.Current.Prefab_ApplyOverrideName);
        UIHelper.ShowHoverTooltip("Spawned prefab objects will be prepended with this instead of the prefab name");

        if (!CFG.Current.Prefab_ApplyOverrideName)
            CFG.Current.Prefab_OverrideName = "";

        ImGui.SameLine();
        ImGui.BeginDisabled(!CFG.Current.Prefab_ApplyOverrideName);
        ImGui.PushItemWidth(-1);
        ImGui.InputText("##PrefabOverrideName", ref CFG.Current.Prefab_OverrideName, 32);
        ImGui.PopItemWidth();
        ImGui.EndDisabled();

        ImGui.Checkbox("Apply Unique Entity ID##prefabApplyUniqueEntityID", ref CFG.Current.Prefab_ApplyUniqueEntityID);
        UIHelper.ShowHoverTooltip("Spawned prefab objects will be given unique Entity IDs.");

        if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Apply Unique Instance ID##prefabApplyUniqueInstanceID", ref CFG.Current.Prefab_ApplyUniqueInstanceID);
            UIHelper.ShowHoverTooltip("Spawned prefab objects will be given unique Instance IDs.");
        }

        if (Smithbox.ProjectType == ProjectType.DS3 || Smithbox.ProjectType == ProjectType.SDT || Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Apply Entity Group ID##prefabApplyEntityGroupID", ref CFG.Current.Prefab_ApplySpecificEntityGroupID);

            if (!CFG.Current.Prefab_ApplySpecificEntityGroupID)
                CFG.Current.Prefab_SpecificEntityGroupID = 0;

            UIHelper.ShowHoverTooltip("Spawned prefab objects will be given this specific Entity Group ID within an empty Entity Group ID slot.");

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

        if (comboMap.name != null && Universe.LoadedObjectContainers[comboMap.name] == null)
            comboMap = (null, null);

        ImGui.PushItemWidth(-1);
        if (ImGui.BeginCombo("##PrefabMapCombo", comboMap.name))
        {
            foreach (var (name, container) in Universe.LoadedObjectContainers)
            {
                if (container is null) continue;
                if (ImGui.Selectable(name))
                {
                    comboMap = (name, container);
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
        if (Smithbox.ProjectType == ProjectType.Undefined)
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
        prefabDir = $"{Smithbox.ProjectRoot}\\.smithbox\\{MiscLocator.GetGameIDForDir()}\\prefabs\\";
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