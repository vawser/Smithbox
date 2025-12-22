using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;
public class ModelSelectorTool
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    private string _searchInput = "";
    private string _selectedEntry = "";

    private bool SelectNextEntry = false;

    public ModelSelectorTool(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        if (ImGui.CollapsingHeader("Model Selector"))
        {
            ImGui.InputText($"##selectorFilter", ref _searchInput, 255);
            UIHelper.Tooltip("Filter the model selector list. Separate terms are split via the + character.");

            ImGui.Checkbox("Update Name on Switch", ref CFG.Current.AssetBrowser_UpdateName);
            UIHelper.Tooltip("When a map object is switched to a new form, update the name to match the new form.");

            if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                ImGui.SameLine();

                ImGui.Checkbox("Update Instance ID on Switch", ref CFG.Current.AssetBrowser_UpdateInstanceID);
                UIHelper.Tooltip("When a map object is switched to a new form, update the Instance ID to account for the new form.");
            }

            var curSelection = Editor.ViewportSelection.GetSelection();

            if (curSelection.Count > 0)
            {
                var firstSelection = (Entity)curSelection.First();

                if (firstSelection.IsPartEnemy() || firstSelection.IsPartDummyEnemy())
                {
                    DisplayCharacterList();
                }
                else if (firstSelection.IsPartAsset() || firstSelection.IsPartDummyAsset())
                {
                    DisplayAssetList();
                }
                else if (firstSelection.IsPartMapPiece())
                {
                    DisplayMapPieceList();
                }
                else
                {
                    ImGui.Text("Your current selection does not use a model.");
                }
            }
            else
            {
                ImGui.Text("You must select a valid map object first.");
            }
        }
    }

    private void DisplayCharacterList()
    {
        // TODO: this needs to draw from a scanned list of characters, not the alias list
        if (Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Characters, out List<AliasEntry> characterAliases))
        {
            var windowSize = DPI.GetWindowSize(Editor.BaseEditor._context);
            var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
            var sectionHeight = windowSize.Y * 0.3f;
            var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

            ImGui.BeginChild("##characterSelectorList", sectionSize, ImGuiChildFlags.Borders);

            foreach (var entry in characterAliases)
            {
                if (FilterSelectionList(entry))
                {
                    if (ImGui.Selectable(entry.ID, entry.ID == _selectedEntry, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedEntry = entry.ID;

                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            ApplyMapAssetSelection(_selectedEntry, FileSelectionType.Character);
                        }
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && SelectNextEntry)
                    {
                        SelectNextEntry = false;
                        _selectedEntry = entry.ID;
                        ApplyMapAssetSelection(_selectedEntry, FileSelectionType.Character);
                    }

                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        SelectNextEntry = true;
                    }

                    DisplaySelectableAlias(entry);
                }
            }

            ImGui.EndChild();
        }
    }

    private void DisplayAssetList()
    {
        // TODO: this needs to draw from a scanned list of assets, not the alias list
        if (Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Assets, out List<AliasEntry> assetAliases))
        {
            var windowSize = DPI.GetWindowSize(Editor.BaseEditor._context);
            var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
            var sectionHeight = windowSize.Y * 0.3f;
            var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

            ImGui.BeginChild("##assetSelectorList", sectionSize, ImGuiChildFlags.Borders);

            foreach (var entry in assetAliases)
            {
                if (FilterSelectionList(entry))
                {
                    if (ImGui.Selectable(entry.ID, entry.ID == _selectedEntry, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedEntry = entry.ID;

                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            ApplyMapAssetSelection(_selectedEntry, FileSelectionType.Asset);
                        }
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && SelectNextEntry)
                    {
                        SelectNextEntry = false;
                        _selectedEntry = entry.ID;
                        ApplyMapAssetSelection(_selectedEntry, FileSelectionType.Asset);
                    }

                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        SelectNextEntry = true;
                    }

                    DisplaySelectableAlias(entry);
                }
            }

            ImGui.EndChild();
        }
    }

    private void DisplayMapPieceList()
    {
        var maps = MsbUtils.GetFullMapList(Editor.Project);

        // TODO: this needs to draw from a scanned list of map pieces, not the alias list
        if (Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.MapPieces, out List<AliasEntry> mapPieceAliases))
        {
            var windowSize = DPI.GetWindowSize(Editor.BaseEditor._context);
            var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
            var sectionHeight = windowSize.Y * 0.3f;
            var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

            ImGui.BeginChild("##mapPieceSelectorList", sectionSize, ImGuiChildFlags.Borders);

            foreach (var map in maps)
            {
                var displayedMapName = $"{map} - {AliasHelper.GetMapNameAlias(Editor.Project, map)}";

                UIHelper.SimpleHeader($"{map}_header", $"{displayedMapName}", "", UI.Current.ImGui_Default_Text_Color);

                var displayedName = $"{map}";
                var modelName = map.Replace($"{map}_", "m");
                displayedName = $"{modelName}";

                if (Editor.Project.ProjectType == ProjectType.DS1 || Editor.Project.ProjectType == ProjectType.DS1R)
                {
                    displayedName = displayedName.Replace($"A{map.Substring(1, 2)}", "");
                }

                foreach (var entry in mapPieceAliases)
                {
                    var mapPieceName = $"{entry.ID.Replace(map, "m")}";

                    if (ImGui.Selectable(mapPieceName, entry.ID == _selectedEntry, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedEntry = entry.ID;

                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            ApplyMapAssetSelection(_selectedEntry, FileSelectionType.MapPiece, map);
                        }
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && SelectNextEntry)
                    {
                        SelectNextEntry = false;
                        _selectedEntry = entry.ID;
                        ApplyMapAssetSelection(_selectedEntry, FileSelectionType.MapPiece, map);
                    }

                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        SelectNextEntry = true;
                    }

                    DisplaySelectableAlias(entry);
                }
            }

            ImGui.EndChild();
        }
    }
    private bool FilterSelectionList(AliasEntry entry)
    {
        var lowerName = entry.ID.ToLower();

        var refName = entry.Name;
        var refTagList = entry.Tags;

        if (!CFG.Current.MapEditor_AssetBrowser_ShowLowDetailParts)
        {
            if (entry.ID.Substring(entry.ID.Length - 2) == "_l")
            {
                return false;
            }
        }

        if (!SearchFilters.IsAssetBrowserSearchMatch(_searchInput, lowerName, refName, refTagList))
        {
            return false;
        }

        return true;
    }

    private void DisplaySelectableAlias(AliasEntry entry)
    {
        var lowerName = entry.ID.ToLower();

        if (CFG.Current.MapEditor_AssetBrowser_ShowAliases)
        {
            UIHelper.DisplayAlias(entry.Name);
        }

        // Tags
        if (CFG.Current.MapEditor_AssetBrowser_ShowTags)
        {
            var tagString = string.Join(" ", entry.Tags);
            AliasHelper.DisplayTagAlias(tagString);
        }
    }

    private void ApplyMapAssetSelection(string _selectedName, FileSelectionType type, string mapId = "")
    {
        var modelName = _selectedName;

        if (modelName.Contains("aeg"))
        {
            modelName = modelName.Replace("aeg", "AEG");
        }

        if (type == FileSelectionType.MapPiece)
        {
            SetObjectModelForSelection(modelName, type, mapId);
        }
        else
        {
            SetObjectModelForSelection(modelName, type, "");
        }
    }

    private void SetObjectModelForSelection(string modelName, FileSelectionType assetType, string assetMapId)
    {
        var actlist = new List<ViewportAction>();

        var selected = Editor.ViewportSelection.GetFilteredSelection<Entity>();

        foreach (var s in selected)
        {
            var isValidObjectType = false;

            if (assetType == FileSelectionType.Character)
            {
                switch (Editor.Project.ProjectType)
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
                    case ProjectType.ACFA:
                        if (s.WrappedObject is MSBFA.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ACV:
                        if (s.WrappedObject is MSBV.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ACVD:
                        if (s.WrappedObject is MSBVD.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    case ProjectType.NR:
                        if (s.WrappedObject is MSB_NR.Part.Enemy)
                            isValidObjectType = true;
                        break;
                    default:
                        throw new ArgumentException("Selected entity type must be Enemy");
                }
            }
            if (assetType == FileSelectionType.Asset)
            {
                switch (Editor.Project.ProjectType)
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
                    case ProjectType.NR:
                        if (s.WrappedObject is MSB_NR.Part.Asset)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ACFA:
                        if (s.WrappedObject is MSBFA.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ACV:
                        if (s.WrappedObject is MSBV.Part.Object)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ACVD:
                        if (s.WrappedObject is MSBVD.Part.Object)
                            isValidObjectType = true;
                        break;
                    default:
                        throw new ArgumentException("Selected entity type must be Object/Asset");
                }
            }
            if (assetType == FileSelectionType.MapPiece)
            {
                switch (Editor.Project.ProjectType)
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
                    case ProjectType.NR:
                        if (s.WrappedObject is MSB_NR.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ACFA:
                        if (s.WrappedObject is MSBFA.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ACV:
                        if (s.WrappedObject is MSBV.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    case ProjectType.ACVD:
                        if (s.WrappedObject is MSBVD.Part.MapPiece)
                            isValidObjectType = true;
                        break;
                    default:
                        throw new ArgumentException("Selected entity type must be MapPiece");
                }
            }

            if (assetType == FileSelectionType.MapPiece)
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
                if (assetType == FileSelectionType.MapPiece)
                {
                    // Adjust modelName for mappieces, since by default they are mXX_YY_ZZ_AA_<id>
                    string newName = modelName.Replace($"{assetMapId}_", "m");
                    modelName = newName;
                }

                // ModelName
                actlist.Add(s.ChangeObjectProperty("ModelName", modelName));

                if (CFG.Current.AssetBrowser_UpdateName)
                {
                    var updateNameAction = UpdateEntityName(modelName, s);
                    actlist.Add(updateNameAction);
                }

                if (CFG.Current.AssetBrowser_UpdateInstanceID)
                {
                    if (s.WrappedObject is MSBE.Part || s.WrappedObject is MSB_AC6.Part)
                    {
                        var updateInstanceAction = UpdateInstanceID(modelName, (MsbEntity)s);
                        actlist.Add(updateInstanceAction);
                    }
                }
            }
        }

        if (actlist.Any())
        {
            var action = new ViewportCompoundAction(actlist);
            Editor.EditorActionManager.ExecuteAction(action);
        }
    }

    private ViewportAction UpdateEntityName(string modelName, Entity ent)
    {
        var name = GetUniqueNameString(modelName);
        ent.Name = name;

        return ent.GetPropertyChangeAction("Name", name);
    }

    private ViewportAction UpdateInstanceID(string modelName, MsbEntity ent)
    {
        MapContainer m;
        m = Editor.Selection.GetMapContainerFromMapID(ent.MapID);

        Dictionary<MapContainer, HashSet<MsbEntity>> mapPartEntities = new();

        // ER
        if (ent.WrappedObject is MSBE.Part msbePart)
        {
            if (mapPartEntities.TryAdd(m, new HashSet<MsbEntity>()))
            {
                foreach (Entity tEnt in m.Objects)
                {
                    if (tEnt.WrappedObject != null && tEnt.WrappedObject is MSBE.Part)
                    {
                        mapPartEntities[m].Add((MsbEntity)tEnt);
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

            return ent.GetPropertyChangeAction("InstanceID", newInstanceID);
        }

        // AC6
        if (ent.WrappedObject is MSB_AC6.Part msb_ac6Part)
        {
            if (mapPartEntities.TryAdd(m, new HashSet<MsbEntity>()))
            {
                foreach (Entity tEnt in m.Objects)
                {
                    if (tEnt.WrappedObject != null && tEnt.WrappedObject is MSB_AC6.Part)
                    {
                        mapPartEntities[m].Add((MsbEntity)tEnt);
                    }
                }
            }

            var newInstanceID = 0; // Default start value

            while (mapPartEntities[m].FirstOrDefault(e =>
                       ((MSB_AC6.Part)e.WrappedObject).ModelName == msb_ac6Part.ModelName
                       && ((MSB_AC6.Part)e.WrappedObject).TypeIndex == newInstanceID
                       && msb_ac6Part != (MSB_AC6.Part)e.WrappedObject) != null)
            {
                newInstanceID++;
            }

            return ent.GetPropertyChangeAction("TypeIndex", newInstanceID);
        }

        return null;
    }

    public string GetUniqueNameString(string modelName)
    {
        var postfix = 0;
        var baseName = $"{modelName}_0000";

        var names = new List<string>();

        // Collect names
        foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer == null)
            {
                continue;
            }

            foreach (var ob in entry.Value.MapContainer.Objects)
            {
                if (ob is MsbEntity e)
                {
                    names.Add(ob.Name);
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

    private string PadNameString(int value)
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
