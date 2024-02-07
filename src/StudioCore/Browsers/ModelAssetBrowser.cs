using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using SoulsFormats.KF4;
using StudioCore.AssetLocator;
using StudioCore.Data.Aliases;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using Veldrid;

namespace StudioCore.Browsers
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

        private string _selectedAssetType = null;
        private string _selectedAssetTypeCache = null;

        private string _selectedAssetMapId = "";
        private string _selectedAssetMapIdCache = null;

        private string _searchStrInput = "";
        private string _searchStrInputCache = "";

        private string _refUpdateId = "";
        private string _refUpdateName = "";
        private string _refUpdateTags = "";

        private string _selectedName;

        private AliasBank _modelAliasBank;
        private AliasBank _mapAliasBank;

        public ModelAssetBrowser(AssetBrowserEventHandler handler, string id, AliasBank modelAliasBank, AliasBank mapAliasBank)
        {
            _id = id;
            _handler = handler;

            _modelAliasBank = modelAliasBank;
            _mapAliasBank = mapAliasBank;

            _selectedName = null;
        }

        public void OnProjectChanged()
        {
            if (Project.Type != ProjectType.Undefined)
            {
                _modelNameCache = new List<string>();
                _mapModelNameCache = new Dictionary<string, List<string>>();
                _selectedAssetMapId = "";
                _selectedAssetMapIdCache = null;

                List<string> mapList = MapAssetLocator.GetFullMapList();

                foreach (var mapId in mapList)
                {
                    var assetMapId = MapAssetLocator.GetAssetMapID(mapId);

                    if (!_mapModelNameCache.ContainsKey(assetMapId))
                        _mapModelNameCache.Add(assetMapId, null);
                }
            }
        }

        public void Display()
        {
            if (Project.Type == ProjectType.Undefined)
                return;

            if (_modelAliasBank.IsLoadingAliases)
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

                ImGui.SameLine();
                ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.BeginChild("AssetList");

                DisplayAssetSelectionList("Chr", _modelAliasBank.AliasNames.GetEntries("Characters"));
                DisplayAssetSelectionList("Obj", _modelAliasBank.AliasNames.GetEntries("Objects"));
                DisplayAssetSelectionList("Part", _modelAliasBank.AliasNames.GetEntries("Parts"));
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
        private void DisplayAssetTypeSelectionList()
        {
            var objLabel = "Obj";

            if (Project.Type is ProjectType.ER or ProjectType.AC6)
                objLabel = "AEG";

            if (ImGui.Selectable("Chr", _selectedAssetType == "Chr"))
            {
                _modelNameCache = ModelAssetLocator.GetChrModels();
                _selectedAssetType = "Chr";
                _selectedAssetMapId = "";
            }
            if (ImGui.Selectable(objLabel, _selectedAssetType == "Obj"))
            {
                _modelNameCache = ModelAssetLocator.GetObjModels();
                _selectedAssetType = "Obj";
                _selectedAssetMapId = "";
            }
            if (ImGui.Selectable("Part", _selectedAssetType == "Part"))
            {
                _modelNameCache = ModelAssetLocator.GetPartsModels();
                _selectedAssetType = "Part";
                _selectedAssetMapId = "";
            }

            foreach (var mapId in _mapModelNameCache.Keys)
            {
                var labelName = mapId;

                if (_mapAliasBank.MapNames != null)
                    if (_mapAliasBank.MapNames.ContainsKey(mapId))
                        labelName = labelName + $" <{_mapAliasBank.MapNames[mapId]}>";

                if (ImGui.Selectable(labelName, _selectedAssetMapId == mapId))
                {
                    if (_mapModelNameCache[mapId] == null)
                    {
                        var modelList = ModelAssetLocator.GetMapModels(mapId);
                        var cache = new List<string>();
                        foreach (var model in modelList)
                            cache.Add(model.AssetName);
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
        private void DisplayAssetSelectionList(string assetType, List<AliasReference> referenceList)
        {
            var referenceDict = new Dictionary<string, AliasReference>();

            foreach (AliasReference v in referenceList)
                if (!referenceDict.ContainsKey(v.id))
                    referenceDict.Add(v.id, v);

            if (_selectedAssetType == assetType)
            {
                if (_searchStrInput != _searchStrInputCache || _selectedAssetType != _selectedAssetTypeCache)
                {
                    _searchStrInputCache = _searchStrInput;
                    _selectedAssetTypeCache = _selectedAssetType;
                }
                foreach (var name in _modelNameCache)
                {
                    var displayedName = $"{name}";
                    var lowerName = name.ToLower();

                    var refID = $"{name}";
                    var refName = "";
                    var refTagList = new List<string>();

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

                    if (SearchFilters.IsSearchMatch(_searchStrInput, lowerName, refName, refTagList, true))
                    {
                        if (ImGui.Selectable(displayedName))
                        {
                            _selectedName = refID;

                            _refUpdateId = refID;
                            _refUpdateName = refName;

                            if (refTagList.Count > 0)
                            {
                                var tagStr = refTagList[0];
                                foreach (var entry in refTagList.Skip(1))
                                    tagStr = $"{tagStr},{entry}";
                                _refUpdateTags = tagStr;
                            }
                            else
                                _refUpdateTags = "";
                        }

                        if (_selectedName == refID)
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

                        if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                        {
                            if (_selectedAssetType == "Chr")
                                _handler.OnInstantiateChr(name);
                            if (_selectedAssetType == "Obj")
                                _handler.OnInstantiateObj(name);
                            if (_selectedAssetType == "Part")
                                _handler.OnInstantiateParts(name);
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
            var referenceDict = new Dictionary<string, AliasReference>();

            foreach (AliasReference v in referenceList)
                if (!referenceDict.ContainsKey(v.id))
                    referenceDict.Add(v.id, v);

            if (_selectedAssetType == assetType)
                if (_mapModelNameCache.ContainsKey(_selectedAssetMapId))
                {
                    if (_searchStrInput != _searchStrInputCache || _selectedAssetType != _selectedAssetTypeCache || _selectedAssetMapId != _selectedAssetMapIdCache)
                    {
                        _searchStrInputCache = _searchStrInput;
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
                        if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DS1R)
                            displayedName = displayedName.Replace($"A{_selectedAssetMapId.Substring(1, 2)}", "");

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

                        if (SearchFilters.IsSearchMatch(_searchStrInput, lowerName, refName, refTagList, true))
                        {
                            if (ImGui.Selectable(displayedName))
                            {
                                _selectedName = refID;

                                _refUpdateId = refID;
                                _refUpdateName = refName;

                                if (refTagList.Count > 0)
                                {
                                    var tagStr = refTagList[0];
                                    foreach (var entry in refTagList.Skip(1))
                                        tagStr = $"{tagStr},{entry}";
                                    _refUpdateTags = tagStr;
                                }
                                else
                                    _refUpdateTags = "";
                            }

                            if (_selectedName == refID)
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

                            if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                                _handler.OnInstantiateMapPiece(_selectedAssetMapId, name);
                        }
                    }
                }
        }
    }
}
