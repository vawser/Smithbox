using ImGuiNET;
using SoulsFormats;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using StudioCore.Configuration;
using StudioCore.Editors.MapEditor.Prefabs;
using StudioCore.Gui;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.UserProject;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using static StudioCore.Editors.MapEditor.Prefabs.ER_AssetPrefab;
using static StudioCore.Editors.MapEditor.Prefabs.ER_AssetPrefab.ER_AssetInfo;

namespace StudioCore.Editors.MapEditor
{

    public class PrefabToolbar
    {
        private readonly EntityActionManager _actionManager;

        private readonly RenderScene _scene;
        private readonly MapSelection _selection;

        private Universe _universe;

        private IViewport _viewport;

        private string _prefabName;
        private string _prefabExt;
        private string _prefabDir;

        private List<PrefabInfo> _prefabInfos;
        private PrefabInfo _selectedPrefabInfo;
        private PrefabInfo _selectedPrefabInfoCache;
        private List<string> _selectedPrefabObjectNames;

        private (string, MapObjectContainer) _comboTargetMap;

        private string _newPrefabName;

        public PrefabToolbar(RenderScene scene, MapSelection sel, EntityActionManager manager, Universe universe, IViewport viewport, (string, MapObjectContainer) comboTargetMap)
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
            _comboTargetMap = comboTargetMap;
        }

        public void OnProjectChanged()
        {
            _prefabDir = $"{Project.GameModDirectory}\\.smithbox\\{Project.GetGameIDForDir()}\\prefabs\\";

            if (!Directory.Exists(_prefabDir))
            {
                Directory.CreateDirectory(_prefabDir);
            }

            RefreshPrefabList();
        }

        public void RefreshPrefabList()
        {
            _prefabInfos = GetPrefabList();
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (Project.Type == ProjectType.Undefined)
                return;

            // Supported Games
            if (Project.Type != ProjectType.ER)
                return;

            MonitorPrefabShortcuts();

            // Window
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Prefabs##PrefabToolbar"))
            {
                ImGui.Text("Prefab Export");
                ImGui.Separator();

                DisplayPrefabSaveMenu();

                ImGui.Separator();
                ImGui.Text("Prefab Import");
                ImGui.Separator();

                DisplayTargetMapMenu();

                ImGui.Separator();

                DisplayPrefabList();

                ImGui.Separator();

                DisplayPrefabActionMenu();

                ImGui.Separator();

                DisplayPrefabContentsList();
            }
            ImGui.End();
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
            ImguiUtils.ShowHelpMarker("Save the current selection as a prefab.");

            if (_selection.GetSelection().Count != 0)
            {
                ImGui.SameLine();
                if (ImGui.Button("Set Name"))
                {
                    _prefabName = GetUniquePrefabName();
                }
                ImguiUtils.ShowHelpMarker("Get an unique prefab name based on the first element of the current selection.");
            }
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
        }

        public void DisplayPrefabList()
        {
            ImGui.Text("Prefabs");
            ImGui.Separator();
            foreach (var info in _prefabInfos)
            {
                var name = info.Name;

                if (ImGui.Selectable($"{name}##{name}", _selectedPrefabInfo == info))
                {
                    _selectedPrefabInfo = info;
                    _newPrefabName = info.Name;
                }
            }
        }

