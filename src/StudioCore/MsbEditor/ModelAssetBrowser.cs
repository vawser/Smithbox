using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using SoulsFormats.KF4;
using StudioCore.Aliases;
using StudioCore.JSON.Assetdex;
using StudioCore.Platform;
using StudioCore.Settings;
using StudioCore.Utilities;
using Veldrid;

namespace StudioCore.MsbEditor
{
    public interface AssetBrowserEventHandler
    {
        public void OnInstantiateChr(string chrid);
        public void OnInstantiateObj(string objid);
        public void OnInstantiateParts(string objid);
        public void OnInstantiateMapPiece(string mapid, string modelid);
    }

    public class ModelAssetBrowser
    {
        private string _id;

        private AssetBrowserEventHandler _handler;

        private List<string> _modelNameCache = new List<string>();
        private Dictionary<string, List<string>> _mapModelNameCache = new Dictionary<string, List<string>>();

        private AssetLocator _assetLocator;

        private string _selectedAssetType = null;
        private string _selectedAssetTypeCache = null;

        private string _selectedAssetMapId = "";
        private string _selectedAssetMapIdCache = null;

        private string _searchStrInput = "";
        private string _searchStrInputCache = "";

        public ModelAssetBrowser(AssetBrowserEventHandler handler, string id, AssetLocator locator)
        {
            _id = id;
            _assetLocator = locator;
            _handler = handler;
        }

        public void OnProjectChanged()
        {
            if (_assetLocator.Type != GameType.Undefined)
            {
                _modelNameCache = new List<string>();
                _mapModelNameCache = new Dictionary<string, List<string>>();
                _selectedAssetMapId = "";
                _selectedAssetMapIdCache = null;

                List<string> mapList = _assetLocator.GetFullMapList();

                foreach (string mapId in mapList)
                {
                    var assetMapId = _assetLocator.GetAssetMapID(mapId);

                    if (!_mapModelNameCache.ContainsKey(assetMapId))
                    {
                        _mapModelNameCache.Add(assetMapId, null);
                    }
                }
            }
        }

