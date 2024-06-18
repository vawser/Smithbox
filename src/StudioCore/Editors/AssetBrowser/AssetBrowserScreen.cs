using ImGuiNET;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Gui;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CompoundAction = StudioCore.Editors.MapEditor.CompoundAction;

namespace StudioCore.Editors.AssetBrowser;

public enum AssetBrowserSource
{
    MapEditor,
    ModelEditor
}

public enum AssetCategoryType
{
    None,
    Character,
    Asset,
    Part,
    MapPiece
}

public class AssetBrowserScreen
{
    private AssetCategoryType _selectedAssetType = AssetCategoryType.None;
    private AssetCategoryType _selectedAssetTypeCache = AssetCategoryType.None;

    private AssetBrowserSource SourceType;
    private MapEditorScreen MapEditor;
    private ModelEditorScreen ModelEditor;
    private ViewportActionManager _actionManager;
    private RenderScene _scene;
    private ViewportSelection _selection;
    private IViewport _viewport;
    private Universe _universe;

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

    public AssetBrowserScreen(AssetBrowserSource sourceType, Universe universe, RenderScene scene, ViewportSelection sel, ViewportActionManager manager, EditorScreen editor, IViewport viewport)
    {
        SourceType = sourceType;

        MapEditor = null;
        ModelEditor = null;

        if (sourceType == AssetBrowserSource.MapEditor)
        {
            MapEditor = (MapEditorScreen)editor;
        }
        if (sourceType == AssetBrowserSource.ModelEditor)
        {
            ModelEditor = (ModelEditorScreen)editor;
        }

        _scene = scene;
        _selection = sel;
        _actionManager = manager;
        _universe = universe;

        _viewport = viewport;

        _selectedName = null;
    }

