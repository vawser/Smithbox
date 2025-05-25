using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor.Tools.AssetBrowser
{
    public class AssetBrowserView
    {
        private string _searchInput = "";
        private string _selectedEntry = "";

        private MapEditorScreen Editor;

        public AssetBrowserView(MapEditorScreen screen)
        {
            Editor = screen;
        }

        public void OnGui()
        {
            var scale = DPI.GetUIScale();

            if (!CFG.Current.Interface_MapEditor_AssetBrowser)
                return;

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Asset Browser##MapAssetBrowser"))
            {
                Editor.FocusManager.SwitchWindowContext(MapEditorContext.AssetBrowser);

                ImGui.InputText($"Search", ref _searchInput, 255);
                UIHelper.Tooltip("Separate terms are split via the + character.");

                ImGui.Checkbox("Update Name on Switch", ref CFG.Current.AssetBrowser_UpdateName);
                UIHelper.Tooltip("When a map object is switched to a new form, update the name to match the new form.");

                if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6)
                {
                    ImGui.Checkbox("Update Instance ID on Switch", ref CFG.Current.AssetBrowser_UpdateInstanceID);
                    UIHelper.Tooltip("When a map object is switched to a new form, update the Instance ID to account for the new form.");
                }

                DisplayCharacterList();
                DisplayAssetList();
                DisplayPartList();
                DisplayMapPieceList();
            }

            ImGui.End();
            ImGui.PopStyleColor(1);
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
                AliasUtils.DisplayTagAlias(tagString);
            }
        }

        private void DisplayCharacterList()
        {
            if (ImGui.CollapsingHeader("Characters"))
            {
                foreach (var entry in Editor.Project.Aliases.Characters)
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
                        DisplaySelectableAlias(entry);

                        if (ImGui.BeginPopupContextItem($"CharacterModel_Context_{entry.ID}"))
                        {
                            if (ImGui.Selectable("Apply"))
                            {
                                ApplyMapAssetSelection(entry.ID, FileSelectionType.Character);
                            }
                            UIHelper.Tooltip("Change your current selection's model to this.");

                            ImGui.EndPopup();
                        }
                    }
                }
            }
        }

        private void DisplayAssetList()
        {
            var assetLabel = "Objects";

            if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                assetLabel = "Assets";
            }

            if (ImGui.CollapsingHeader(assetLabel))
            {
                foreach (var entry in Editor.Project.Aliases.Assets)
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
                        DisplaySelectableAlias(entry);

                        if (ImGui.BeginPopupContextItem($"AssetModel_Context_{entry.ID}"))
                        {
                            if (ImGui.Selectable("Apply"))
                            {
                                ApplyMapAssetSelection(entry.ID, FileSelectionType.Asset);
                            }
                            UIHelper.Tooltip("Change your current selection's model to this.");

                            ImGui.EndPopup();
                        }
                    }
                }
            }
        }

        private void DisplayPartList()
        {
            if (ImGui.CollapsingHeader("Parts"))
            {
                foreach (var entry in Editor.Project.Aliases.Parts)
                {
                    if (FilterSelectionList(entry))
                    {
                        if (ImGui.Selectable(entry.ID, entry.ID == _selectedEntry, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            _selectedEntry = entry.ID;

                            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                            {
                                ApplyMapAssetSelection(_selectedEntry, FileSelectionType.Part);
                            }
                        }
                        DisplaySelectableAlias(entry);

                        if (ImGui.BeginPopupContextItem($"PartModel_Context_{entry.ID}"))
                        {
                            if (ImGui.Selectable("Apply"))
                            {
                                ApplyMapAssetSelection(entry.ID, FileSelectionType.Part);
                            }
                            UIHelper.Tooltip("Change your current selection's model to this.");

                            ImGui.EndPopup();
                        }
                    }
                }
            }
        }

        private void DisplayMapPieceList()
        {
            var maps = MapLocator.GetFullMapList(Editor.Project);

            if (ImGui.CollapsingHeader("Map Pieces"))
            {
                foreach (var map in maps)
                {
                    var displayedMapName = $"{map} - {AliasUtils.GetMapNameAlias(Editor.Project, map)}";

                    if (ImGui.CollapsingHeader($"{displayedMapName}"))
                    {
                        var displayedName = $"{map}";
                        var modelName = map.Replace($"{map}_", "m");
                        displayedName = $"{modelName}";

                        if (Editor.Project.ProjectType == ProjectType.DS1 || Editor.Project.ProjectType == ProjectType.DS1R)
                        {
                            displayedName = displayedName.Replace($"A{map.Substring(1, 2)}", "");
                        }

                        foreach (var entry in Editor.Project.Aliases.MapPieces)
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
                            DisplaySelectableAlias(entry);

                            if (ImGui.BeginPopupContextItem($"MapPieceModel_Context_{entry.ID}"))
                            {
                                if (ImGui.Selectable("Apply"))
                                {
                                    ApplyMapAssetSelection(entry.ID, FileSelectionType.MapPiece, map);
                                }
                                UIHelper.Tooltip("Change your current selection's model to this.");

                                ImGui.EndPopup();
                            }
                        }
                    }
                }
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
                var action = new Actions.Viewport.CompoundAction(actlist);
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
            m = Editor.GetMapContainerFromMapID(ent.MapID);

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
}
