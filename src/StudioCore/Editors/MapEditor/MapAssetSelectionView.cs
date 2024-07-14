using ImGuiNET;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Banks.AliasBank;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor
{
    public class MapAssetSelectionView
    {
        private string _searchInput = "";
        private string _selectedEntry = "";
        private ModelSelectionType _selectedEntryType = ModelSelectionType.None;

        private MapEditorScreen Screen;

        public MapAssetSelectionView(MapEditorScreen screen)
        {
            Screen = screen;
        }

        public void OnProjectChanged()
        {
            if (Smithbox.ProjectType != ProjectType.Undefined)
            {
                _selectedEntry = "";
                _selectedEntryType = ModelSelectionType.None;
            }
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (Smithbox.ProjectType == ProjectType.Undefined)
                return;

            if (!Smithbox.AliasCacheHandler.AliasCache.UpdateCacheComplete)
                return;

            if (!CFG.Current.Interface_MapEditor_AssetBrowser)
                return;

            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Asset Browser##MapAssetBrowser"))
            {
                ImGui.InputText($"Search", ref _searchInput, 255);
                ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

                DisplayCharacterList();
                DisplayAssetList();
                DisplayPartList();
                DisplayMapPieceList();
            }

            ImGui.End();
            ImGui.PopStyleColor(1);
        }

        private bool FilterSelectionList(string name, Dictionary<string, AliasReference> referenceDict)
        {
            var lowerName = name.ToLower();

            var refName = "";
            var refTagList = new List<string>();

            if (referenceDict.ContainsKey(lowerName))
            {
                refName = referenceDict[lowerName].name;
                refTagList = referenceDict[lowerName].tags;
            }

            if (!CFG.Current.MapEditor_AssetBrowser_ShowLowDetailParts)
            {
                if (name.Substring(name.Length - 2) == "_l")
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

        private void DisplaySelectableAlias(string name, Dictionary<string, AliasReference> referenceDict)
        {
            var lowerName = name.ToLower();

            if (referenceDict.ContainsKey(lowerName))
            {
                if (CFG.Current.MapEditor_AssetBrowser_ShowAliases)
                {
                    var aliasName = referenceDict[lowerName].name;

                    AliasUtils.DisplayAlias(aliasName);
                }

                // Tags
                if (CFG.Current.MapEditor_AssetBrowser_ShowTags)
                {
                    var tagString = string.Join(" ", referenceDict[lowerName].tags);
                    AliasUtils.DisplayTagAlias(tagString);
                }
            }
        }

        private void DisplayCharacterList()
        {
            if (Smithbox.BankHandler.CharacterAliases.Aliases == null)
                return;

            if (ImGui.CollapsingHeader("Characters"))
            {
                foreach (var entry in Smithbox.AliasCacheHandler.AliasCache.CharacterList)
                {
                    if (FilterSelectionList(entry, Smithbox.AliasCacheHandler.AliasCache.Characters))
                    {
                        if (ImGui.Selectable(entry, entry == _selectedEntry, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            _selectedEntry = entry;
                            _selectedEntryType = ModelSelectionType.Character;

                            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                            {
                                ApplyMapAssetSelection(_selectedEntry, ModelSelectionType.Character);
                            }
                        }
                        DisplaySelectableAlias(entry, Smithbox.AliasCacheHandler.AliasCache.Characters);
                    }
                }
            }
        }

        private void DisplayAssetList()
        {
            if (Smithbox.BankHandler.AssetAliases.Aliases == null)
                return;

            var assetLabel = "Objects";

            if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                assetLabel = "Assets";
            }

            if (ImGui.CollapsingHeader(assetLabel))
            {
                foreach (var entry in Smithbox.AliasCacheHandler.AliasCache.AssetList)
                {
                    if (FilterSelectionList(entry, Smithbox.AliasCacheHandler.AliasCache.Assets))
                    {
                        if (ImGui.Selectable(entry, entry == _selectedEntry, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            _selectedEntry = entry;
                            _selectedEntryType = ModelSelectionType.Asset;

                            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                            {
                                ApplyMapAssetSelection(_selectedEntry, ModelSelectionType.Asset);
                            }
                        }
                        DisplaySelectableAlias(entry, Smithbox.AliasCacheHandler.AliasCache.Assets);
                    }
                }
            }
        }

        private void DisplayPartList()
        {
            if (Smithbox.BankHandler.PartAliases.Aliases == null)
                return;

            if (ImGui.CollapsingHeader("Parts"))
            {
                foreach (var entry in Smithbox.AliasCacheHandler.AliasCache.PartList)
                {
                    if (FilterSelectionList(entry, Smithbox.AliasCacheHandler.AliasCache.Parts))
                    {
                        if (ImGui.Selectable(entry, entry == _selectedEntry, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            _selectedEntry = entry;
                            _selectedEntryType = ModelSelectionType.Part;

                            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                            {
                                ApplyMapAssetSelection(_selectedEntry, ModelSelectionType.Part);
                            }
                        }
                        DisplaySelectableAlias(entry, Smithbox.AliasCacheHandler.AliasCache.Parts);
                    }
                }
            }
        }

        private void DisplayMapPieceList()
        {
            if (Smithbox.BankHandler.MapPieceAliases.Aliases == null)
                return;

            var maps = MapLocator.GetFullMapList();

            if (ImGui.CollapsingHeader("Map Pieces"))
            {
                foreach (var map in maps)
                {
                    var displayedMapName = $"{map} - {AliasUtils.GetMapNameAlias(map)}";

                    if (ImGui.CollapsingHeader($"{displayedMapName}"))
                    {
                        var displayedName = $"{map}";
                        var modelName = map.Replace($"{map}_", "m");
                        displayedName = $"{modelName}";

                        if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
                        {
                            displayedName = displayedName.Replace($"A{map.Substring(1, 2)}", "");
                        }

                        foreach (var entry in Smithbox.AliasCacheHandler.AliasCache.MapPieceDict[map])
                        {
                            var mapPieceName = $"{entry.Replace(map, "m")}";

                            if (ImGui.Selectable(mapPieceName, entry == _selectedEntry, ImGuiSelectableFlags.AllowDoubleClick))
                            {
                                _selectedEntry = entry;
                                _selectedEntryType = ModelSelectionType.MapPiece;

                                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                                {
                                    ApplyMapAssetSelection(_selectedEntry, ModelSelectionType.MapPiece, map);
                                }
                            }
                            DisplaySelectableAlias(entry, Smithbox.AliasCacheHandler.AliasCache.MapPieces);
                        }
                    }
                }
            }
        }
        private void ApplyMapAssetSelection(string _selectedName, ModelSelectionType type, string mapId = "")
        {
            var modelName = _selectedName;

            if (modelName.Contains("aeg"))
            {
                modelName = modelName.Replace("aeg", "AEG");
            }

            if (type == ModelSelectionType.MapPiece)
            {
                SetObjectModelForSelection(modelName, type, mapId);
            }
            else
            {
                SetObjectModelForSelection(modelName, type, "");
            }
        }

        private void SetObjectModelForSelection(string modelName, ModelSelectionType assetType, string assetMapId)
        {
            var actlist = new List<ViewportAction>();

            var selected = Screen._selection.GetFilteredSelection<Entity>();

            foreach (var s in selected)
            {
                var isValidObjectType = false;

                if (assetType == ModelSelectionType.Character)
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
                if (assetType == ModelSelectionType.Asset)
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
                if (assetType == ModelSelectionType.MapPiece)
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

                if (assetType == ModelSelectionType.MapPiece)
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
                    if (assetType == ModelSelectionType.MapPiece)
                    {
                        // Adjust modelName for mappieces, since by default they are mXX_YY_ZZ_AA_<id>
                        string newName = modelName.Replace($"{assetMapId}_", "m");
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
                Screen.EditorActionManager.ExecuteAction(action);
            }
        }

        public string GetUniqueNameString(string modelName)
        {
            var postfix = 0;
            var baseName = $"{modelName}_0000";

            var names = new List<string>();

            // Collect names
            foreach (var o in Screen.Universe.LoadedObjectContainers.Values)
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

        private void SetUniqueInstanceID(MsbEntity selected, string modelName)
        {
            MapContainer m;
            m = Screen.Universe.GetLoadedMap(selected.MapID);

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
}