    public void OnProjectChanged()
    {
        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            _selectedAssetMapId = "";
            _selectedAssetMapIdCache = null;
            _selectedAssetType = AssetCategoryType.None;
            _selectedAssetTypeCache = AssetCategoryType.None;
        }
    }

    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (SourceType == AssetBrowserSource.MapEditor)
        {
            if (!CFG.Current.Interface_MapEditor_AssetBrowser)
                return;
        }
        if (SourceType == AssetBrowserSource.ModelEditor)
        {
            if (!CFG.Current.Interface_ModelEditor_AssetBrowser)
                return;
        }

        if (Smithbox.BankHandler.CharacterAliases.Aliases == null)
            return;

        if (Smithbox.BankHandler.AssetAliases.Aliases == null)
            return;

        if (Smithbox.BankHandler.PartAliases.Aliases == null)
            return;

        if (Smithbox.BankHandler.MapPieceAliases.Aliases == null)
            return;

        if (!Smithbox.NameCacheHandler.AssetBrowserNameCache.UpdateCacheComplete)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Asset Browser: Category##{SourceType}AssetBrowser_CategoryList"))
        {
            ImGui.Separator();
            ImguiUtils.WrappedText("Categories:");
            ImGui.Separator();

            DisplayCategoryList();
        }

        ImGui.End();
        ImGui.PopStyleColor(1);

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Asset Browser: Contents##{SourceType}AssetBrowser_ContentList"))
        {
            DisplayTopSection();

            ImGui.Separator();
            ImguiUtils.WrappedText("Assets:");
            ImGui.Separator();

            DisplayBrowserList(AssetCategoryType.Character, Smithbox.NameCacheHandler.AssetBrowserNameCache.CharacterList, Smithbox.NameCacheHandler.AssetBrowserNameCache.Characters);
            DisplayBrowserList(AssetCategoryType.Asset, Smithbox.NameCacheHandler.AssetBrowserNameCache.AssetList, Smithbox.NameCacheHandler.AssetBrowserNameCache.Assets);
            DisplayBrowserList(AssetCategoryType.Part, Smithbox.NameCacheHandler.AssetBrowserNameCache.PartList, Smithbox.NameCacheHandler.AssetBrowserNameCache.Parts);
            DisplayBrowserList_MapPiece(AssetCategoryType.MapPiece, Smithbox.NameCacheHandler.AssetBrowserNameCache.MapPieces);
        }

        ImGui.End();
        ImGui.PopStyleColor(1);

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Asset Browser: Actions##{SourceType}AssetBrowser_CategoryList"))
        {
            ImGui.Indent(10.0f);

            ImGui.Separator();
            ImguiUtils.WrappedText("Actions:");
            ImGui.Separator();

            if (SourceType == AssetBrowserSource.MapEditor)
            {
                DisplayActions_MapEditor();
            }
            if (SourceType == AssetBrowserSource.ModelEditor)
            {
                DisplayActions_ModelEditor();
            }

        }

        ImGui.End();
        ImGui.PopStyleColor(1);

        // AssetBrowserCache.UpdateCache();
    }

    private void DisplayTopSection()
    {
        ImGui.Separator();
        ImGui.InputText($"Search", ref _searchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");
    }

    private void DisplayCategoryList()
    {
        var assetLabel = "Objects";

        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            assetLabel = "Assets";
        }

        if (ImGui.Selectable("Characters", _selectedAssetType == AssetCategoryType.Character))
        {
            _selectedAssetType = AssetCategoryType.Character;
            _selectedAssetMapId = "";
        }
        if (ImGui.Selectable(assetLabel, _selectedAssetType == AssetCategoryType.Asset))
        {
            _selectedAssetType = AssetCategoryType.Asset;
            _selectedAssetMapId = "";
        }

        // Only display in Model Editor since Parts aren't used anywhere in Map Editor.
        if (SourceType == AssetBrowserSource.ModelEditor)
        {
            if (ImGui.Selectable("Parts", _selectedAssetType == AssetCategoryType.Part))
            {
                _selectedAssetType = AssetCategoryType.Part;
                _selectedAssetMapId = "";
            }
        }

        foreach (var mapId in Smithbox.NameCacheHandler.AssetBrowserNameCache.MapPieceDict.Keys)
        {
            if (ImGui.Selectable($"MapPieces: {mapId}", _selectedAssetMapId == mapId))
            {
                _selectedAssetMapId = mapId;
                _selectedAssetType = AssetCategoryType.MapPiece;
            }

            if (CFG.Current.AssetBrowser_ShowAliasesInBrowser)
            {
                var labelName = Smithbox.NameCacheHandler.MapNameCache.GetMapName(mapId);
                AliasUtils.DisplayAlias(labelName);
            }
        }
    }

    private void DisplayBrowserList(AssetCategoryType assetType, List<string> nameCache, Dictionary<string, AliasReference> referenceDict)
    {
        if (updateScrollPosition)
        {
            updateScrollPosition = false;
            ImGui.SetScrollY(_currentScrollY);
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
                    refID = referenceDict[lowerName].id;
                    refName = referenceDict[lowerName].name;
                    refTagList = referenceDict[lowerName].tags;
                }

                if (!CFG.Current.AssetBrowser_ShowLowDetailParts)
                {
                    if (_selectedAssetType == AssetCategoryType.Part)
                    {
                        if (name.Substring(name.Length - 2) == "_l")
                        {
                            continue; // Skip this entry if it is a low detail entry
                        }
                    }
                }

                if (SearchFilters.IsAssetBrowserSearchMatch(_searchInput, lowerName, refName, refTagList))
                {
                    if (ImGui.Selectable(displayedName, _selectedName == name))
                    {
                        _currentScrollY = ImGui.GetScrollY();

                        _selectedName = refID;
                        _refUpdateId = refID;
                        _refUpdateName = refName;
                        _refUpdateTags = AliasUtils.GetTagListString(refTagList);
                    }

                    // Alias
                    if (referenceDict.ContainsKey(lowerName))
                    {
                        if (CFG.Current.AssetBrowser_ShowAliasesInBrowser)
                        {
                            var aliasName = referenceDict[lowerName].name;

                            AliasUtils.DisplayAlias(aliasName);
                        }

                        // Tags
                        if (CFG.Current.AssetBrowser_ShowTagsInBrowser)
                        {
                            var tagString = string.Join(" ", referenceDict[lowerName].tags);
                            AliasUtils.DisplayTagAlias(tagString);
                        }
                    }
                }
            }
        }
    }

    private void DisplayBrowserList_MapPiece(AssetCategoryType assetType, Dictionary<string, AliasReference> referenceDict)
    {
        if (_selectedAssetType == assetType)
        {
            if (Smithbox.NameCacheHandler.AssetBrowserNameCache.MapPieceDict.ContainsKey(_selectedAssetMapId))
            {
                if (_searchInput != _searchInputCache || _selectedAssetType != _selectedAssetTypeCache || _selectedAssetMapId != _selectedAssetMapIdCache)
                {
                    _searchInputCache = _searchInput;
                    _selectedAssetTypeCache = _selectedAssetType;
                    _selectedAssetMapIdCache = _selectedAssetMapId;
                }

                foreach (var name in Smithbox.NameCacheHandler.AssetBrowserNameCache.MapPieceDict[_selectedAssetMapId])
                {
                    var modelName = name.Replace($"{_selectedAssetMapId}_", "m");

                    var displayedName = $"{modelName}";
                    var lowerName = name.ToLower();

                    var refID = $"{name}";
                    var refName = "";
                    var refTagList = new List<string>();

                    // Adjust the name to remove the A{mapId} section.
                    if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
                    {
                        displayedName = displayedName.Replace($"A{_selectedAssetMapId.Substring(1, 2)}", "");
                    }

                    if (referenceDict.ContainsKey(lowerName))
                    {
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
                            _refUpdateTags = AliasUtils.GetTagListString(refTagList);
                        }

                        // Alias
                        if (referenceDict.ContainsKey(lowerName))
                        {
                            if (CFG.Current.AssetBrowser_ShowAliasesInBrowser)
                            {
                                var aliasName = referenceDict[lowerName].name;
                                AliasUtils.DisplayAlias(aliasName);
                            }

                            // Tags
                            if (CFG.Current.AssetBrowser_ShowTagsInBrowser)
                            {
                                var tagString = string.Join(" ", referenceDict[lowerName].tags);
                                AliasUtils.DisplayTagAlias(tagString);
                            }
                        }
                    }
                }
            }
        }
    }

    private void DisplayActions_MapEditor()
    {
        if (_selectedName == null || _selectedName == "")
            return;

        ImguiUtils.WrappedText("Apply the selected asset attributes to your current object selection.");
        ImguiUtils.WrappedText("");

        ImGui.Checkbox("Update Name of Selected Object", ref CFG.Current.AssetBrowser_UpdateName);
        ImguiUtils.ShowHoverTooltip("Update the Name property of the selected entity when it is changed to a selected asset.");

        if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Update Instance ID of Selected Object", ref CFG.Current.AssetBrowser_UpdateInstanceID);
            ImguiUtils.ShowHoverTooltip("Update the Name property of the selected entity when it is changed to a selected asset.");
            ImguiUtils.WrappedText("");
        }

        if (ImGui.Button("Apply##action_Asset_Apply", new Vector2(200, 32)))
        {
            ApplyMapAssetSelection();
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

    private void DisplayActions_ModelEditor()
    {
        if (_selectedName == null || _selectedName == "")
            return;

        ImguiUtils.WrappedText("Load the selected asset.");
        ImguiUtils.WrappedText("");

        if (ImGui.Button("Load##action_Asset_Load", new Vector2(200, 32)))
        {
            LoadModelAssetSelection();
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

    private void UpdateAssetAlias()
    {
        switch(_selectedAssetType)
        {
            case AssetCategoryType.Character:
                Smithbox.BankHandler.CharacterAliases.AddToLocalAliasBank(_refUpdateId, _refUpdateName, _refUpdateTags);
                break;
            case AssetCategoryType.Asset:
                Smithbox.BankHandler.AssetAliases.AddToLocalAliasBank(_refUpdateId, _refUpdateName, _refUpdateTags);
                break;
            case AssetCategoryType.Part:
                Smithbox.BankHandler.PartAliases.AddToLocalAliasBank(_refUpdateId, _refUpdateName, _refUpdateTags);
                break;
            case AssetCategoryType.MapPiece:
                Smithbox.BankHandler.MapPieceAliases.AddToLocalAliasBank(_refUpdateId, _refUpdateName, _refUpdateTags);
                break;
        }

        Smithbox.NameCacheHandler.ReloadNameCaches = true;
    }

    private void RestoreAssetAlias()
    {
        switch (_selectedAssetType)
        {
            case AssetCategoryType.Character:
                Smithbox.BankHandler.CharacterAliases.RemoveFromLocalAliasBank(_refUpdateId);
                break;
            case AssetCategoryType.Asset:
                Smithbox.BankHandler.AssetAliases.RemoveFromLocalAliasBank(_refUpdateId);
                break;
            case AssetCategoryType.Part:
                Smithbox.BankHandler.PartAliases.RemoveFromLocalAliasBank(_refUpdateId);
                break;
            case AssetCategoryType.MapPiece:
                Smithbox.BankHandler.MapPieceAliases.RemoveFromLocalAliasBank(_refUpdateId);
                break;
        }

        Smithbox.NameCacheHandler.ReloadNameCaches = true;
    }

    public void ApplyMapAssetSelection()
    {
        var modelName = _selectedName;
        var assetType = _selectedAssetType;

        if (modelName.Contains("aeg"))
        {
            modelName = modelName.Replace("aeg", "AEG");
        }

        if (assetType == AssetCategoryType.MapPiece)
        {
            SetObjectModelForSelection(modelName, assetType, _selectedAssetMapId);
        }
        else
        {
            SetObjectModelForSelection(modelName, assetType, "");
        }
    }

    public void LoadModelAssetSelection()
    {
        var modelName = _selectedName;
        var assetType = _selectedAssetType;

        if (assetType == AssetCategoryType.Character)
        {
            ModelEditor.OnInstantiateChr(modelName);
        }

        if (assetType == AssetCategoryType.Asset)
        {
            ModelEditor.OnInstantiateObj(modelName);
        }

        if (assetType == AssetCategoryType.Part)
        {
            ModelEditor.OnInstantiateParts(modelName);
        }

        if (assetType == AssetCategoryType.MapPiece)
        {
            ModelEditor.OnInstantiateMapPiece(_selectedAssetMapId, modelName);
        }
    }

    public void SetObjectModelForSelection(string modelName, AssetCategoryType assetType, string assetMapId)
    {
        var actlist = new List<ViewportAction>();

        var selected = _selection.GetFilteredSelection<Entity>();

        foreach (var s in selected)
        {
            var isValidObjectType = false;

            if (assetType == AssetCategoryType.Character)
            {
                switch (Smithbox.ProjectType)
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
                    case ProjectType.DS2:
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
            if (assetType == AssetCategoryType.Asset)
            {
                switch (Smithbox.ProjectType)
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
                    case ProjectType.DS2:
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
            if (assetType == AssetCategoryType.MapPiece)
            {
                switch (Smithbox.ProjectType)
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
                    case ProjectType.DS2:
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

            if (assetType == AssetCategoryType.MapPiece)
            {
                if (s.Parent != null)
                {
                    var mapName = s.Parent.Name;
                    if (mapName != assetMapId)
                    {
                        PlatformUtils.Instance.MessageBox($"Map Pieces are specific to each map.\nYou cannot change a Map Piece in {mapName} to a Map Piece from {assetMapId}.", "Object Browser", MessageBoxButtons.OK);

                        isValidObjectType = false;
                    }
                }
                else
                {
                    isValidObjectType = false;
                }
            }

            if (isValidObjectType)
            {
                if (assetType == AssetCategoryType.MapPiece)
                {
                    // Adjust modelName for mappieces, since by default they are mXX_YY_ZZ_AA_<id>
                    string newName = modelName.Replace($"{_selectedAssetMapId}_", "m");
                    modelName = newName;
                }

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
