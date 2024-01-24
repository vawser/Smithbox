using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using StudioCore.Aliases;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Gui;
using StudioCore.ParamEditor;
using StudioCore.Scene;
using StudioCore.Settings;
using StudioCore.Utilities;
using Veldrid;
using System.IO;
using System;
using System.Threading;
using static SoulsFormats.ACB;
using SoulsFormats;
using StudioCore.Platform;

namespace StudioCore.MsbEditor
{
    public class MsbAssetBrowser
    {
        private readonly ActionManager _actionManager;

        private readonly RenderScene _scene;
        private readonly Selection _selection;

        private AssetLocator _assetLocator;
        private MsbEditorScreen _msbEditor;

        private List<string> _loadedMaps = new List<string>();
        private List<string> _modelNameCache = new List<string>();
        private Dictionary<string, List<string>> _mapModelNameCache = new Dictionary<string, List<string>>();

        private string _selectedAssetType = null;
        private string _selectedAssetTypeCache = null;

        private string _selectedAssetMapId = "";
        private string _selectedAssetMapIdCache = null;

        private string _searchInput = "";
        private string _searchInputCache = "";

        private string _refUpdateId = "";
        private string _refUpdateName = "";
        private string _refUpdateTags = "";

        private IViewport _viewport;

        private string _selectedName;

        private bool updateScrollPosition = false;
        private float _currentScrollY;

        private AliasBank _modelAliasBank;
        private AliasBank _mapAliasBank;

        private Universe _universe;

        public MsbAssetBrowser(Universe universe, RenderScene scene, Selection sel, ActionManager manager, AssetLocator locator, MsbEditorScreen editor, IViewport viewport, AliasBank modelAliasBank, AliasBank mapAliasBank)
        {
            _scene = scene;
            _selection = sel;
            _actionManager = manager;
            _universe = universe;

            _assetLocator = locator;
            _msbEditor = editor;
            _viewport = viewport;

            _modelAliasBank = modelAliasBank;
            _mapAliasBank = mapAliasBank;

            _selectedName = null;
        }

        /// <summary>
        /// Display the Asset Browser window.
        /// </summary>
        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (_assetLocator.Type == GameType.Undefined)
                return;

            if (_modelAliasBank.IsLoadingAliases)
                return;

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Asset Browser##MsbAssetBrowser"))
            {
                ImGui.Columns(2);

                // Asset Type List
                ImGui.BeginChild("AssetTypeList");

                DisplayAssetTypeSelectionList();

                ImGui.EndChild();

                // Asset List
                ImGui.NextColumn();

                ImGui.BeginChild("AssetListSearch");
                ImGui.InputText($"Search", ref _searchInput, 255);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.BeginChild("AssetList");

                DisplayAssetSelectionList("Chr", _modelAliasBank.AliasNames.GetEntries("Characters"));
                DisplayAssetSelectionList("Obj", _modelAliasBank.AliasNames.GetEntries("Objects"));
                DisplayMapAssetSelectionList("MapPiece", _modelAliasBank.AliasNames.GetEntries("MapPieces"));

                ImGui.EndChild();
                ImGui.EndChild();
            }
            ImGui.End();

            if (_modelAliasBank.mayReloadAliasBank)
            {
                _modelAliasBank.mayReloadAliasBank = false;
                _modelAliasBank.ReloadAliasBank();
            }
        }

