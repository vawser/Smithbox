using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using SoulsFormats.KF4;
using StudioCore.AssetLocator;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Configuration;
using StudioCore.Interface;
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
                _modelNameCache = new List<string>();
                _mapModelNameCache = new Dictionary<string, List<string>>();
                _selectedAssetMapId = "";
                _selectedAssetMapIdCache = null;
                _selectedAssetType = null;
                _selectedAssetTypeCache = null;

                List<string> mapList = MapAssetLocator.GetFullMapList();

                foreach (var mapId in mapList)
                {
                    var assetMapId = MapAssetLocator.GetAssetMapID(mapId);

                    if (!_mapModelNameCache.ContainsKey(assetMapId))
                        _mapModelNameCache.Add(assetMapId, null);
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

            if (ImGui.Begin($@"Asset Browser##ModelEditor_AssetBrowser"))
            {
                DisplayTopSection();

                if (CFG.Current.Interface_ModelEditor_AssetBrowser_HorizontalOrientation)
                {
                    ImGui.Columns(3);

                    ImGui.BeginChild("##ModelEditor_AssetBrowser_CategoryList");

                    ImGui.Separator();
                    ImGui.Text("Categories:");
                    ImGui.Separator();

                    DisplayCategoryList();

                    ImGui.EndChild();

                    ImGui.NextColumn();

                    ImGui.BeginChild("##ModelEditor_AssetBrowser_CategoryContentsList");

                    ImGui.Separator();
                    ImGui.Text("Assets:");
                    ImGui.Separator();

                    DisplayCategoryContentsList("Chr", ModelAliasBank.Bank.AliasNames.GetEntries("Characters"));
                    DisplayCategoryContentsList("Obj", ModelAliasBank.Bank.AliasNames.GetEntries("Objects"));
                    DisplayCategoryContentsList("Parts", ModelAliasBank.Bank.AliasNames.GetEntries("Parts"));
                    DisplayMapPieceContentsList("MapPiece", ModelAliasBank.Bank.AliasNames.GetEntries("MapPieces"));

                    ImGui.EndChild();

                    ImGui.NextColumn();

                    ImGui.BeginChild("##ModelEditor_AssetBrowser_SelectedMenu");

                    ImGui.Indent(10.0f);

                    ImGui.Separator();
                    ImGui.Text("Actions:");
                    ImGui.Separator();

                    DisplayActionSection();

                    ImGui.EndChild();
                }
                else
                {
                    var width = ImGui.GetWindowWidth();
                    var height = ImGui.GetWindowHeight();

                    ImGui.Separator();
                    ImGui.Text("Categories:");
                    ImGui.Separator();

                    ImGui.BeginChild("##ModelEditor_AssetBrowser_CategoryList", new Vector2((width - 10), (height / 4)));

                    DisplayCategoryList();

                    ImGui.EndChild();

                    ImGui.Separator();

                    ImGui.BeginChild("##ModelEditor_AssetBrowser_CategoryContentsList", new Vector2((width - 10), (height / 3)));

                    ImGui.Separator();
                    ImGui.Text("Assets:");
                    ImGui.Separator();

                    DisplayCategoryContentsList("Chr", ModelAliasBank.Bank.AliasNames.GetEntries("Characters"));
                    DisplayCategoryContentsList("Obj", ModelAliasBank.Bank.AliasNames.GetEntries("Objects"));
                    DisplayCategoryContentsList("Parts", ModelAliasBank.Bank.AliasNames.GetEntries("Parts"));
                    DisplayMapPieceContentsList("MapPiece", ModelAliasBank.Bank.AliasNames.GetEntries("MapPieces"));

                    ImGui.EndChild();

                    ImGui.BeginChild("##MapEditor_AssetBrowser_SelectedMenu");

                    ImGui.Indent(10.0f);

                    ImGui.Separator();
                    ImGui.Text("Actions:");
                    ImGui.Separator();

                    DisplayActionSection();

                    ImGui.EndChild();
                }
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

            if (ImGui.Button($"{ForkAwesome.Refresh}##SwitchOrientation"))
            {
                CFG.Current.Interface_ModelEditor_AssetBrowser_HorizontalOrientation = !CFG.Current.Interface_ModelEditor_AssetBrowser_HorizontalOrientation;
            }
            ImguiUtils.ShowHoverTooltip("Toggle the orientation of the asset browser.");

            ImGui.Checkbox("Display Tags", ref CFG.Current.AssetBrowser_ShowTagsInBrowser);
            ImguiUtils.ShowHoverTooltip("Show the tags for each entry within the browser list as part of their displayed name.");

            ImGui.Separator();
        }

        private void DisplayActionSection()
        {
            if (_selectedName == null || _selectedName == "")
                return;

            ImGui.Text("Load the selected asset.");
            ImGui.Text("");

            if (ImGui.Button("Load##action_Asset_Load", new Vector2(200, 32)))
            {
                LoadAssetSelection();
            }
            ImGui.Text("");

            ImGui.Separator();
            ImGui.Text("Alias:");
            ImGui.Separator();

            ImGui.Text("Update the stored name and tag list for the selected asset here.");
            ImGui.Text("");

            ImGui.Text("Name:");
            ImGui.InputText($"##Name", ref _refUpdateName, 255);
            ImguiUtils.ShowHoverTooltip("Alias name given to this asset.");
            ImGui.Text("");

            ImGui.Text("Tags:");
            ImGui.InputText($"##Tags", ref _refUpdateTags, 255);
            ImguiUtils.ShowHoverTooltip("Tags associated with this asset. Tags are separated with the , character.");
            ImGui.Text("");

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
            if (ImGui.Selectable("Part", _selectedAssetType == "Parts"))
            {
                _modelNameCache = ModelAssetLocator.GetPartsModels();
                _selectedAssetType = "Parts";
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
                    _selectedAssetType = "MapPiece";
                }
            }
        }

        /// <summary>
        /// Display the asset selection list for Chr, Obj/AEG and Parts.
        /// </summary>
        private void DisplayCategoryContentsList(string assetType, List<AliasReference> referenceList)
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
