using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using SoulsFormats.KF4;
using StudioCore.AssetLocator;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Interface;
using StudioCore.Interface.Contexts;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using Veldrid;

namespace StudioCore.Editors.ModelEditor
{
    public enum SelectedCategoryType
    {
        None,
        Character,
        Object,
        Part,
        MapPiece
    }

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

        private SelectedCategoryType _selectedAssetType = SelectedCategoryType.None;
        private SelectedCategoryType _selectedAssetTypeCache = SelectedCategoryType.None;

        private string _selectedAssetMapId = "";
        private string _selectedAssetMapIdCache = null;

        private string _searchStrInput = "";
        private string _searchStrInputCache = "";

        private string _refUpdateId = "";
        private string _refUpdateName = "";
        private string _refUpdateTags = "";

        private string _selectedName;

        private AssetAliasPopup AssetAliasContext;

        public ModelAssetBrowser(AssetBrowserEventHandler handler, string id)
        {
            _id = id;
            _handler = handler;

            _selectedName = null;

            AssetAliasContext = new AssetAliasPopup();
        }
        public string GetSelectedCategoryNameForAliasBank()
        {
            string category = "None";

            switch (_selectedAssetType)
            {
                case SelectedCategoryType.Character:
                    category = "Chr";
                    break;
                case SelectedCategoryType.Object:
                    category = "Obj";
                    break;
                case SelectedCategoryType.Part:
                    category = "Part";
                    break;
                case SelectedCategoryType.MapPiece:
                    category = "MapPiece";
                    break;
            }

            return category;
        }

        public void OnProjectChanged()
        {
            if (Project.Type != ProjectType.Undefined)
            {
                _modelNameCache = new List<string>();
                _mapModelNameCache = new Dictionary<string, List<string>>();
                _selectedAssetMapId = "";
                _selectedAssetMapIdCache = null;
                _selectedAssetType = SelectedCategoryType.None;
                _selectedAssetTypeCache = SelectedCategoryType.None;

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
            {
                return;
            }

            if (ModelAliasBank.Bank.IsLoadingAliases)
            {
                return;
            }

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
                ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Checkbox("Show tags", ref CFG.Current.AssetBrowser_ShowTagsInBrowser);
                ImguiUtils.ShowHoverTooltip("Show the tags for each entry within the browser list as part of their displayed name.");

                if(_selectedAssetType == SelectedCategoryType.Part)
                {
                    ImGui.Checkbox("Show low detail models", ref CFG.Current.AssetBrowser_ShowLowDetailParts);
                    ImguiUtils.ShowHoverTooltip("Show the low detail part models in this list.");
                }

                ImGui.BeginChild("AssetList");

                DisplayAssetSelectionList(SelectedCategoryType.Character, ModelAliasBank.Bank.AliasNames.GetEntries("Characters"));
                DisplayAssetSelectionList(SelectedCategoryType.Object, ModelAliasBank.Bank.AliasNames.GetEntries("Objects"));
                DisplayAssetSelectionList(SelectedCategoryType.Part, ModelAliasBank.Bank.AliasNames.GetEntries("Parts"));
                DisplayMapAssetSelectionList(SelectedCategoryType.MapPiece, ModelAliasBank.Bank.AliasNames.GetEntries("MapPieces"));

                ImGui.EndChild();
                ImGui.EndChild();
            }
            ImGui.End();

            if (ModelAliasBank.Bank.mayReloadAliasBank)
            {
                ModelAliasBank.Bank.mayReloadAliasBank = false;
                ModelAliasBank.Bank.ReloadAliasBank();
            }
        }
        private void DisplayAssetTypeSelectionList()
        {
            var objLabel = "Obj";

            if (Project.Type is ProjectType.ER or ProjectType.AC6)
            {
                objLabel = "AEG";
            }

            if (ImGui.Selectable("Chr", _selectedAssetType == SelectedCategoryType.Character))
            {
                _modelNameCache = ModelAssetLocator.GetChrModels();
                _selectedAssetType = SelectedCategoryType.Character;
                _selectedAssetMapId = "";
            }
            if (ImGui.Selectable(objLabel, _selectedAssetType == SelectedCategoryType.Object))
            {
                _modelNameCache = ModelAssetLocator.GetObjModels();
                _selectedAssetType = SelectedCategoryType.Object;
                _selectedAssetMapId = "";
            }
            if (ImGui.Selectable("Part", _selectedAssetType == SelectedCategoryType.Part))
            {
                _modelNameCache = ModelAssetLocator.GetPartsModels();
                _selectedAssetType = SelectedCategoryType.Part;
                _selectedAssetMapId = "";
            }

            foreach (var mapId in _mapModelNameCache.Keys)
            {
                var labelName = MapAliasBank.GetFormattedMapName(mapId, mapId);

                if (ImGui.Selectable(labelName, _selectedAssetMapId == mapId))
                {
                    if (_mapModelNameCache[mapId] == null)
                    {
                        var modelList = ModelAssetLocator.GetMapModels(mapId);
                        var cache = new List<string>();
                        foreach (var model in modelList)
                        {
                            cache.Add(model.AssetName);
                        }

                        _mapModelNameCache[mapId] = cache;
                    }

                    _selectedAssetMapId = mapId;
                    _selectedAssetType = SelectedCategoryType.MapPiece;
                }
            }
        }

