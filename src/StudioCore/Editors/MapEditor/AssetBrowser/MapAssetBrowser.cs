using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using StudioCore.Gui;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using SoulsFormats;
using StudioCore.Platform;
using StudioCore.MsbEditor;
using Action = StudioCore.Editors.MapEditor.ViewportAction;
using StudioCore.Interface;
using StudioCore.Banks.AliasBank;
using StudioCore.UserProject;
using StudioCore.AssetLocator;
using StudioCore.Banks;
using StudioCore.BanksMain;
using StudioCore.Editor;
using static SoulsFormats.ACB;

namespace StudioCore.Editors.MapEditor.AssetBrowser;

public class MapAssetBrowser
{
    private readonly ViewportActionManager _actionManager;

    private readonly RenderScene _scene;
    private readonly ViewportSelection _selection;

    private MapEditorScreen _msbEditor;

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

    private Universe _universe;

    public MapAssetBrowser(Universe universe, RenderScene scene, ViewportSelection sel, ViewportActionManager manager, MapEditorScreen editor, IViewport viewport)
    {
        _scene = scene;
        _selection = sel;
        _actionManager = manager;
        _universe = universe;

        _msbEditor = editor;
        _viewport = viewport;

        _selectedName = null;
    }

    /// <summary>
    /// Display the Asset Browser window.
    /// </summary>
    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();

        if (Project.Type == ProjectType.Undefined)
            return;

        if (!CFG.Current.Interface_MapEditor_AssetBrowser)
            return;

