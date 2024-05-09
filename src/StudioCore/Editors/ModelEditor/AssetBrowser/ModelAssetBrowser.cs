using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using SoulsFormats.KF4;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using Veldrid;

namespace StudioCore.Editors.ModelEditor
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
        private AssetBrowserEventHandler _handler;

        private List<string> _characterNameCache = new List<string>();
        private List<string> _objectNameCache = new List<string>();
        private List<string> _partNameCache = new List<string>();
        private Dictionary<string, List<string>> _mapPieceNameCache = new Dictionary<string, List<string>>();

        private string _selectedAssetType = null;
        private string _selectedAssetTypeCache = null;

        private string _selectedAssetMapId = "";
        private string _selectedAssetMapIdCache = null;

        private string _searchInput = "";
        private string _searchInputCache = "";

        private string _refUpdateId = "";
        private string _refUpdateName = "";
        private string _refUpdateTags = "";

        private string _selectedName;

        private bool updateScrollPosition = false;
        private float _currentScrollY;


        public ModelAssetBrowser(AssetBrowserEventHandler handler)
        {
            _handler = handler;

            _selectedName = null;
        }

        public void OnProjectChanged()
        {
            if (Project.Type != ProjectType.Undefined)
            {
                _characterNameCache = AssetListLocator.GetChrModels();
                _objectNameCache = AssetListLocator.GetObjModels();
                _partNameCache = AssetListLocator.GetPartsModels();
                _mapPieceNameCache = new Dictionary<string, List<string>>();

                _selectedAssetMapId = "";
                _selectedAssetMapIdCache = null;
                _selectedAssetType = null;
                _selectedAssetTypeCache = null;

                List<string> mapList = ResourceMapLocator.GetFullMapList();

                foreach (var mapId in mapList)
                {
                    var assetMapId = ResourceMapLocator.GetAssetMapID(mapId);

                    List<ResourceDescriptor> modelList = new List<ResourceDescriptor>();

                    if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
                    {
                        modelList = AssetListLocator.GetMapModelsFromBXF(mapId);
                    }
                    else
                    {
                        modelList = AssetListLocator.GetMapModels(mapId);
                    }

                    var cache = new List<string>();
                    foreach (var model in modelList)
                    {
                        cache.Add(model.AssetName);
                    }

                    if (!_mapPieceNameCache.ContainsKey(assetMapId))
                    {
                        _mapPieceNameCache.Add(assetMapId, cache);
                    }
                }
            }
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (Project.Type == ProjectType.Undefined)
                return;

            if (!CFG.Current.Interface_ModelEditor_AssetBrowser)
                return;
            
            if (ModelAliasBank.Bank.IsLoadingAliases)
                return;

            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Asset Browser: Category##ModelEditor_AssetBrowser_CategoryList"))
            {
                DisplayTopSection();

                ImGui.Separator();
                ImguiUtils.WrappedText("Categories:");
                ImGui.Separator();

                DisplayCategoryList();
            }

            ImGui.End();
            ImGui.PopStyleColor(1);

            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Asset Browser: Contents##ModelEditor_AssetBrowser_ContentList"))
            {
                ImGui.Separator();
                ImguiUtils.WrappedText("Assets:");
                ImGui.Separator();

                DisplayCategoryContentsList("Chr", ModelAliasBank.Bank.AliasNames.GetEntries("Characters"), _characterNameCache);
                DisplayCategoryContentsList("Obj", ModelAliasBank.Bank.AliasNames.GetEntries("Objects"), _objectNameCache);
                DisplayCategoryContentsList("Parts", ModelAliasBank.Bank.AliasNames.GetEntries("Parts"), _partNameCache);
                DisplayMapPieceContentsList("MapPiece", ModelAliasBank.Bank.AliasNames.GetEntries("MapPieces"));
            }

            ImGui.End();
            ImGui.PopStyleColor(1);

            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Asset Browser: Actions##ModelEditor_AssetBrowser_CategoryList"))
            {
                ImGui.Indent(10.0f);

                ImGui.Separator();
                ImguiUtils.WrappedText("Actions:");
                ImGui.Separator();

                DisplayActionSection();
            }

            ImGui.End();
            ImGui.PopStyleColor(1);

            if (ModelAliasBank.Bank.CanReloadBank)
            {
                ModelAliasBank.Bank.CanReloadBank = false;
                ModelAliasBank.Bank.ReloadAliasBank();
            }
        }

        private void DisplayTopSection()
        {
            ImGui.Separator();
            ImGui.InputText($"Search", ref _searchInput, 255);
            ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");
            ImGui.SameLine();

            ImGui.Checkbox("Display Tags", ref CFG.Current.AssetBrowser_ShowTagsInBrowser);
            ImguiUtils.ShowHoverTooltip("Show the tags for each entry within the browser list as part of their displayed name.");

            ImGui.Separator();
        }

        private void DisplayActionSection()
        {
            if (_selectedName == null || _selectedName == "")
                return;

            ImguiUtils.WrappedText("Load the selected asset.");
            ImguiUtils.WrappedText("");

            if (ImGui.Button("Load##action_Asset_Load", new Vector2(200, 32)))
            {
                LoadAssetSelection();
            }
            ImguiUtils.WrappedText("");

            ImGui.Separator();
            ImguiUtils.WrappedText("Alias:");
            ImGui.Separator();

            ImguiUtils.WrappedText("Update the stored name and tag list for the selected asset here.");
            ImguiUtils.WrappedText("");

            ImguiUtils.WrappedText("Name:");
            ImGui.InputText($"##Name", ref _refUpdateName, 255);
            ImguiUtils.ShowHoverTooltip("Alias name given to this asset.");
            ImguiUtils.WrappedText("");

            ImguiUtils.WrappedText("Tags:");
            ImGui.InputText($"##Tags", ref _refUpdateTags, 255);
            ImguiUtils.ShowHoverTooltip("Tags associated with this asset. Tags are separated with the , character.");
            ImguiUtils.WrappedText("");

            if (ImGui.Button("Update##action_AssetAlias_Update", new Vector2(200, 32)))
            {
                UpdateAssetAlias();
            }
            ImGui.SameLine();
            if (ImGui.Button("Restore Default##action_AssetAlias_Restore", new Vector2(200, 32)))
            {
                RestoreAssetAlias();
            }
        }

        private void DisplayCategoryList()
        {
            var objLabel = "Obj";

            if (Project.Type is ProjectType.ER or ProjectType.AC6)
            {
                objLabel = "AEG";
            }

            if (ImGui.Selectable("Chr", _selectedAssetType == "Chr"))
            {
                _selectedAssetType = "Chr";
                _selectedAssetMapId = "";
            }
            if (ImGui.Selectable(objLabel, _selectedAssetType == "Obj"))
            {
                _selectedAssetType = "Obj";
                _selectedAssetMapId = "";
            }
            if (ImGui.Selectable("Part", _selectedAssetType == "Parts"))
            {
                _selectedAssetType = "Parts";
                _selectedAssetMapId = "";
            }

            foreach (var mapId in _mapPieceNameCache.Keys)
            {
                var labelName = MapAliasBank.GetFormattedMapName(mapId, mapId);

                if (ImGui.Selectable(labelName, _selectedAssetMapId == mapId))
                {
                    _selectedAssetMapId = mapId;
                    _selectedAssetType = "MapPiece";
                }
            }
        }

        /// <summary>
        /// Display the asset selection list for Chr, Obj/AEG and Parts.
        /// </summary>
        private void DisplayCategoryContentsList(string assetType, List<AliasReference> referenceList, List<string> nameCache)
        {
            if (updateScrollPosition)
            {
                updateScrollPosition = false;
                ImGui.SetScrollY(_currentScrollY);
            }

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
                if (_searchInput != _searchInputCache || _selectedAssetType != _selectedAssetTypeCache)
                {
                    _searchInputCache = _searchInput;
                    _selectedAssetTypeCache = _selectedAssetType;
                }

                foreach (var name in nameCache)
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
                        if (_selectedAssetType == "Parts")
                        {
                            if (name.Substring(name.Length - 2) == "_l")
                            {
                                continue; // Skip this entry if it is a low detail entry
                            }
                        }
                    }

                    if (SearchFilters.IsSearchMatch(_searchInput, lowerName, refName, refTagList, true, false, true))
                    {
                        if (ImGui.Selectable(displayedName, _selectedName == name))
                        {
                            _selectedName = refID;

                            _refUpdateId = refID;
                            _refUpdateName = refName;
                            _refUpdateTags = PresentationUtils.GetTagListString(refTagList);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Display the asset selection list for Map Pieces.
        /// </summary>
        private void DisplayMapPieceContentsList(string assetType, List<AliasReference> referenceList)
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
                if (_mapPieceNameCache.ContainsKey(_selectedAssetMapId))
                {
                    if (_searchInput != _searchInputCache || _selectedAssetType != _selectedAssetTypeCache || _selectedAssetMapId != _selectedAssetMapIdCache)
                    {
                        _searchInputCache = _searchInput;
                        _selectedAssetTypeCache = _selectedAssetType;
                        _selectedAssetMapIdCache = _selectedAssetMapId;
                    }
                    foreach (var name in _mapPieceNameCache[_selectedAssetMapId])
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

                        if (SearchFilters.IsSearchMatch(_searchInput, lowerName, refName, refTagList, true, false, true))
                        {
                            if (ImGui.Selectable(displayedName, _selectedName == name))
                            {
                                _selectedName = refID;

                                _refUpdateId = refID;
                                _refUpdateName = refName;
                                _refUpdateTags = PresentationUtils.GetTagListString(refTagList);
                            }
                        }
                    }
                }
            }
        }
        private void UpdateAssetAlias()
        {
            var assetType = _selectedAssetType;

            ModelAliasBank.Bank.AddToLocalAliasBank(assetType, _refUpdateId, _refUpdateName, _refUpdateTags);
            ImGui.CloseCurrentPopup();
            ModelAliasBank.Bank.CanReloadBank = true;
        }

        private void RestoreAssetAlias()
        {
            var assetType = _selectedAssetType;

            ModelAliasBank.Bank.RemoveFromLocalAliasBank(assetType, _refUpdateId);
            ModelAliasBank.Bank.CanReloadBank = true;
        }
        public void LoadAssetSelection()
        {
            var modelName = _selectedName;
            var assetType = _selectedAssetType;

            if (assetType == "Chr")
            {
                _handler.OnInstantiateChr(modelName);
            }

            if (assetType == "Obj")
            {
                _handler.OnInstantiateObj(modelName);
            }

            if (assetType == "Parts")
            {
                _handler.OnInstantiateParts(modelName);
            }

            if (assetType == "MapPiece")
            {
                _handler.OnInstantiateMapPiece(_selectedAssetMapId, modelName);
            }
        }
    }
}