        /// <summary>
        /// Display the asset selection list for Chr, Obj/AEG and Parts.
        /// </summary>
        private void DisplayAssetSelectionList(SelectedCategoryType assetType, List<AliasReference> referenceList)
        {
            var referenceDict = new Dictionary<string, AliasReference>();

            foreach (AliasReference v in referenceList)
            {
                if (!referenceDict.ContainsKey(v.id))
                {
                    referenceDict.Add(v.id, v);
                }
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

                    if(!CFG.Current.AssetBrowser_ShowLowDetailParts)
                    {
                        if (_selectedAssetType == SelectedCategoryType.Part)
                        {
                            if (name.Substring(name.Length - 2) == "_l")
                            {
                                continue; // Skip this entry if it is a low detail entry
                            }
                        }
                    }

                    if (SearchFilters.IsSearchMatch(_searchStrInput, lowerName, refName, refTagList, true, false, true))
                    {
                        if (ImGui.Selectable(displayedName))
                        {
                            _selectedName = refID;

                            _refUpdateId = refID;
                            _refUpdateName = refName;
                            _refUpdateTags = PresentationUtils.GetTagListString(refTagList);
                        }

                        if (_selectedName == refID)
                        {
                            AssetAliasContext.Show(refID, _refUpdateId, _refUpdateName, _refUpdateTags, GetSelectedCategoryNameForAliasBank());
                        }

                        if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                        {
                            // TODO: fix issue with DS2 loading
                            if (Project.Type != ProjectType.DS2S)
                            {
                                if (_selectedAssetType == SelectedCategoryType.Character)
                                {
                                    _handler.OnInstantiateChr(name);
                                }

                                if (_selectedAssetType == SelectedCategoryType.Object)
                                {
                                    _handler.OnInstantiateObj(name);
                                }

                                if (_selectedAssetType == SelectedCategoryType.Part)
                                {
                                    _handler.OnInstantiateParts(name);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Display the asset selection list for Map Pieces.
        /// </summary>
        private void DisplayMapAssetSelectionList(SelectedCategoryType assetType, List<AliasReference> referenceList)
        {
            var referenceDict = new Dictionary<string, AliasReference>();

            foreach (AliasReference v in referenceList)
            {
                if (!referenceDict.ContainsKey(v.id))
                {
                    referenceDict.Add(v.id, v);
                }
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

                        if (SearchFilters.IsSearchMatch(_searchStrInput, lowerName, refName, refTagList, true, false, true))
                        {
                            if (ImGui.Selectable(displayedName))
                            {
                                _selectedName = refID;

                                _refUpdateId = refID;
                                _refUpdateName = refName;
                                _refUpdateTags = PresentationUtils.GetTagListString(refTagList);
                            }

                            if (_selectedName == refID)
                            {
                                AssetAliasContext.Show(refID, _refUpdateId, _refUpdateName, _refUpdateTags, GetSelectedCategoryNameForAliasBank());
                            }

                            if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                            {
                                // TODO: fix issue with DS2 loading
                                if (Project.Type != ProjectType.DS2S)
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
}
