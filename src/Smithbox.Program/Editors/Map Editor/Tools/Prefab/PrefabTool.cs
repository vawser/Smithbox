using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace StudioCore.Editors.MapEditor;

public class PrefabTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    public Dictionary<string, PrefabAttributes> Prefabs = new();
    public Dictionary<string, Prefab> LoadedPrefabs = new();
    public PrefabAttributes SelectedPrefab;

    public string Prefab_EditName = "";
    public string Prefab_EditFlags = "";
    
    public (string, ObjectContainer) TargetMap = ("None", null);

    public PrefabTool(MapEditorView view, ProjectEntry project) 
    { 
        View = view;
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
        ImGui.BeginChild("prefabSection", ImGuiChildFlags.Borders);

        ImGui.BeginTabBar("prefabTabs");

        if (ImGui.BeginTabItem("Import"))
        {
            DisplayImportMenu();

            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Export"))
        {
            DisplayExportMenu();

            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();

        ImGui.EndChild();
    }

    public void DisplayImportMenu()
    {
        GUI.WrappedText("Use this to import a set of pre-defined map objects into the target loaded map.");

        GUI.Spacer();
        GUI.SimpleHeader("Target Map", "The target map to duplicate the current selection to.");

        GUI.SetInputWidth();
        if (ImGui.BeginCombo("##targetMapSelect", TargetMap.Item1))
        {
            foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
            {
                var map = entry.Value.MapContainer;

                if (map == null)
                    continue;

                var mapID = entry.Key.Filename;
                var mapName = AliasHelper.GetMapNameAlias(View.Project, mapID);
                var displayName = $"{mapID}: {mapName}";

                if (ImGui.Selectable(displayName, TargetMap.Item1 == mapID))
                {
                    TargetMap = (mapID, map);
                }
            }

            ImGui.EndCombo();
        }

        GUI.Spacer();
        GUI.SimpleHeader("Options", "");

        ImGui.Checkbox("Override import name##prefabOverrideImportName", ref CFG.Current.Prefab_ApplyOverrideName);
        GUI.Tooltip("Spawned prefab objects will be prepended with this instead of the prefab name");

        if (!CFG.Current.Prefab_ApplyOverrideName)
            CFG.Current.Prefab_OverrideName = "";

        ImGui.Checkbox("Import on Placement Orb Origin##prefabPlaceAtPlacementOrb", ref CFG.Current.Prefab_PlaceAtPlacementOrb);
        GUI.Tooltip("Spawned prefab objects will be placed at the placement orb origin rather than their original co-ordinates.");

        ImGui.Checkbox("Apply Unique Entity ID##prefabApplyUniqueEntityID", ref CFG.Current.Prefab_ApplyUniqueEntityID);
        GUI.Tooltip("Spawned prefab objects will be given unique Entity IDs.");

        if (View.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            ImGui.Checkbox("Apply Unique Instance ID##prefabApplyUniqueInstanceID", ref CFG.Current.Prefab_ApplyUniqueInstanceID);
            GUI.Tooltip("Spawned prefab objects will be given unique Instance IDs.");
        }

        if (View.Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            ImGui.Checkbox("Apply Entity Group ID##prefabApplyEntityGroupID", ref CFG.Current.Prefab_ApplySpecificEntityGroupID);

            if (!CFG.Current.Prefab_ApplySpecificEntityGroupID)
                CFG.Current.Prefab_SpecificEntityGroupID = 0;

            GUI.Tooltip("Spawned prefab objects will be given this specific Entity Group ID within an empty Entity Group ID slot.");
        }

        if (CFG.Current.Prefab_ApplyOverrideName)
        {
            GUI.Spacer();
            GUI.SimpleHeader("Override Name", "The name to prepend to the imported map object names.");

            GUI.SinglelineTextInput("OverrideName", ref CFG.Current.Prefab_OverrideName);
        }

        if (CFG.Current.Prefab_ApplySpecificEntityGroupID)
        {
            GUI.Spacer();
            GUI.SimpleHeader("Entity Group ID", "The entity group ID to give to the imported map objects.");

            GUI.IntInput("OverrideEntityGroupID", ref CFG.Current.Prefab_SpecificEntityGroupID);
        }

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("importActions",
            "importPrefab", "Import", "Import a prefab from the prefab list.", ApplyPrefabImportAction,
            "importPrefabFromFile", "Import from File", "Import a prefab from an external prefab JSON file.", ImportPrefabFromFileAction);

        PrefabTree("import");
    }

    public void DisplayExportMenu()
    {
        GUI.WrappedText("Use this to export a set of pre-defined map objects.");

        GUI.Spacer();
        GUI.SimpleHeader("Name", "The name of the prefab to save.");

        GUI.SinglelineTextInput("ExportedPrefabName", ref Prefab_EditName);

        GUI.Spacer();
        GUI.SimpleHeader("Tags", "The tags to associate with the prefab.");

        GUI.SinglelineTextInput("ExportedPrefabTags", ref Prefab_EditFlags);

        GUI.Spacer();
        GUI.SimpleHeader("Options", "");

        ImGui.Checkbox("Retain Entity ID##prefabRetainEntityID", ref CFG.Current.Prefab_IncludeEntityID);
        GUI.Tooltip("Saved objects within a prefab will retain their Entity ID. If false, their Entity ID is set to 0.");

        ImGui.Checkbox("Retain Entity Group IDs##prefabRetainGroupEntityIDs", ref CFG.Current.Prefab_IncludeEntityGroupIDs);
        GUI.Tooltip("Saved objects within a prefab will retain their Entity Group IDs. If false, their Entity Group IDs will be set to 0.");

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("exportPrefabActions",
            "exportPrefab", "Export", "Export the current selection as a prefab.", CreateExportedPrefabAction,
            "deletePrefab", "Delete", "Delete the currently selected prefab in the Prefabs list.", DeleteSelectedPrefab,
            "replacePrefab", "Replace", "Replace the contents currently selected prefab in the Prefabs list with the current selection of map objects.", ReplaceSelectedPrefab);

        PrefabTree("export");
    }

    public void CreateExportedPrefabAction()
    {
        bool selectedEntities = View.ViewportSelection.GetFilteredSelection<MsbEntity>().Any();

        if (!selectedEntities)
        {
            Smithbox.LogError<PrefabTool>("No map objects have been selected.");
            return;
        }

        if (!Prefab_EditName.Any())
        {
            Smithbox.LogError<PrefabTool>("No prefab name has been set.");
            return;
        }

        CreateFromSelection(Prefab_EditName);
    }

    public void DeleteSelectedPrefab()
    {
        if (SelectedPrefab == null)
        {
            Smithbox.LogError<PrefabTool>("No prefab name has been selected.");
            return;
        }

        Delete(SelectedPrefab.PrefabName);
        SelectedPrefab = null;
        Prefab_EditName = "";
        Prefab_EditFlags = "";
    }

    public void ReplaceSelectedPrefab()
    {
        bool selectedEntities = View.ViewportSelection.GetFilteredSelection<MsbEntity>().Any();

        if (!selectedEntities)
        {
            Smithbox.LogError<PrefabTool>("No map objects have been selected.");
            return;
        }

        if (SelectedPrefab == null)
        {
            Smithbox.LogError<PrefabTool>("No prefab name has been selected.");
            return;
        }

        if (!Prefab_EditName.Any())
        {
            Smithbox.LogError<PrefabTool>("No prefab name has been set.");
            return;
        }

        Delete(SelectedPrefab.PrefabName);
        CreateFromSelection(Prefab_EditName);
    }

    public void PrefabTree(string imguiKey)
    {
        GUI.Spacer();
        GUI.SimpleHeader("Prefabs", "");

        if (ImGui.BeginChild($"PrefabEditorTree_{imguiKey}", ImGuiChildFlags.Borders))
        {
            int index = 0;

            foreach (var (name, prefab) in Prefabs)
            {
                bool selected = SelectedPrefab == prefab;
                var flag = ImGuiTreeNodeFlags.OpenOnArrow;
                if (selected)
                    flag |= ImGuiTreeNodeFlags.Selected;

                bool opened = ImGui.TreeNodeEx($"{name}##PrefabTreeNode{index}{imguiKey}", flag);

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
                    {
                        LoadedPrefabs.Remove(name);
                    }
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

    public Prefab GetLoadedPrefab(string name)
    {
        var prefabDir = PrefabUtils.GetPrefabStorageDirectory(View.Project);
        var prefabPath = Path.Join(prefabDir, $"{name}.json");
        var loadedPrefab = LoadedPrefabs.GetValueOrDefault(name);

        if (loadedPrefab is not null)
            return loadedPrefab;

        loadedPrefab = Prefab.New(View);

        if (File.Exists(prefabPath))
        {
            loadedPrefab.ImportJson(prefabPath);
            LoadedPrefabs[name] = loadedPrefab;
        }

        return loadedPrefab;
    }

    public Prefab GetLoadedPrefabFromFile(string path)
    {
        var filename = Path.GetFileNameWithoutExtension(path);
        var loadedPrefab = LoadedPrefabs.GetValueOrDefault(filename);

        if (loadedPrefab is not null)
            return loadedPrefab;

        loadedPrefab = Prefab.New(View);

        if (File.Exists(path))
        {
            loadedPrefab.ImportJson(path);
            LoadedPrefabs[filename] = loadedPrefab;
        }

        return loadedPrefab;
    }

    public void CreateFromSelection(string name)
    {
        var prefabDir = PrefabUtils.GetPrefabStorageDirectory(View.Project);

        if (Prefabs.ContainsKey(name))
        {
            Smithbox.LogError(this, $"Failed to create prefab {name}: prefab already exists with this name.");
            return;
        }
        var newPrefab = Prefab.New(View);

        if (newPrefab == null)
        {
            Smithbox.Log(this, "Prefabs are not supported for this project type.");
        }
        else
        {
            newPrefab.ExportSelection(Path.Join(prefabDir, $"{name}.json"), name, Prefab_EditFlags, View.ViewportSelection);

            Prefabs.Add(name, newPrefab);
            SelectedPrefab = newPrefab;
            LoadedPrefabs.Remove(name);

            RefreshPrefabList();
        }
    }

    public void Delete(string name)
    {
        var prefabDir = PrefabUtils.GetPrefabStorageDirectory(View.Project);

        Prefabs.Remove(name);
        File.Delete(Path.Join(prefabDir, $"{name}.json"));
    }


    public void ApplyPrefabImportAction()
    {
        if(SelectedPrefab is null)
        {
            Smithbox.LogError<PrefabTool>("No prefab has been selected.");
            return;
        }

        if (TargetMap.Item2 is not MapContainer)
        {
            Smithbox.LogError<PrefabTool>("No loaded map has been selected.");
            return;
        }

        string prefixName = null;

        if (CFG.Current.Prefab_ApplyOverrideName)
        {
            prefixName = CFG.Current.Prefab_OverrideName;
        }

        var loadedPrefab = GetLoadedPrefab(SelectedPrefab.PrefabName);

        if (loadedPrefab != null)
        {
            loadedPrefab.ImportToMap(View, TargetMap.Item2 as MapContainer, View.ViewportHandler.ActiveViewport.RenderScene, View.ViewportActionManager, prefixName);
        }
    }

    public void ImportPrefabFromFileAction()
    {
        var dialog = PlatformUtils.Instance.OpenFileDialog("Select JSON", out var path);

        if (dialog && path.EndsWith(".json") && Path.Exists(path))
        {
            var newPrefab = GetLoadedPrefabFromFile(path);

            string prefixName = null;

            if (CFG.Current.Prefab_ApplyOverrideName)
            {
                prefixName = CFG.Current.Prefab_OverrideName;
            }

            if (newPrefab != null)
            {
                newPrefab.ImportToMap(View, TargetMap.Item2 as MapContainer, View.ViewportHandler.ActiveViewport.RenderScene, View.ViewportActionManager, prefixName);
            }
        }

    }

    public void RefreshPrefabList()
    {
        var prefabDir = PrefabUtils.GetPrefabStorageDirectory(View.Project);
        Prefabs = new();

        if (!Directory.Exists(prefabDir))
        {
            try
            {
                Directory.CreateDirectory(prefabDir);
            }
            catch { }
        }

        if (Directory.Exists(prefabDir))
        {
            string[] files = Directory.GetFiles(prefabDir, "*.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var options = new JsonSerializerOptions
                {
                    IncludeFields = true,
                    Converters = { new PrefabAttributesConverter(View) }
                };

                var prefab = JsonSerializer.Deserialize<PrefabAttributes>(File.ReadAllText(file), options);

                if (prefab != null)
                    Prefabs.Add(prefab.PrefabName, prefab);
            }
        }
    }
}