        /// <summary>
        /// Display the asset category type selection list: Chr, Obj/AEG, Part and each map id for Map Pieces.
        /// </summary>
        private void DisplayAssetTypeSelectionList()
        {
            var objLabel = "Obj";

            if (_assetLocator.Type is GameType.EldenRing or GameType.ArmoredCoreVI)
                objLabel = "AEG";

            if (ImGui.Selectable("Chr", _selectedAssetType == "Chr"))
            {
                _modelNameCache = _assetLocator.GetChrModels();
                _selectedAssetType = "Chr";
                _selectedAssetMapId = "";
            }
            if (ImGui.Selectable(objLabel, _selectedAssetType == "Obj"))
            {
                _modelNameCache = _assetLocator.GetObjModels();
                _selectedAssetType = "Obj";
                _selectedAssetMapId = "";
            }

            _loadedMaps.Clear();

            // Map-specific MapPieces
            foreach (var mapId in _mapModelNameCache.Keys)
            {
                foreach (var obj in _msbEditor.Universe.LoadedObjectContainers)
                    if (obj.Value != null)
                        _loadedMaps.Add(obj.Key);

                if (_loadedMaps.Contains(mapId))
                {
                    var labelName = mapId;

                    if (_mapAliasBank.MapNames != null)
                    {
                        if (_mapAliasBank.MapNames.ContainsKey(mapId))
                            labelName = labelName + $" <{_mapAliasBank.MapNames[mapId]}>";
                    }

                    if (ImGui.Selectable(labelName, _selectedAssetMapId == mapId))
                    {
                        if (_mapModelNameCache[mapId] == null)
                        {
                            List<AssetDescription> modelList = _assetLocator.GetMapModels(mapId);
                            var cache = new List<string>();

                            foreach (AssetDescription model in modelList)
                                cache.Add(model.AssetName);
                            _mapModelNameCache[mapId] = cache;
                        }

                        _selectedAssetMapId = mapId;
                        _selectedAssetType = "MapPiece";
                    }
                }
            }
        }