        public void Display()
        {
            if (_assetLocator.Type == GameType.Undefined)
                return;

            if (ModelAliasBank._loadedModelAliasBank == null)
                return;

            if (ImGui.Begin($@"Asset Browser##{_id}"))
            {
                ImGui.Columns(2);

                // Asset Type List
                ImGui.BeginChild("AssetTypeList");

                DisplayAssetTypeSelectionList();

                ImGui.EndChild();

                // Asset List
                ImGui.NextColumn();

                ImGui.BeginChild("AssetListSearch");
                ImGui.InputText($"Search", ref _searchStrInput, 255);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.BeginChild("AssetList");

                DisplayAssetSelectionList("Chr", ModelAliasBank._loadedModelAliasBank.GetChrEntries());
                DisplayAssetSelectionList("Obj", ModelAliasBank._loadedModelAliasBank.GetObjEntries());
                DisplayAssetSelectionList("Parts", ModelAliasBank._loadedModelAliasBank.GetPartEntries());
                DisplayMapAssetSelectionList("MapPiece", ModelAliasBank._loadedModelAliasBank.GetMapPieceEntries());

                ImGui.EndChild();
                ImGui.EndChild();
            }
            ImGui.End();
        }
        private void DisplayAssetTypeSelectionList()
        {
            string objLabel = "Obj";

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
            if (ImGui.Selectable("Parts", _selectedAssetType == "Parts"))
            {
                _modelNameCache = _assetLocator.GetPartsModels();
                _selectedAssetType = "Parts";
                _selectedAssetMapId = "";
            }

            foreach (var mapId in _mapModelNameCache.Keys)
            {
                string labelName = mapId;

                if (MapAliasBank.MapNames.ContainsKey(mapId))
                {
                    labelName = labelName + $" <{MapAliasBank.MapNames[mapId]}>";
                }

                if (ImGui.Selectable(labelName, _selectedAssetMapId == mapId))
                {
                    if (_mapModelNameCache[mapId] == null)
                    {
                        var modelList = _assetLocator.GetMapModels(mapId);
                        var cache = new List<string>();
                        foreach (var model in modelList)
                        {
                            cache.Add(model.AssetName);
                        }
                        _mapModelNameCache[mapId] = cache;
                    }

                    _selectedAssetMapId = mapId;
                    _selectedAssetType = "MapPiece";
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
                referenceDict.Add(v.id, v);
            }

            if (_selectedAssetType == assetType)
            {
                if (_searchStrInput != _searchStrInputCache || _selectedAssetType != _selectedAssetTypeCache)
                {
                    _searchStrInputCache = _searchStrInput;
                    _selectedAssetTypeCache = _selectedAssetType;
                }
                foreach (var name in _modelNameCache)
                {
                    string displayName = $"{name}";

                    string referenceName = "";
                    List<string> tagList = new List<string>();

                    string lowercaseName = name.ToLower();

                    if (referenceDict.ContainsKey(lowercaseName))
                    {
                        displayName = displayName + $" <{referenceDict[lowercaseName].name}>";

                        if (CFG.Current.AssetBrowser_ShowTagsInBrowser)
                        {
                            string tagString = string.Join(" ", referenceDict[lowercaseName].tags);
                            displayName = $"{displayName} {{ {tagString} }}";
                        }

                        referenceName = referenceDict[lowercaseName].name;
                        tagList = referenceDict[lowercaseName].tags;
                    }

                    if (Utils.IsSearchFilterMatch(_searchStrInput, lowercaseName, referenceName, tagList))
                    {
                        if (ImGui.Selectable(displayName))
                        {
                        }
                        if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                        {
                            if (_selectedAssetType == "Chr")
                            {
                                _handler.OnInstantiateChr(name);
                            }
                            if (_selectedAssetType == "Obj")
                            {
                                _handler.OnInstantiateObj(name);
                            }
                            if (_selectedAssetType == "Parts")
                            {
                                _handler.OnInstantiateParts(name);
                            }
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
                referenceDict.Add(v.id, v);
            }

            if (_selectedAssetType == assetType)
            {
                if (_mapModelNameCache.ContainsKey(_selectedAssetMapId))
                {
                    if (_searchStrInput != _searchStrInputCache || _selectedAssetType != _selectedAssetTypeCache || _selectedAssetMapId != _selectedAssetMapIdCache)
                    {
                        _searchStrInputCache = _searchStrInput;
                        _selectedAssetTypeCache = _selectedAssetType;
                        _selectedAssetMapIdCache = _selectedAssetMapId;
                    }
                    foreach (string name in _mapModelNameCache[_selectedAssetMapId])
                    {
                        string modelName = name.Replace($"{_selectedAssetMapId}_", "m");
                        string displayName = $"{modelName}";

                        // Adjust the name to remove the A{mapId} section.
                        if (_assetLocator.Type == GameType.DarkSoulsPTDE || _assetLocator.Type == GameType.DarkSoulsRemastered)
                        {
                            displayName = displayName.Replace($"A{_selectedAssetMapId.Substring(1, 2)}", "");
                        }

                        string referenceName = "";
                        List<string> tagList = new List<string>();

                        string lowercaseName = name.ToLower();

                        if (referenceDict.ContainsKey(lowercaseName))
                        {
                            displayName = displayName + $" <{referenceDict[lowercaseName].name}>";

                            if (CFG.Current.AssetBrowser_ShowTagsInBrowser)
                            {
                                string tagString = string.Join(" ", referenceDict[lowercaseName].tags);
                                displayName = $"{displayName} {{ {tagString} }}";
                            }

                            referenceName = referenceDict[lowercaseName].name;
                            tagList = referenceDict[lowercaseName].tags;
                        }

                        if (Utils.IsSearchFilterMatch(_searchStrInput, lowercaseName, referenceName, tagList))
                        {
                            if (ImGui.Selectable(displayName))
                            {
                            }
                            if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                            {
                                _handler.OnInstantiateMapPiece(_selectedAssetMapId, name);
                            }
                        }
                    }
                }
            }
        }
    }
}
