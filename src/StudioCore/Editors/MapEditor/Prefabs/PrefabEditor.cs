using ImGuiNET;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StudioCore;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Prefabs;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

public class PrefabEditor
{
    public Universe universe { get; init; }
    public ViewportActionManager actionManager { get; init; }
    public RenderScene scene { get; init; }


    Dictionary<string, PrefabAttributes> prefabs = new();
    Dictionary<string, Prefab> loadedPrefabs = new();
    PrefabAttributes selectedPrefab;

    string editName = "";
    string editFlags = "";

    string prefabDir = "";


    (string name, ObjectContainer map) comboMap;


    public PrefabEditor() { }

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
            TaskLogs.AddLog($"Failed to create prefab {name}: There is already a prefab by that name", LogLevel.Error);
            return;
        }
        var newPrefab = Prefab.New(Smithbox.ProjectType);

        newPrefab.ExportSelection($@"{prefabDir}\{name}.json", name, editFlags, universe.Selection);

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
        bool selectedEntities = universe.Selection.GetFilteredSelection<MsbEntity>().Any();

        ImGui.BeginDisabled(!selectedEntities || selectedPrefab is not null || !editName.Any());

        if (ImGui.Button("Create", buttonSize))
        {
            CreateFromSelection(editName);
        }
        ImguiUtils.ShowHoverTooltip("Create a new prefab from the selected entities.");

        ImGui.EndDisabled();
    }

    void DeleteButton(Vector2 buttonSize)
    {
        ImGui.BeginDisabled(selectedPrefab is null);

        if (ImGui.Button("Delete", buttonSize))
        {
            Delete(selectedPrefab.PrefabName);
            selectedPrefab = null;
            editName = "";
            editFlags = "";
        };
        ImguiUtils.ShowHoverTooltip("Delete the selected prefab.");

        ImGui.EndDisabled();
    }

    void ImportButton(Vector2 buttonSize)
    {
        ImGui.BeginDisabled(selectedPrefab is null || comboMap.map is not MapContainer);

        if (ImGui.Button("Import", buttonSize))
        {
            string prefixName = null;
            if (CFG.Current.Prefab_ApplyOverrideName)
                prefixName = CFG.Current.Prefab_OverrideName;

            var loadedPrefab = GetLoadedPrefab(selectedPrefab.PrefabName);

            if (loadedPrefab != null)
                loadedPrefab.ImportToMap(comboMap.map as MapContainer, universe, scene, actionManager, prefixName);
        }
        ImguiUtils.ShowHoverTooltip("Import the selected prefab into a loaded map.");

        ImGui.EndDisabled();
    }


    void ReplaceButton(Vector2 buttonSize)
    {
        bool selectedEntities = universe.Selection.GetFilteredSelection<MsbEntity>().Any();

        ImGui.BeginDisabled(selectedPrefab is null || !selectedEntities);

        if (ImGui.Button("Replace", buttonSize))
        {
            Delete(selectedPrefab.PrefabName);
            CreateFromSelection(editName);
        }
        ImguiUtils.ShowHoverTooltip("Replace the selected prefab with the selected entities.");

        ImGui.EndDisabled();
    }

    void ExportConfig()
    {
        ImGui.Checkbox("Retain Entity ID", ref CFG.Current.Prefab_IncludeEntityID);
        ImguiUtils.ShowHoverTooltip("Saved objects within a prefab will retain their Entity ID. If false, their Entity ID is set to 0.");

        ImGui.Checkbox("Retain Entity Group IDs", ref CFG.Current.Prefab_IncludeEntityGroupIDs);
        ImguiUtils.ShowHoverTooltip("Saved objects within a prefab will retain their Entity Group IDs. If false, their Entity Group IDs will be set to 0.");
    }

    void ImportConfig()
    {
        ImGui.Checkbox("Override import name", ref CFG.Current.Prefab_ApplyOverrideName);
        ImguiUtils.ShowHoverTooltip("Spawned prefab objects will be prepended with this instead of the prefab name");

        if (!CFG.Current.Prefab_ApplyOverrideName)
            CFG.Current.Prefab_OverrideName = "";

        ImGui.SameLine();
        ImGui.BeginDisabled(!CFG.Current.Prefab_ApplyOverrideName);
        ImGui.PushItemWidth(-1);
        ImGui.InputText("##PrefabOverrideName", ref CFG.Current.Prefab_OverrideName, 32);
        ImGui.PopItemWidth();
        ImGui.EndDisabled();

        ImGui.Checkbox("Apply Unique Entity ID", ref CFG.Current.Prefab_ApplyUniqueEntityID);
        ImguiUtils.ShowHoverTooltip("Spawned prefab objects will be given unique Entity IDs.");

        if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Apply Unique Instance ID", ref CFG.Current.Prefab_ApplyUniqueInstanceID);
            ImguiUtils.ShowHoverTooltip("Spawned prefab objects will be given unique Instance IDs.");
        }

        if (Smithbox.ProjectType == ProjectType.DS3 || Smithbox.ProjectType == ProjectType.SDT || Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Apply Entity Group ID", ref CFG.Current.Prefab_ApplySpecificEntityGroupID);

            if (!CFG.Current.Prefab_ApplySpecificEntityGroupID)
                CFG.Current.Prefab_SpecificEntityGroupID = 0;

            ImguiUtils.ShowHoverTooltip("Spawned prefab objects will be given this specific Entity Group ID within an empty Entity Group ID slot.");

            ImGui.BeginDisabled(!CFG.Current.Prefab_ApplySpecificEntityGroupID);
            ImGui.SameLine();
            ImGui.PushItemWidth(-1);
            ImGui.InputInt("##entityGroupIdInput", ref CFG.Current.Prefab_SpecificEntityGroupID);
            ImGui.PopItemWidth();
            ImGui.EndDisabled();
        }
    }

    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Prefabs##MapEditor_PrefabEditor"))
        {
            var width = ImGui.GetWindowWidth();

            var defaultSize = new Vector2(width * 0.975f, 32);
            var thirdSizeButton = new Vector2(defaultSize.X * 0.33f, 32);

            if (ImGui.CollapsingHeader("Export Prefab"))
            {
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

            if (ImGui.CollapsingHeader("Import Prefab"))
            {

                ImGui.Text("Map:");
                ImGui.SameLine();

                if (comboMap.name != null && universe.LoadedObjectContainers[comboMap.name] == null)
                    comboMap = (null, null);

                ImGui.PushItemWidth(-1);
                if (ImGui.BeginCombo("##PrefabMapCombo", comboMap.name))
                {
                    foreach (var (name, container) in universe.LoadedObjectContainers)
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

            ImGui.Text("");
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
        ImGui.End();
        ImGui.PopStyleColor();
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