        /// <summary>
        /// Display the asset selection list for Chr, Obj/AEG and Parts.
        /// </summary>
        private void DisplayAssetSelectionList(string assetType, List<AliasReference> referenceList)
        {
            if (updateScrollPosition)
            {
                updateScrollPosition = false;
                ImGui.SetScrollY(_currentScrollY);
            }

            Dictionary<string, AliasReference> referenceDict = new Dictionary<string, AliasReference>();

            foreach (AliasReference v in referenceList)
            {
                if(!referenceDict.ContainsKey(v.id))
                    referenceDict.Add(v.id, v);
            }

            if (_selectedAssetType == assetType)
            {
                if (_searchInput != _searchInputCache || _selectedAssetType != _selectedAssetTypeCache)
                {
                    _searchInputCache = _searchInput;
                    _selectedAssetTypeCache = _selectedAssetType;
                }

                foreach (var name in _modelNameCache)
                {
                    var displayedName = $"{name}";
                    var lowerName = name.ToLower();

                    var refID = $"{name}";
                    var refName = "";
                    var refTagList = new List<string>();

                    // Alias contains name
                    if (referenceDict.ContainsKey(lowerName))
                    {
                        displayedName = displayedName + $" <{referenceDict[lowerName].name}>";

                        // Append tags to to displayed name
                        if (CFG.Current.AssetBrowser_ShowTagsInBrowser)
                        {
                            var tagString = string.Join(" ", referenceDict[lowerName].tags);
                            displayedName = $"{displayedName} {{ {tagString} }}";
                        }

                        refID = referenceDict[lowerName].id;
                        refName = referenceDict[lowerName].name;
                        refTagList = referenceDict[lowerName].tags;
                    }

                    if (Utils.IsAssetSearchFilterMatch(_searchInput, lowerName, refName, refTagList))
                    {
                        if (ImGui.Selectable(displayedName))
                        {
                            _currentScrollY = ImGui.GetScrollY();

                            _selectedName = refID;

                            _refUpdateId = refID;
                            _refUpdateName = refName;

                            if (refTagList.Count > 0)
                            {
                                string tagStr = refTagList[0];
                                foreach (string entry in refTagList.Skip(1))
                                {
                                    tagStr = $"{tagStr},{entry}";
                                }
                                _refUpdateTags = tagStr;
                            }
                            else
                            {
                                _refUpdateTags = "";
                            }
                        }

                        if (_selectedName == refID)
                        {
                            if (ImGui.BeginPopupContextItem($"{refID}##context"))
                            {
                                if (ImGui.InputText($"Name", ref _refUpdateName, 255))
                                {

                                }

                                if (ImGui.InputText($"Tags", ref _refUpdateTags, 255))
                                {

                                }

                                if (ImGui.Button("Update"))
                                {
                                    _modelAliasBank.AddToLocalAliasBank(assetType, _refUpdateId, _refUpdateName, _refUpdateTags);
                                    ImGui.CloseCurrentPopup();
                                    _modelAliasBank.mayReloadAliasBank = true;
                                }

                                ImGui.SameLine();
                                if (ImGui.Button("Restore Default"))
                                {
                                    _modelAliasBank.RemoveFromLocalAliasBank(assetType, _refUpdateId);
                                    ImGui.CloseCurrentPopup();
                                    _modelAliasBank.mayReloadAliasBank = true;
                                }

                                ImGui.EndPopup();
                            }
                        }

                        if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                        {
                            var modelName = name;

                            if (modelName.Contains("aeg"))
                                modelName = modelName.Replace("aeg", "AEG");

                            SetObjectModelForSelection(modelName, assetType, "");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Display the asset selection list for Map Pieces.
        /// </summary>
        private void DisplayMapAssetSelectionList(string assetType, List<AliasReference> referenceList)
        {
            Dictionary<string, AliasReference> referenceDict = new Dictionary<string, AliasReference>();

            foreach (AliasReference v in referenceList)
            {
                if (!referenceDict.ContainsKey(v.id))
                    referenceDict.Add(v.id, v);
            }

            if (_selectedAssetType == assetType)
            {
                if (_mapModelNameCache.ContainsKey(_selectedAssetMapId))
                {
                    if (_searchInput != _searchInputCache || _selectedAssetType != _selectedAssetTypeCache || _selectedAssetMapId != _selectedAssetMapIdCache)
                    {
                        _searchInputCache = _searchInput;
                        _selectedAssetTypeCache = _selectedAssetType;
                        _selectedAssetMapIdCache = _selectedAssetMapId;
                    }
                    foreach (var name in _mapModelNameCache[_selectedAssetMapId])
                    {
                        var modelName = name.Replace($"{_selectedAssetMapId}_", "m");

                        var displayedName = $"{modelName}";
                        var lowerName = name.ToLower();

                        var refID = $"{name}";
                        var refName = "";
                        var refTagList = new List<string>();

                        // Adjust the name to remove the A{mapId} section.
                        if (_assetLocator.Type == GameType.DarkSoulsPTDE || _assetLocator.Type == GameType.DarkSoulsRemastered)
                        {
                            displayedName = displayedName.Replace($"A{_selectedAssetMapId.Substring(1, 2)}", "");
                        }

                        if (referenceDict.ContainsKey(lowerName))
                        {
                            displayedName = displayedName + $" <{referenceDict[lowerName].name}>";

                            if (CFG.Current.AssetBrowser_ShowTagsInBrowser)
                            {
                                var tagString = string.Join(" ", referenceDict[lowerName].tags);
                                displayedName = $"{displayedName} {{ {tagString} }}";
                            }

                            refID = referenceDict[lowerName].id;
                            refName = referenceDict[lowerName].name;
                            refTagList = referenceDict[lowerName].tags;
                        }

                        if (Utils.IsAssetSearchFilterMatch(_searchInput, lowerName, refName, refTagList))
                        {
                            if (ImGui.Selectable(displayedName))
                            {
                                _selectedName = refID;

                                _refUpdateId = refID;
                                _refUpdateName = refName;

                                if (refTagList.Count > 0)
                                {
                                    string tagStr = refTagList[0];
                                    foreach (string entry in refTagList.Skip(1))
                                    {
                                        tagStr = $"{tagStr},{entry}";
                                    }
                                    _refUpdateTags = tagStr;
                                }
                                else
                                {
                                    _refUpdateTags = "";
                                }
                            }

                            if (_selectedName == refID)
                            {
                                if (ImGui.BeginPopupContextItem($"{refID}##context"))
                                {
                                    if (ImGui.InputText($"Name", ref _refUpdateName, 255))
                                    {

                                    }

                                    if (ImGui.InputText($"Tags", ref _refUpdateTags, 255))
                                    {

                                    }

                                    if (ImGui.Button("Update"))
                                    {
                                        _modelAliasBank.AddToLocalAliasBank(assetType, _refUpdateId, _refUpdateName, _refUpdateTags);
                                        ImGui.CloseCurrentPopup();
                                        _modelAliasBank.mayReloadAliasBank = true;
                                    }

                                    ImGui.SameLine();
                                    if (ImGui.Button("Restore Default"))
                                    {
                                        _modelAliasBank.RemoveFromLocalAliasBank(assetType, _refUpdateId);
                                        ImGui.CloseCurrentPopup();
                                        _modelAliasBank.mayReloadAliasBank = true;
                                    }

                                    ImGui.EndPopup();
                                }
                            }

                            if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                            {
                                SetObjectModelForSelection(modelName, assetType, _selectedAssetMapId);
                            }
                        }
                    }
                }
            }
        }

        public void SetObjectModelForSelection(string modelName, string assetType, string assetMapId)
        {
            var actlist = new List<Action>();

            var selected = _selection.GetFilteredSelection<Entity>();

            foreach (var s in selected)
            {
                bool isValidObjectType = false;

                if (assetType == "Chr")
                {
                    switch (_assetLocator.Type)
                    {
                        case GameType.DemonsSouls:
                            if (s.WrappedObject is MSBD.Part.Enemy)
                                isValidObjectType = true;
                            break;
                        case GameType.DarkSoulsPTDE:
                        case GameType.DarkSoulsRemastered:
                            if (s.WrappedObject is MSB1.Part.Enemy)
                                isValidObjectType = true;
                            break;
                        case GameType.DarkSoulsIISOTFS:
                            break;
                        case GameType.DarkSoulsIII:
                            if (s.WrappedObject is MSB3.Part.Enemy)
                                isValidObjectType = true;
                            break;
                        case GameType.Bloodborne:
                            if (s.WrappedObject is MSBB.Part.Enemy)
                                isValidObjectType = true;
                            break;
                        case GameType.Sekiro:
                            if (s.WrappedObject is MSBS.Part.Enemy)
                                isValidObjectType = true;
                            break;
                        case GameType.EldenRing:
                            if (s.WrappedObject is MSBE.Part.Enemy)
                                isValidObjectType = true;
                            break;
                        case GameType.ArmoredCoreVI:
                            if (s.WrappedObject is MSB_AC6.Part.Enemy)
                                isValidObjectType = true;
                            break;
                        default:
                            throw new ArgumentException("Selected entity type must be Enemy");
                    }
                }
                if (assetType == "Obj")
                {
                    switch (_assetLocator.Type)
                    {
                        case GameType.DemonsSouls:
                            if (s.WrappedObject is MSBD.Part.Object)
                                isValidObjectType = true;
                            break;
                        case GameType.DarkSoulsPTDE:
                        case GameType.DarkSoulsRemastered:
                            if (s.WrappedObject is MSB1.Part.Object)
                                isValidObjectType = true;
                            break;
                        case GameType.DarkSoulsIISOTFS:
                            if (s.WrappedObject is MSB2.Part.Object)
                                isValidObjectType = true;
                            break;
                        case GameType.DarkSoulsIII:
                            if (s.WrappedObject is MSB3.Part.Object)
                                isValidObjectType = true;
                            break;
                        case GameType.Bloodborne:
                            if (s.WrappedObject is MSBB.Part.Object)
                                isValidObjectType = true;
                            break;
                        case GameType.Sekiro:
                            if (s.WrappedObject is MSBS.Part.Object)
                                isValidObjectType = true;
                            break;
                        case GameType.EldenRing:
                            if (s.WrappedObject is MSBE.Part.Asset)
                                isValidObjectType = true;
                            break;
                        case GameType.ArmoredCoreVI:
                            if (s.WrappedObject is MSB_AC6.Part.Asset)
                                isValidObjectType = true;
                            break;
                        default:
                            throw new ArgumentException("Selected entity type must be Object/Asset");
                    }
                }
                if (assetType == "MapPiece")
                {
                    switch (_assetLocator.Type)
                    {
                        case GameType.DemonsSouls:
                            if (s.WrappedObject is MSBD.Part.MapPiece)
                                isValidObjectType = true;
                            break;
                        case GameType.DarkSoulsPTDE:
                        case GameType.DarkSoulsRemastered:
                            if (s.WrappedObject is MSB1.Part.MapPiece)
                                isValidObjectType = true;
                            break;
                        case GameType.DarkSoulsIISOTFS:
                            if (s.WrappedObject is MSB2.Part.MapPiece)
                                isValidObjectType = true;
                            break;
                        case GameType.DarkSoulsIII:
                            if (s.WrappedObject is MSB3.Part.MapPiece)
                                isValidObjectType = true;
                            break;
                        case GameType.Bloodborne:
                            if (s.WrappedObject is MSBB.Part.MapPiece)
                                isValidObjectType = true;
                            break;
                        case GameType.Sekiro:
                            if (s.WrappedObject is MSBS.Part.MapPiece)
                                isValidObjectType = true;
                            break;
                        case GameType.EldenRing:
                            if (s.WrappedObject is MSBE.Part.MapPiece)
                                isValidObjectType = true;
                            break;
                        case GameType.ArmoredCoreVI:
                            if (s.WrappedObject is MSB_AC6.Part.MapPiece)
                                isValidObjectType = true;
                            break;
                        default:
                            throw new ArgumentException("Selected entity type must be MapPiece");
                    }
                }

                if (assetType == "MapPiece")
                {
                    string mapName = s.Parent.Name;
                    if (mapName != assetMapId)
                    {
                        PlatformUtils.Instance.MessageBox($"Map Pieces are specific to each map.\nYou cannot change a Map Piece in {mapName} to a Map Piece from {assetMapId}.", "Object Browser", MessageBoxButtons.OK);

                        isValidObjectType = false;
                    }
                }

                if (isValidObjectType)
                {
                    // ModelName
                    actlist.Add(s.ChangeObjectProperty("ModelName", modelName));

                    // Name
                    string name = GetUniqueNameString(modelName);
                    s.Name = name;
                    actlist.Add(s.ChangeObjectProperty("Name", name));

                    // Name
                    if (s.WrappedObject is MSBE.Part)
                    {
                        SetUniqueInstanceID((MapEntity)s, modelName);
                    }
                }
            }

            if (actlist.Any())
            {
                var action = new CompoundAction(actlist);
                _actionManager.ExecuteAction(action);
            }
        }

        public string GetUniqueNameString(string modelName)
        {
            int postfix = 0;
            string baseName = $"{modelName}_0000";

            List<string> names = new List<string>();

            // Collect names
            foreach (var o in _universe.LoadedObjectContainers.Values)
            {
                if (o == null)
                {
                    continue;
                }
                if (o is Map m)
                {
                    foreach (var ob in m.Objects)
                    {
                        if (ob is MapEntity e)
                        {
                            names.Add(ob.Name);
                        }
                    }
                }
            }

            bool validName = false;
            while (!validName)
            {
                bool matchesName = false;

                foreach (string name in names)
                {
                    // Name already exists
                    if (name == baseName)
                    {
                        // Increment postfix number by 1
                        int old_value = postfix;
                        postfix = postfix + 1;

                        // Replace baseName postfix number
                        baseName = baseName.Replace($"{PadNameString(old_value)}", $"{PadNameString(postfix)}");

                        matchesName = true;
                    }
                }

                // If it does not match any name during 1 full iteration, then it must be valid
                if (!matchesName)
                {
                    validName = true;
                }
            }

            return baseName;
        }

        public void SetUniqueInstanceID(MapEntity selected, string modelName)
        {
            Map? m;
            m = _universe.GetLoadedMap(selected.MapID);

            Dictionary<Map, HashSet<MapEntity>> mapPartEntities = new();

            if (selected.WrappedObject is MSBE.Part msbePart)
            {
                if (mapPartEntities.TryAdd(m, new HashSet<MapEntity>()))
                {
                    foreach (Entity ent in m.Objects)
                    {
                        if (ent.WrappedObject != null && ent.WrappedObject is MSBE.Part)
                        {
                            mapPartEntities[m].Add((MapEntity)ent);
                        }
                    }
                }

                var newInstanceID = 9000; // Default start value

                while (mapPartEntities[m].FirstOrDefault(e =>
                           ((MSBE.Part)e.WrappedObject).ModelName == msbePart.ModelName
                           && ((MSBE.Part)e.WrappedObject).InstanceID == newInstanceID
                           && msbePart != (MSBE.Part)e.WrappedObject) != null)
                {
                    newInstanceID++;
                }

                msbePart.InstanceID = newInstanceID;
            }
        }

        public string PadNameString(int value)
        {
            if (value < 10)
                return $"000{value}";

            if (value >= 10 && value < 100)
                return $"00{value}";

            if (value >= 100 && value < 1000)
                return $"0{value}";

            return $"{value}";
        }
    }
}
