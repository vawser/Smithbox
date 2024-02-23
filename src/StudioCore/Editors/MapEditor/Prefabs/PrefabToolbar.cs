using ImGuiNET;
using StudioCore.Banks.AliasBank;
using StudioCore.Configuration;
using StudioCore.Editors.MapEditor.Prefabs;
using StudioCore.Gui;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace StudioCore.Editors.MapEditor
{

    public class PrefabToolbar
    {
        private readonly ViewportActionManager _actionManager;

        private readonly RenderScene _scene;
        private readonly ViewportSelection _selection;

        private Universe _universe;

        private IViewport _viewport;

        private string _prefabName;
        private string _prefabExt;
        private string _prefabDir;

        private List<PrefabInfo> _prefabInfos;
        private PrefabInfo _selectedPrefabInfo;
        private PrefabInfo _selectedPrefabInfoCache;
        private List<string> _selectedPrefabObjectNames;

        private (string, ObjectContainer) _comboTargetMap;

        private string _newPrefabName;
        private string _prefabTags;

        private string _searchInput = "";
        private string _searchInputCache = "";

        public PrefabToolbar(RenderScene scene, ViewportSelection sel, ViewportActionManager manager, Universe universe, IViewport viewport, (string, ObjectContainer) comboTargetMap)
        {
            _scene = scene;
            _selection = sel;
            _actionManager = manager;
            _universe = universe;

            _viewport = viewport;

            _prefabName = "";
            _prefabExt = ".json";
            _prefabDir = "";
            _newPrefabName = "";
            _prefabTags = "";
            _comboTargetMap = comboTargetMap;
        }

        public void OnProjectChanged()
        {
            _selectedPrefabObjectNames = new List<string>();
            _prefabInfos = new List<PrefabInfo>();
            _selectedPrefabInfo = null;
            _selectedPrefabInfoCache = null;
            _comboTargetMap = ("", null);
            _newPrefabName = "";
            _prefabTags = "";

            _prefabDir = $"{Project.GameModDirectory}\\.smithbox\\{Project.GetGameIDForDir()}\\prefabs\\";

            if (!Directory.Exists(_prefabDir))
            {
                Directory.CreateDirectory(_prefabDir);
            }

            RefreshPrefabList();
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (Project.Type == ProjectType.Undefined)
                return;

            // Supported Games
            if (!(Project.Type is ProjectType.ER or ProjectType.DS3 or ProjectType.SDT or ProjectType.DS2S or ProjectType.DS1 or ProjectType.DS1R))
                return;

            MonitorPrefabShortcuts();

            // Window
            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Prefabs##PrefabToolbar"))
            {
                DisplayPrefabActionMenu();

                ImGui.Separator();
                ImGui.Text("Prefab Export");
                ImguiUtils.ShowHelpMarker($"Shortcut: {KeyBindings.Current.Toolbar_ExportPrefab.HintText}");
                ImGui.Separator();

                DisplayPrefabSaveMenu();

                ImGui.Separator();
                ImGui.Text("Prefab Import");
                ImguiUtils.ShowHelpMarker($"Shortcut: {KeyBindings.Current.Toolbar_ImportPrefab.HintText}");
                ImGui.Separator();

                DisplayTargetMapMenu();
                DisplayPrefabSearch();

                ImGui.Columns(2);
                DisplayPrefabList();

                ImGui.NextColumn();

                DisplayPrefabContentsList();

                ImGui.Columns(1);
            }
            ImGui.End();
            ImGui.PopStyleColor(1);
        }

        public void MonitorPrefabShortcuts()
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_ExportPrefab))
            {
                var name = GetUniquePrefabName();
                ExportCurrentSelection(name);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_ImportPrefab))
            {
                if (_selectedPrefabInfo != null)
                {
                    ImportSelectedPrefab(_selectedPrefabInfo);
                }
                else
                {
                    PlatformUtils.Instance.MessageBox("No prefab has been selected to import.", "Prefab Error", MessageBoxButtons.OK);
                }
            }
        }

        public void DisplayPrefabActionMenu()
        {
            ImGui.Text("Actions");
            ImguiUtils.ShowHelpMarker($"The actions we can apply with this prefab.");
            ImGui.Separator();

            if (_selectedPrefabInfo != null)
            {
                if (ImGui.Button("Spawn Prefab"))
                {
                    ImportSelectedPrefab(_selectedPrefabInfo);
                }
                ImguiUtils.ShowHelpMarker("Spawn the prefab objects into the current target map.");

                if (ImGui.Button("Delete Prefab"))
                {
                    var filepath = $"{_selectedPrefabInfo.Path}";

                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }

                    _selectedPrefabInfo = null;
                    RefreshPrefabList();
                }
                ImguiUtils.ShowHelpMarker("Delete this prefab.");

                if (ImGui.Button("Rename Prefab"))
                {
                    var filepath = $"{_selectedPrefabInfo.Path}";
                    var name = Path.GetFileNameWithoutExtension(filepath);

                    var newFilePath = filepath.Replace(name, _newPrefabName);

                    File.Move(filepath, newFilePath);

                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }

                    _selectedPrefabInfo = null;
                    RefreshPrefabList();
                }
                ImGui.SameLine();
                ImGui.InputText("##prefabRename", ref _newPrefabName, 255);
                ImguiUtils.ShowHelpMarker("Rename this prefab.");

                // Options
                ImGui.Checkbox("Apply Unique Entity ID", ref CFG.Current.Prefab_ApplyUniqueEntityID);
                ImguiUtils.ShowHelpMarker("Spawned prefab objects will be given unique Entity IDs.");

                if (Project.Type == ProjectType.DS3 || Project.Type == ProjectType.SDT || Project.Type == ProjectType.ER)
                {
                    ImGui.Checkbox("Apply Entity Group ID", ref CFG.Current.Prefab_ApplySpecificEntityGroupID);
                    ImGui.SameLine();
                    ImGui.InputInt("##entityGroupIdInput", ref CFG.Current.Prefab_SpecificEntityGroupID);
                    ImguiUtils.ShowHelpMarker("Spawned prefab objects will be given this specific Entity Group ID within an empty Entity Group ID slot.");
                }

                if (Project.Type == ProjectType.ER)
                {
                    ImGui.Checkbox("Apply Unique Instance ID", ref CFG.Current.Prefab_ApplyUniqueInstanceID);
                    ImguiUtils.ShowHelpMarker("Spawned prefab objects will be given unique Entity IDs.");

                    ImGui.Checkbox("Apply Asset UnkPartNames", ref CFG.Current.Prefab_ApplySelfPartNames);
                    ImguiUtils.ShowHelpMarker("Spawned prefab objects that are Assets will be given UnkPartNames matching themselves.");
                }
            }
        }

        public void DisplayPrefabSaveMenu()
        {
            ImGui.InputText("##prefabName", ref _prefabName, 255);
            ImGui.SameLine();

            if (ImGui.Button("Save"))
            {
                if (File.Exists($"{_prefabDir}{_prefabName}{_prefabExt}"))
                {
                    PlatformUtils.Instance.MessageBox("Prefab already exists with this name, try another.", "Prefab Error", MessageBoxButtons.OK);
                }
                else if (_prefabName == "" || _prefabName == null)
                {
                    PlatformUtils.Instance.MessageBox("Prefab name cannot be blank.", "Prefab Error", MessageBoxButtons.OK);
                }
                else
                {
                    ExportCurrentSelection(_prefabName);
                }
            }
            ImguiUtils.ShowHelpMarker("Save the current selection as a prefab.\n\nNote, map object fields that refer other map objects will be set to empty when saved as a prefab.");

            if (_selection.GetSelection().Count != 0)
            {
                ImGui.SameLine();
                if (ImGui.Button("Set Name"))
                {
                    _prefabName = GetUniquePrefabName();
                }
                ImguiUtils.ShowHelpMarker("Get an unique prefab name based on the first element of the current selection.");
            }

            ImGui.InputText("Tags##prefabTags", ref _prefabTags, 255);
            ImguiUtils.ShowHelpMarker("The set of tags to save this prefab under. Split each tag with the , character.");

            ImGui.Checkbox("Retain Entity ID", ref CFG.Current.Prefab_IncludeEntityID);
            ImguiUtils.ShowHelpMarker("Saved objects within a prefab will retain their Entity ID. If false, their Entity ID is set to 0.");

            ImGui.Checkbox("Retain Entity Group IDs", ref CFG.Current.Prefab_IncludeEntityGroupIDs);
            ImguiUtils.ShowHelpMarker("Saved objects within a prefab will retain their Entity Group IDs. If false, their Entity Group IDs will be set to 0.");
        }

        public void DisplayTargetMapMenu()
        {
            if (ImGui.BeginCombo("Targeted Map", _comboTargetMap.Item1))
            {
                foreach (var obj in _universe.LoadedObjectContainers)
                {
                    if (obj.Value != null)
                    {
                        if (ImGui.Selectable(obj.Key))
                        {
                            _comboTargetMap = (obj.Key, obj.Value);
                            break;
                        }
                    }
                }
                ImGui.EndCombo();
            }
            ImguiUtils.ShowHelpMarker("The target map to spawn a prefab in.");
        }

        public void DisplayPrefabSearch()
        {
            ImGui.InputText($"Search", ref _searchInput, 255);

            if (_searchInput != _searchInputCache)
            {
                _searchInputCache = _searchInput;
            }
        }

        public void DisplayPrefabList()
        {
            ImGui.BeginChild("##PrefabSelectionList");

            ImGui.Separator();
            ImGui.Text("Available Prefabs");
            ImGui.Separator();

            foreach (var info in _prefabInfos)
            {
                var name = info.Name;

                if (SearchFilters.IsSearchMatch(_searchInput, name, name, info.Tags, false, false, true, "_"))
                {
                    if (ImGui.Selectable($"{name}##{name}", _selectedPrefabInfo == info))
                    {
                        _selectedPrefabInfo = info;
                        _newPrefabName = info.Name;
                    }
                }
            }

            ImGui.EndChild();
        }

        public void DisplayPrefabContentsList()
        {
            ImGui.BeginChild("##PrefabContentList");
            if (_selectedPrefabInfo != null)
            {
                switch (Project.Type)
                {
                    case ProjectType.ER:
                        _selectedPrefabObjectNames = Prefab_ER.GetSelectedPrefabObjects(_selectedPrefabInfo, _comboTargetMap);
                        break;
                    case ProjectType.SDT:
                        _selectedPrefabObjectNames = Prefab_SDT.GetSelectedPrefabObjects(_selectedPrefabInfo, _comboTargetMap);
                        break;
                    case ProjectType.DS3:
                        _selectedPrefabObjectNames = Prefab_DS3.GetSelectedPrefabObjects(_selectedPrefabInfo, _comboTargetMap);
                        break;
                    case ProjectType.DS2S:
                        _selectedPrefabObjectNames = Prefab_DS2.GetSelectedPrefabObjects(_selectedPrefabInfo, _comboTargetMap);
                        break;
                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        _selectedPrefabObjectNames = Prefab_DS1.GetSelectedPrefabObjects(_selectedPrefabInfo, _comboTargetMap);
                        break;
                    default: break;
                }

                // Tags
                ImGui.Text("Tags:");

                if(_selectedPrefabInfo.Tags != null)
                {
                    foreach(var tag in _selectedPrefabInfo.Tags)
                    {
                        ImGui.Text(tag);
                    }
                }

                ImGui.Text("");

                // Contents
                ImGui.Text("Contents:");
                if (_selectedPrefabObjectNames != null)
                {
                    foreach (var name in _selectedPrefabObjectNames)
                    {
                        ImGui.Text(name);
                    }
                }
            }

            ImGui.EndChild();
        }
        public void RefreshPrefabList()
        {
            _prefabInfos = GetPrefabList();
        }

        public List<PrefabInfo> GetPrefabList()
        {
            List<PrefabInfo> infoList = new();

            string[] files = Directory.GetFiles(_prefabDir, "*.json", SearchOption.AllDirectories);
            foreach(var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                PrefabInfo info = new PrefabInfo(name, file, GetPrefabTags(file));
                infoList.Add(info);
            }

            return infoList;
        }

        public List<string> GetPrefabTags(string filepath)
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            List<string> tags = new List<string>();

            switch (Project.Type)
            {
                case ProjectType.ER:
                    var prefab_ER = new Prefab_ER();
                    using (var stream = File.OpenRead(filepath))
                        prefab_ER = JsonSerializer.Deserialize<Prefab_ER>(File.OpenRead(filepath), options);

                    tags = prefab_ER.TagList;
                    break;
                case ProjectType.SDT:
                    var prefab_SDT = new Prefab_SDT();
                    using (var stream = File.OpenRead(filepath))
                        prefab_SDT = JsonSerializer.Deserialize<Prefab_SDT>(File.OpenRead(filepath), options);

                    tags = prefab_SDT.TagList;
                    break;
                case ProjectType.DS3:
                    var prefab_DS3 = new Prefab_DS3();
                    using (var stream = File.OpenRead(filepath))
                        prefab_DS3 = JsonSerializer.Deserialize<Prefab_DS3>(File.OpenRead(filepath), options);

                    tags = prefab_DS3.TagList;
                    break;
                case ProjectType.DS2S:
                    var prefab_DS2 = new Prefab_DS2();
                    using (var stream = File.OpenRead(filepath))
                        prefab_DS2 = JsonSerializer.Deserialize<Prefab_DS2>(File.OpenRead(filepath), options);

                    tags = prefab_DS2.TagList;
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    var prefab_DS1 = new Prefab_DS1();
                    using (var stream = File.OpenRead(filepath))
                        prefab_DS1 = JsonSerializer.Deserialize<Prefab_DS1>(File.OpenRead(filepath), options);

                    tags = prefab_DS1.TagList;
                    break;
                default: break;
            }

            return tags;
        }

        public string GetUniquePrefabName()
        {
            var ent = _selection.GetSelection().First() as Entity;
            int count = 1000;
            _prefabName = $"{ent.Name}_{count}"; // Use First entity's name as prefab name

            // Loop until we reach a filename that isn't used
            while (File.Exists($"{_prefabDir}{_prefabName}{_prefabExt}"))
            {
                count++;
                _prefabName = $"{ent.Name}_{count}";
            }

            return _prefabName;
        }

        // <summary>
        /// Export current selection as prefab.
        /// </summary>
        /// <param name="filepath"></param>
        public void ExportCurrentSelection(string name)
        {
            var filepath = $"{_prefabDir}{name}{_prefabExt}";

            switch(Project.Type)
            {
                case ProjectType.ER: 
                    Prefab_ER.ExportSelection(filepath, _selection, _prefabTags);
                    break;
                case ProjectType.SDT:
                    Prefab_SDT.ExportSelection(filepath, _selection, _prefabTags);
                    break;
                case ProjectType.DS3:
                    Prefab_DS3.ExportSelection(filepath, _selection, _prefabTags);
                    break;
                case ProjectType.DS2S:
                    Prefab_DS2.ExportSelection(filepath, _selection, _prefabTags);
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    Prefab_DS1.ExportSelection(filepath, _selection, _prefabTags);
                    break;
                default: break;
            }

            RefreshPrefabList();
        }

        /// <summary>
        /// Import selected prefab.
        /// </summary>
        /// <param name="info"></param>
        public void ImportSelectedPrefab(PrefabInfo info)
        {
            switch (Project.Type)
            {
                case ProjectType.ER: 
                    Prefab_ER.ImportSelectedPrefab(info, _comboTargetMap, _universe, _scene, _actionManager); 
                    break;
                case ProjectType.SDT:
                    Prefab_SDT.ImportSelectedPrefab(info, _comboTargetMap, _universe, _scene, _actionManager);
                    break;
                case ProjectType.DS3:
                    Prefab_DS3.ImportSelectedPrefab(info, _comboTargetMap, _universe, _scene, _actionManager);
                    break;
                case ProjectType.DS2S:
                    Prefab_DS2.ImportSelectedPrefab(info, _comboTargetMap, _universe, _scene, _actionManager);
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    Prefab_DS1.ImportSelectedPrefab(info, _comboTargetMap, _universe, _scene, _actionManager);
                    break;
                default: break;
            }
        }
    }

    public class PrefabInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<string> Tags { get; set; }

        public PrefabInfo(string name, string path, List<string> tags)
        {
            Name = name;
            Path = path;
            Tags = tags;
        }
    }
}