        public void DisplayPrefabActionMenu()
        {
            ImGui.Text("Actions");
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
            }
        }

        public void DisplayPrefabContentsList()
        {
            if (_selectedPrefabInfo != null)
            {
                ImGui.Text("Contents:");
                ImGui.Separator();
                _selectedPrefabObjectNames = GetSelectedPrefabObjects_ER(_selectedPrefabInfo);
                foreach (var name in _selectedPrefabObjectNames)
                {
                    ImGui.Text(name);
                }
            }
        }

        public List<PrefabInfo> GetPrefabList()
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            List<PrefabInfo> infoList = new();

            string[] files = Directory.GetFiles(_prefabDir, "*.json", SearchOption.AllDirectories);
            foreach(var file in files)
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(file);
                PrefabInfo info = new PrefabInfo(name, file);
                infoList.Add(info);
            }

            return infoList;
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
                case ProjectType.ER: ExportSelection_ER(filepath); break;
                default: break;
            }

            RefreshPrefabList();
        }

        /// <summary>
        /// Export selection as Elden Ring prefab.
        /// </summary>
        /// <param name="filepath"></param>
        public void ExportSelection_ER(string filepath)
        {
            ER_AssetPrefab prefab = new(_selection.GetFilteredSelection<MsbEntity>());

            if (!prefab.AssetInfoChildren.Any())
            {
                PlatformUtils.Instance.MessageBox("Export failed, nothing in selection could be exported.", "Prefab Error", MessageBoxButtons.OK);
            }
            else
            {
                prefab.PrefabName = System.IO.Path.GetFileNameWithoutExtension(filepath);
                prefab.Write(filepath);
            }
        }

        /// <summary>
        /// Import selected prefab.
        /// </summary>
        /// <param name="info"></param>
        public void ImportSelectedPrefab(PrefabInfo info)
        {
            switch (Project.Type)
            {
                case ProjectType.ER: ImportSelectedPrefab_ER(info); break;
                default: break;
            }
        }

        /// <summary>
        /// Import selected prefab as Elden Ring map object.
        /// </summary>
        /// <param name="info"></param>
        public void ImportSelectedPrefab_ER(PrefabInfo info)
        {
            ER_AssetPrefab _selectedAssetPrefab_ER;

            _selectedAssetPrefab_ER = ER_AssetPrefab.ImportJson(info.Path);
            Map targetMap = (Map)_comboTargetMap.Item2;

            if (targetMap != null)
            {
                if (_selectedAssetPrefab_ER != null)
                {
                    var parent = targetMap.RootObject;
                    List<MsbEntity> ents = _selectedAssetPrefab_ER.GenerateMapEntities(targetMap);

                    AddMapObjectsAction act = new(_universe, targetMap, _scene, ents, true, parent);
                    _actionManager.ExecuteAction(act);
                    //_comboTargetMap = ("None", null);
                    _selectedAssetPrefab_ER = null;
                }
            }
            else
            {
                PlatformUtils.Instance.MessageBox("Import failed, no map has been selected.", "Prefab Error", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// Get the names of each map object within the prefab
        /// </summary>
        /// <param name="info"></param>
        public List<string> GetSelectedPrefabObjects_ER(PrefabInfo info)
        {
            List<string> entNames = new List<string>();
            ER_AssetPrefab _selectedAssetPrefab_ER;

            _selectedAssetPrefab_ER = ER_AssetPrefab.ImportJson(info.Path);
            Map targetMap = (Map)_comboTargetMap.Item2;

            if (targetMap != null)
            {
                if (_selectedAssetPrefab_ER != null)
                {
                    List<MsbEntity> ents = _selectedAssetPrefab_ER.GenerateMapEntities(targetMap);
                    foreach(var ent in ents) 
                    {
                        string name = BuildModelAliasName(ent);
                        name = PrependEntityType(ent, name);
                        entNames.Add(name);
                    }
                }
            }
            else
            {
                // Fail silently
            }

            return entNames;
        }

        /// <summary>
        /// Get the model alias name for the passed entity
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public string BuildModelAliasName(MsbEntity ent)
        {
            string fullname = "";

            PropertyInfo prop = ent.WrappedObject.GetType().GetProperty("ModelName");
            string modelName = prop.GetValue(ent.WrappedObject) as string;
            fullname = $"{modelName} <>";

            if (ent.WrappedObject is MSBE.Part.MapPiece mapPiece)
            {
                foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("MapPieces"))
                {
                    if (modelName == entry.id)
                    {
                        fullname = $"{modelName} <{entry.name}>";
                    }
                }
            }

            if (ent.WrappedObject is MSBE.Part.Enemy enemy || ent.WrappedObject is MSBE.Part.DummyEnemy dummyEnemy)
            {
                foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("Characters"))
                {
                    if (modelName == entry.id)
                    {
                        fullname = $"{modelName} <{entry.name}>";
                    }
                }
            }

            if (ent.WrappedObject is MSBE.Part.Asset asset || ent.WrappedObject is MSBE.Part.DummyAsset dummyAsset)
            {
                foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("Objects"))
                {
                    if (modelName == entry.id)
                    {
                        fullname = $"{modelName} <{entry.name}>";
                    }
                }
            }

            return fullname;
        }

        public string PrependEntityType(MsbEntity ent, string existingName)
        {
            string name = existingName;

            // Parts
            if(ent.WrappedObject is MSBE.Part.MapPiece mapPiece)
            {
                name = $"Map Piece: {name}";
            }
            if (ent.WrappedObject is MSBE.Part.Enemy enemy)
            {
                name = $"Enemy: {name}";
            }
            if (ent.WrappedObject is MSBE.Part.Player player)
            {
                name = $"Player: {name}";
            }
            if (ent.WrappedObject is MSBE.Part.Collision col)
            {
                name = $"Collision: {name}";
            }
            if (ent.WrappedObject is MSBE.Part.DummyAsset dummyAsset)
            {
                name = $"Dummy Asset: {name}";
            }
            if (ent.WrappedObject is MSBE.Part.DummyEnemy dummyEnemy)
            {
                name = $"Dummy Enemy: {name}";
            }
            if (ent.WrappedObject is MSBE.Part.ConnectCollision connectCol)
            {
                name = $"Connect Collision: {name}";
            }
            if (ent.WrappedObject is MSBE.Part.Asset asset)
            {
                name = $"Asset: {name}";
            }

            // Regions
            if (ent.WrappedObject is MSBE.Region.Other region)
            {
                name = $"Region: {name}";
            }

            return name;
        }
    }

    public class PrefabInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public PrefabInfo(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}
