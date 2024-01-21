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

        private bool disableListGeneration = false;

        private IViewport _viewport;

        private string _selectedName;

        private bool reloadModelAlias = false;

        private bool updateScrollPosition = false;
        private float _currentScrollY;

        private AliasBank _modelAliasBank;
        private AliasBank _mapAliasBank;

        public MsbAssetBrowser(RenderScene scene, Selection sel, ActionManager manager, AssetLocator locator, MsbEditorScreen editor, IViewport viewport, AliasBank modelAliasBank, AliasBank mapAliasBank)
        {
            _scene = scene;
            _selection = sel;
            _actionManager = manager;

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

            if (CFG.Current.AssetBrowser_SuspendListWhenInViewport)
            {
                // Disable the list generation if using the camera panning to prevent visual lag
                if (InputTracker.GetMouseButton(MouseButton.Right) && _viewport.ViewportSelected)
                    disableListGeneration = true;
                else
                    disableListGeneration = false;
            }
            else
            {
                disableListGeneration = false;
            }

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Asset Browser##MsbAssetBrowser"))
            {
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
                    DisplayAssetSelectionList("Chr", _modelAliasBank.AliasNames.GetEntries("Characters"));
                    DisplayAssetSelectionList("Obj", _modelAliasBank.AliasNames.GetEntries("Objects"));
                    DisplayMapAssetSelectionList("MapPiece", _modelAliasBank.AliasNames.GetEntries("MapPieces"));
                }

                ImGui.EndChild();
                ImGui.EndChild();
            }
            ImGui.End();

            if (disableListGeneration)
            {
                updateScrollPosition = true;
            }

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

                            _msbEditor.SetObjectModelForSelection(modelName, assetType, "");
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
                                _msbEditor.SetObjectModelForSelection(modelName, assetType, _selectedAssetMapId);
                            }
                        }
                    }
                }
            }
        }
    }
}
