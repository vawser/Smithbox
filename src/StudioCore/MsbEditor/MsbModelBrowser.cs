using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using StudioCore.Aliases;
using StudioCore.Configuration;
using StudioCore.Gui;
using StudioCore.Scene;
using StudioCore.Settings;
using StudioCore.Utilities;
using Veldrid;

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

        private bool disableListGeneration = false;

        private IViewport _viewport;

        public MsbAssetBrowser(RenderScene scene, Selection sel, ActionManager manager, AssetLocator locator, MsbEditorScreen editor, IViewport viewport)
        {
            _scene = scene;
            _selection = sel;
            _actionManager = manager;

            _assetLocator = locator;
            _msbEditor = editor;
            _viewport = viewport;
        }

        /// <summary>
        /// Update <c>_modelNameCache</c> and <c>_mapModelNameCache</c> when the project has changed.
        /// </summary>
        public void UpdateBrowserCache()
        {
            if (_assetLocator.Type != GameType.Undefined)
            {
                _modelNameCache = new List<string>();
                _mapModelNameCache = new Dictionary<string, List<string>>();
                _selectedAssetMapId = "";
                _selectedAssetMapIdCache = null;

                List<string> mapList = _assetLocator.GetFullMapList();

                foreach (var mapId in mapList)
                {
                    var assetMapId = _assetLocator.GetAssetMapID(mapId);

                    if (!_mapModelNameCache.ContainsKey(assetMapId))
                        _mapModelNameCache.Add(assetMapId, null);
                }
            }
        }

        /// <summary>
        /// Display the Asset Browser window.
        /// </summary>
        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (_assetLocator.Type == GameType.Undefined)
                return;

            if (ModelAliasBank._loadedModelAliasBank == null)
                return;

            // Disable the list generation if using the camera panning to prevent visual lag
            if (InputTracker.GetMouseButton(MouseButton.Right) && _viewport.ViewportSelected)
                disableListGeneration = true;
            else
                disableListGeneration = false;

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Asset Browser##MsbAssetBrowser"))
            {
                if (ImGui.Button("Help"))
                    ImGui.OpenPopup("##AssetBrowserHelp");
                if (ImGui.BeginPopup("##AssetBrowserHelp"))
                {
                    ImGui.Text(
                        "The Asset Browser allows you to browse through all of the available characters, assets and objects and map pieces.\n" +
                        "The search will filter the browser list by filename, reference name and tags.\n" +
                        "\n" +
                        "If a Enemy is selected within the MSB view, \n" +
                        "you can click on an entry within the Chr list to change the enemy to that type.\n" +
                        "\n" +
                        "If a Asset or Obj is selected within the MSB view, \n" +
                        "you can click on an entry within the AEG or Obj list to change the object to that type.\n" +
                        "\n" +
                        "If a Map Piece is selected within the MSB view, \n" +
                        "you can click on an entry within the Map Piece list to change the object to that type.\n" +
                        "Note, you cannot apply a Map Piece asset from a different map to that of the selected Map Piece."
                        );
                    ImGui.EndPopup();
                }

                ImGui.SameLine();
                ImGui.Checkbox("Show Tags", ref CFG.Current.AssetBrowser_ShowTagsInBrowser);
                ImGui.SameLine();
                ImGui.Checkbox("Update Selected Name", ref CFG.Current.AssetBrowser_UpdateSelectionName);

                ImGui.Columns(2);

                // Asset Type List
                ImGui.BeginChild("AssetTypeList");

                if (!disableListGeneration)
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

                if (!disableListGeneration)
                {
                    DisplayAssetSelectionList("Chr", ModelAliasBank._loadedModelAliasBank.GetChrEntries());
                    DisplayAssetSelectionList("Obj", ModelAliasBank._loadedModelAliasBank.GetObjEntries());
                    DisplayMapAssetSelectionList("MapPiece", ModelAliasBank._loadedModelAliasBank.GetMapPieceEntries());
                }

                ImGui.EndChild();
                ImGui.EndChild();
            }
            ImGui.End();
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

                    if (MapAliasBank.MapNames.ContainsKey(mapId))
                        labelName = labelName + $" <{MapAliasBank.MapNames[mapId]}>";

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
        private void DisplayAssetSelectionList(string assetType, List<ModelAliasReference> referenceList)
        {
            Dictionary<string, ModelAliasReference> referenceDict = new Dictionary<string, ModelAliasReference>();

            foreach (ModelAliasReference v in referenceList)
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
                    var displayName = $"{name}";

                    var referenceName = "";
                    var tagList = new List<string>();

                    var lowercaseName = name.ToLower();

                    if (referenceDict.ContainsKey(lowercaseName))
                    {
                        displayName = displayName + $" <{referenceDict[lowercaseName].name}>";

                        if (CFG.Current.AssetBrowser_ShowTagsInBrowser)
                        {
                            var tagString = string.Join(" ", referenceDict[lowercaseName].tags);
                            displayName = $"{displayName} {{ {tagString} }}";
                        }

                        referenceName = referenceDict[lowercaseName].name;
                        tagList = referenceDict[lowercaseName].tags;
                    }

                    if (Utils.IsSearchFilterMatch(_searchInput, lowercaseName, referenceName, tagList))
                    {
                        if (ImGui.Selectable(displayName))
                        {
                        }
                        if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                        {
                            var modelName = name;

                            if (modelName.Contains("aeg"))
                                modelName = modelName.Replace("aeg", "AEG");

                            _msbEditor.SetObjectModelForSelection(modelName, assetType, "");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Display the asset selection list for Map Pieces.
        /// </summary>
        private void DisplayMapAssetSelectionList(string assetType, List<ModelAliasReference> referenceList)
        {
            Dictionary<string, ModelAliasReference> referenceDict = new Dictionary<string, ModelAliasReference>();

            foreach (ModelAliasReference v in referenceList)
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

                        // Adjust the name to remove the A{mapId} section.
                        if (_assetLocator.Type == GameType.DarkSoulsPTDE || _assetLocator.Type == GameType.DarkSoulsRemastered)
                            modelName = modelName.Replace($"A{_selectedAssetMapId.Substring(1, 2)}", "");

                        var displayName = $"{modelName}";

                        var referenceName = "";
                        var tagList = new List<string>();

                        var lowercaseName = name.ToLower();

                        if (referenceDict.ContainsKey(lowercaseName))
                        {
                            displayName = displayName + $" <{referenceDict[lowercaseName].name}>";

                            if (CFG.Current.AssetBrowser_ShowTagsInBrowser)
                            {
                                var tagString = string.Join(" ", referenceDict[lowercaseName].tags);
                                displayName = $"{displayName} {{ {tagString} }}";
                            }

                            referenceName = referenceDict[lowercaseName].name;
                            tagList = referenceDict[lowercaseName].tags;
                        }

                        if (Utils.IsSearchFilterMatch(_searchInput, name, referenceName, tagList))
                        {
                            if (ImGui.Selectable(displayName))
                            {
                            }
                            if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                                _msbEditor.SetObjectModelForSelection(modelName, assetType, _selectedAssetMapId);
                        }
                    }
                }
            }
        }
    }
}