        if (ModelAliasBank.Bank.IsLoadingAliases)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Asset Browser##MapEditor_AssetBrowser"))
        {
            DisplayTopSection();

            if (CFG.Current.Interface_MapEditor_AssetBrowser_HorizontalOrientation)
            {
                ImGui.Columns(3);

                ImGui.BeginChild("##MapEditor_AssetBrowser_CategoryList");

                ImGui.Separator();
                ImGui.Text("Categories:");
                ImGui.Separator();

                DisplayCategoryList();

                ImGui.EndChild();

                ImGui.NextColumn();

                ImGui.BeginChild("##MapEditor_AssetBrowser_CategoryContentsList");

                ImGui.Separator();
                ImGui.Text("Assets:");
                ImGui.Separator();

                DisplayCategoryContentsList("Chr", ModelAliasBank.Bank.AliasNames.GetEntries("Characters"));
                DisplayCategoryContentsList("Obj", ModelAliasBank.Bank.AliasNames.GetEntries("Objects"));
                DisplayMapPieceContentsList("MapPiece", ModelAliasBank.Bank.AliasNames.GetEntries("MapPieces"));

                ImGui.EndChild();

                ImGui.NextColumn();

                ImGui.BeginChild("##MapEditor_AssetBrowser_SelectedMenu");

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

                ImGui.BeginChild("##MapEditor_AssetBrowser_CategoryList", new Vector2((width - 10), (height / 4)));

                DisplayCategoryList();

                ImGui.EndChild();

                ImGui.Separator();

                ImGui.BeginChild("##MapEditor_AssetBrowser_CategoryContentsList", new Vector2((width-10), (height/3)));

                ImGui.Separator();
                ImGui.Text("Assets:");
                ImGui.Separator();

                DisplayCategoryContentsList("Chr", ModelAliasBank.Bank.AliasNames.GetEntries("Characters"));
                DisplayCategoryContentsList("Obj", ModelAliasBank.Bank.AliasNames.GetEntries("Objects"));
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
            CFG.Current.Interface_MapEditor_AssetBrowser_HorizontalOrientation = !CFG.Current.Interface_MapEditor_AssetBrowser_HorizontalOrientation;
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

        ImGui.Text("Apply the selected asset attributes to your current object selection.");
        ImGui.Text("");

        ImGui.Checkbox("Update Name of Selected Object", ref CFG.Current.AssetBrowser_UpdateName);
        ImguiUtils.ShowHoverTooltip("Update the Name property of the selected entity when it is changed to a selected asset.");

        if (Project.Type == ProjectType.ER || Project.Type == ProjectType.AC6)
        {
            ImGui.Checkbox("Update Instance ID of Selected Object", ref CFG.Current.AssetBrowser_UpdateInstanceID);
            ImguiUtils.ShowHoverTooltip("Update the Name property of the selected entity when it is changed to a selected asset.");
            ImGui.Text("");
        }

        if (ImGui.Button("Apply##action_Asset_Apply", new Vector2(200, 32)))
        {
            ApplyAssetSelection();
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

    /// <summary>
    /// Display the asset category type selection list: Chr, Obj/AEG, Part and each map id for Map Pieces.
    /// </summary>
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

        _loadedMaps.Clear();

        // Map-specific MapPieces
        foreach (var mapId in _mapModelNameCache.Keys)
        {
            foreach (var obj in _msbEditor.Universe.LoadedObjectContainers)
            {
                if (obj.Value != null)
                {
                    _loadedMaps.Add(obj.Key);
                }
            }

            if (_loadedMaps.Contains(mapId))
            {
                var labelName = MapAliasBank.GetFormattedMapName(mapId, mapId);

                if (ImGui.Selectable(labelName, _selectedAssetMapId == mapId))
                {
                    if (_mapModelNameCache[mapId] == null)
                    {
                        List<AssetDescription> modelList = ModelAssetLocator.GetMapModels(mapId);
                        var cache = new List<string>();

                        foreach (AssetDescription model in modelList)
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

                if (SearchFilters.IsSearchMatch(_searchInput, lowerName, refName, refTagList, true))
                {
                    if (ImGui.Selectable(displayedName, _selectedName == name))
                    {
                        _currentScrollY = ImGui.GetScrollY();

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

                    if (SearchFilters.IsSearchMatch(_searchInput, lowerName, refName, refTagList, true))
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

    private void ApplyAssetSelection()
    {
        var modelName = _selectedName;
        var assetType = _selectedAssetType;

        if (modelName.Contains("aeg"))
        {
            modelName = modelName.Replace("aeg", "AEG");
        }

        if (assetType == "MapPiece")
        {
            SetObjectModelForSelection(modelName, assetType, _selectedAssetMapId);
        }
        else
        {
            SetObjectModelForSelection(modelName, assetType, "");
        }
    }

    public void SetObjectModelForSelection(string modelName, string assetType, string assetMapId)
    {
        var actlist = new List<ViewportAction>();

        var selected = _selection.GetFilteredSelection<Entity>();

        foreach (var s in selected)
        {
            var isValidObjectType = false;

            if (assetType == "Chr")
            {
                switch (Project.Type)
                {
                    case ProjectType.DES:
                        if (s.WrappedObject is MSBD.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        if (s.WrappedObject is MSB1.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS2S:
                        break;
                    case ProjectType.DS3:
                        if (s.WrappedObject is MSB3.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.BB:
                        if (s.WrappedObject is MSBB.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.SDT:
                        if (s.WrappedObject is MSBS.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ER:
                        if (s.WrappedObject is MSBE.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.AC6:
                        if (s.WrappedObject is MSB_AC6.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    default:
                        throw new ArgumentException("Selected entity type must be Enemy");
                }
            }
            if (assetType == "Obj")
            {
                switch (Project.Type)
                {
                    case ProjectType.DES:
                        if (s.WrappedObject is MSBD.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        if (s.WrappedObject is MSB1.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS2S:
                        if (s.WrappedObject is MSB2.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS3:
                        if (s.WrappedObject is MSB3.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.BB:
                        if (s.WrappedObject is MSBB.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.SDT:
                        if (s.WrappedObject is MSBS.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ER:
                        if (s.WrappedObject is MSBE.Part.Asset)
                            isValidObjectType = true;
                        break;
                    case ProjectType.AC6:
                        if (s.WrappedObject is MSB_AC6.Part.Asset)
                            isValidObjectType = true;
                        break;
                    default:
                        throw new ArgumentException("Selected entity type must be Object/Asset");
                }
            }
            if (assetType == "MapPiece")
            {
                switch (Project.Type)
                {
                    case ProjectType.DES:
                        if (s.WrappedObject is MSBD.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        if (s.WrappedObject is MSB1.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS2S:
                        if (s.WrappedObject is MSB2.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.DS3:
                        if (s.WrappedObject is MSB3.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.BB:
                        if (s.WrappedObject is MSBB.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.SDT:
                        if (s.WrappedObject is MSBS.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ER:
                        if (s.WrappedObject is MSBE.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.AC6:
                        if (s.WrappedObject is MSB_AC6.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    default:
                        throw new ArgumentException("Selected entity type must be MapPiece");
                }
            }

            if (assetType == "MapPiece")
            {
                var mapName = s.Parent.Name;
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
                if (CFG.Current.AssetBrowser_UpdateName)
                {
                    var name = GetUniqueNameString(modelName);
                    s.Name = name;
                    actlist.Add(s.ChangeObjectProperty("Name", name));
                }

                if (CFG.Current.AssetBrowser_UpdateInstanceID)
                {
                    // Name
                    if (s.WrappedObject is MSBE.Part)
                    {
                        SetUniqueInstanceID((MsbEntity)s, modelName);
                    }
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
        var postfix = 0;
        var baseName = $"{modelName}_0000";

        var names = new List<string>();

        // Collect names
        foreach (var o in _universe.LoadedObjectContainers.Values)
        {
            if (o == null)
            {
                continue;
            }

            if (o is MapContainer m)
            {
                foreach (var ob in m.Objects)
                {
                    if (ob is MsbEntity e)
                    {
                        names.Add(ob.Name);
                    }
                }
            }
        }

        var validName = false;
        while (!validName)
        {
            var matchesName = false;

            foreach (var name in names)
            {
                // Name already exists
                if (name == baseName)
                {
                    // Increment postfix number by 1
                    var old_value = postfix;
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

    public void SetUniqueInstanceID(MsbEntity selected, string modelName)
    {
        MapContainer m;
        m = _universe.GetLoadedMap(selected.MapID);

        Dictionary<MapContainer, HashSet<MsbEntity>> mapPartEntities = new();

        if (selected.WrappedObject is MSBE.Part msbePart)
        {
            if (mapPartEntities.TryAdd(m, new HashSet<MsbEntity>()))
            {
                foreach (Entity ent in m.Objects)
                {
                    if (ent.WrappedObject != null && ent.WrappedObject is MSBE.Part)
                    {
                        mapPartEntities[m].Add((MsbEntity)ent);
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
        {
            return $"000{value}";
        }

        if (value >= 10 && value < 100)
        {
            return $"00{value}";
        }

        if (value >= 100 && value < 1000)
        {
            return $"0{value}";
        }

        return $"{value}";
    }
}
