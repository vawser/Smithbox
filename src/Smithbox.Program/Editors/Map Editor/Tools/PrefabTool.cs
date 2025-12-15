using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;
public class PrefabTool
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public Dictionary<string, PrefabAttributes> Prefabs = new();
    public Dictionary<string, Prefab> LoadedPrefabs = new();
    public PrefabAttributes SelectedPrefab;

    public string Prefab_EditName = "";
    public string Prefab_EditFlags = "";

    public (string name, ObjectContainer map) comboMap;

    public PrefabTool(MapEditorScreen editor, ProjectEntry project) 
    { 
        Editor = editor;
        Project = project;

        RefreshPrefabList();
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {

    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Import Prefab"))
        {
            ImportPrefabMenu();
            PrefabTree();
        }

        if (ImGui.CollapsingHeader("Export Prefab"))
        {
            ExportPrefabMenu();
            PrefabTree();
        }
    }

    public Prefab GetLoadedPrefab(string name)
    {
        var prefabDir = PrefabUtils.GetPrefabStorageDirectory(Editor.Project);
        var prefabPath = Path.Join(prefabDir, $"{name}.json");
        var loadedPrefab = LoadedPrefabs.GetValueOrDefault(name);

        if (loadedPrefab is not null)
            return loadedPrefab;

        loadedPrefab = Prefab.New(Editor);

        if (File.Exists(prefabPath))
        {
            loadedPrefab.ImportJson(prefabPath);
            LoadedPrefabs[name] = loadedPrefab;
        }

        return loadedPrefab;
    }

    public void CreateFromSelection(string name)
    {
        var prefabDir = PrefabUtils.GetPrefabStorageDirectory(Editor.Project);

        if (Prefabs.ContainsKey(name))
        {
            TaskLogs.AddLog($"Failed to create prefab {name}: prefab already exists with this name.", LogLevel.Error);
            return;
        }
        var newPrefab = Prefab.New(Editor);

        if (newPrefab == null)
        {
            TaskLogs.AddLog("Prefabs are not supported for this project type.");
        }
        else
        {
            newPrefab.ExportSelection(Path.Join(prefabDir, $"{name}.json"), name, Prefab_EditFlags, Editor.Universe.Selection);

            Prefabs.Add(name, newPrefab);
            SelectedPrefab = newPrefab;
            LoadedPrefabs.Remove(name);

            RefreshPrefabList();
        }
    }

    public void Delete(string name)
    {
        var prefabDir = PrefabUtils.GetPrefabStorageDirectory(Editor.Project);

        Prefabs.Remove(name);
        File.Delete(Path.Join(prefabDir, $"{name}.json"));
    }

    public void CreateButton()
    {
        var windowWidth = ImGui.GetWindowWidth();

        bool selectedEntities = Editor.Universe.Selection.GetFilteredSelection<MsbEntity>().Any();

        var isDisabled = !selectedEntities || !Prefab_EditName.Any();

        if (isDisabled)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button("Create##createPrefab", DPI.ThirdWidthButton(windowWidth, 24)))
            {
                CreateFromSelection(Prefab_EditName);
            }
            UIHelper.Tooltip("Create a new prefab from the selected entities.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button("Create##createPrefab", DPI.ThirdWidthButton(windowWidth, 24)))
            {
                CreateFromSelection(Prefab_EditName);
            }
            UIHelper.Tooltip("Create a new prefab from the selected entities.");
        }
    }

    public void DeleteButton()
    {
        var windowWidth = ImGui.GetWindowWidth();

        ImGui.BeginDisabled(SelectedPrefab is null);

        if (ImGui.Button("Delete##deletePrefab", DPI.ThirdWidthButton(windowWidth, 24)))
        {
            Delete(SelectedPrefab.PrefabName);
            SelectedPrefab = null;
            Prefab_EditName = "";
            Prefab_EditFlags = "";
        };
        UIHelper.Tooltip("Delete the selected prefab.");

        ImGui.EndDisabled();
    }

    public void ImportButton()
    {
        var windowWidth = ImGui.GetWindowWidth();

        ImGui.BeginDisabled(SelectedPrefab is null || comboMap.map is not MapContainer);

        if (ImGui.Button("Import##importPrefab", DPI.WholeWidthButton(windowWidth, 24)))
        {
            string prefixName = null;
            if (CFG.Current.Prefab_ApplyOverrideName)
                prefixName = CFG.Current.Prefab_OverrideName;

            var loadedPrefab = GetLoadedPrefab(SelectedPrefab.PrefabName);

            if (loadedPrefab != null)
                loadedPrefab.ImportToMap(Editor, comboMap.map as MapContainer, Editor.MapViewportView.RenderScene, Editor.EditorActionManager, prefixName);
        }
        UIHelper.Tooltip("Import the selected prefab into a loaded map.");

        ImGui.EndDisabled();
    }

    public void ReplaceButton()
    {
        var windowWidth = ImGui.GetWindowWidth();

        bool selectedEntities = Editor.Universe.Selection.GetFilteredSelection<MsbEntity>().Any();

        ImGui.BeginDisabled(SelectedPrefab is null || !selectedEntities);

        if (ImGui.Button("Replace##replacePrefab", DPI.ThirdWidthButton(windowWidth, 24)))
        {
            Delete(SelectedPrefab.PrefabName);
            CreateFromSelection(Prefab_EditName);
        }
        UIHelper.Tooltip("Replace the selected prefab with the selected entities.");

        ImGui.EndDisabled();
    }

    public void ExportConfig()
    {
        ImGui.Checkbox("Retain Entity ID##prefabRetainEntityID", ref CFG.Current.Prefab_IncludeEntityID);
        UIHelper.Tooltip("Saved objects within a prefab will retain their Entity ID. If false, their Entity ID is set to 0.");

        ImGui.Checkbox("Retain Entity Group IDs##prefabRetainGroupEntityIDs", ref CFG.Current.Prefab_IncludeEntityGroupIDs);
        UIHelper.Tooltip("Saved objects within a prefab will retain their Entity Group IDs. If false, their Entity Group IDs will be set to 0.");
    }

    public void ImportConfig()
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

        ImGui.Checkbox("Import on Placement Orb Origin##prefabPlaceAtPlacementOrb", ref CFG.Current.Prefab_PlaceAtPlacementOrb);
        UIHelper.Tooltip("Spawned prefab objects will be placed at the placement orb origin rather than their original co-ordinates.");

        ImGui.Checkbox("Apply Unique Entity ID##prefabApplyUniqueEntityID", ref CFG.Current.Prefab_ApplyUniqueEntityID);
        UIHelper.Tooltip("Spawned prefab objects will be given unique Entity IDs.");

        if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            ImGui.Checkbox("Apply Unique Instance ID##prefabApplyUniqueInstanceID", ref CFG.Current.Prefab_ApplyUniqueInstanceID);
            UIHelper.Tooltip("Spawned prefab objects will be given unique Instance IDs.");
        }

        if (Editor.Project.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
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

        UIHelper.SimpleHeader("Target Map", "Target Map", "The map to spawn the selected prefab in.", UI.Current.ImGui_Default_Text_Color);

        var container = Editor.Selection.GetMapContainerFromMapID(comboMap.name);

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

        UIHelper.SimpleHeader("Import Options", "Options", "", UI.Current.ImGui_Default_Text_Color);
        ImportConfig();

        ImportButton();
    }

    public void ExportPrefabMenu()
    {
        var windowWidth = ImGui.GetWindowWidth();

        UIHelper.SimpleHeader("Name", "Name", "The name of the prefab to save.", UI.Current.ImGui_Default_Text_Color);
        DPI.ApplyInputWidth(windowWidth);
        ImGui.InputText("##PrefabName", ref Prefab_EditName, 64);

        UIHelper.SimpleHeader("Tags", "Tags", "The prefab tags to associate with this prefab.", UI.Current.ImGui_Default_Text_Color);
        DPI.ApplyInputWidth(windowWidth);
        ImGui.InputText("##PrefabFlags", ref Prefab_EditFlags, 64);

        UIHelper.SimpleHeader("Export Options", "Options", "", UI.Current.ImGui_Default_Text_Color);
        ExportConfig();

        CreateButton();
        ImGui.SameLine();
        DeleteButton();
        ImGui.SameLine();
        ReplaceButton();

    }

    public void PrefabTree()
    {
        var windowSize = DPI.GetWindowSize(Editor.BaseEditor._context);
        var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
        var sectionHeight = windowSize.Y * 0.3f;
        var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

        UIHelper.SimpleHeader("Prefabs", "Prefabs", "", UI.Current.ImGui_Default_Text_Color);

        if (ImGui.BeginChild("PrefabEditorTree", sectionSize, ImGuiChildFlags.Borders))
        {
            int index = 0;

            foreach (var (name, prefab) in Prefabs)
            {
                bool selected = SelectedPrefab == prefab;
                var flag = ImGuiTreeNodeFlags.OpenOnArrow;
                if (selected)
                    flag |= ImGuiTreeNodeFlags.Selected;

                bool opened = ImGui.TreeNodeEx($"{name}##PrefabTreeNode{index}", flag);

                if (ImGui.IsItemClicked())
                {
                    SelectedPrefab = prefab;
                    Prefab_EditName = name;
                    Prefab_EditFlags = "";
                    if (prefab.TagList != null)
                    {
                        Prefab_EditFlags = string.Join(',', prefab.TagList);
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
                    if (LoadedPrefabs.ContainsKey(name))
                        LoadedPrefabs.Remove(name);
                }

                index++;
            }
        }

        ImGui.EndChild();
        if (ImGui.IsItemClicked() && SelectedPrefab is not null)
        {
            SelectedPrefab = null;
            Prefab_EditName = "";
            Prefab_EditFlags = "";
        }
    }

    public void RefreshPrefabList()
    {
        var prefabDir = PrefabUtils.GetPrefabStorageDirectory(Editor.Project);
        Prefabs = new();

        if (!Directory.Exists(prefabDir))
        {
            try { 
                Directory.CreateDirectory(prefabDir); 
            } 
            catch { }
        }

        if(Directory.Exists(prefabDir))
        {
            string[] files = Directory.GetFiles(prefabDir, "*.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var settings = new JsonSerializerSettings
                {
                    Converters = { new PrefabAttributesConverter(Editor) }
                };

                var prefab = JsonConvert.DeserializeObject<PrefabAttributes>(File.ReadAllText(file), settings);

                Prefabs.Add(prefab.PrefabName, prefab);
            }
        }
    }
}