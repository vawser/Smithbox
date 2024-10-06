using HKLib.hk2018.hk;
using ImGuiNET;
using Microsoft.Extensions.FileSystemGlobbing;
using SoulsFormats.KF4;
using StudioCore.Banks.AliasBank;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public class FileSelectionView
{
    private ModelEditorScreen Screen;
    private ModelResourceManager ResManager;
    private ModelAssetCopyManager AssetCopyManager;

    public string _searchInput = "";
    public string _selectedEntry = "";
    private string _selectedMapId = "";
    private ModelSelectionType _selectedEntryType = ModelSelectionType.None;

    public FileSelectionView(ModelEditorScreen screen)
    {
        Screen = screen;
        ResManager = screen.ResManager;
        AssetCopyManager = screen.AssetCopyManager;
    }

    public void OnProjectChanged()
    {
        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            _selectedEntry = "";
            _selectedEntryType = ModelSelectionType.None;
        }
    }

    public void Display()
    {
        var scale = DPI.GetUIScale();

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (!Smithbox.AliasCacheHandler.AliasCache.UpdateCacheComplete)
            return;

        if (!UI.Current.Interface_ModelEditor_AssetBrowser)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Asset Browser##ModelAssetBrower"))
        {
            ImGui.InputText($"Search", ref _searchInput, 255);
            UIHelper.ShowHoverTooltip("Separate terms are split via the + character.");

            DisplayLooseSection();
            DisplayCharacterList();
            DisplayAssetList();
            DisplayPartList();
            DisplayMapPieceList();
        }

        AssetCopyManager.CharacterCopyMenu();
        AssetCopyManager.AssetCopyMenu();
        AssetCopyManager.PartCopyMenu();

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

        if (!CFG.Current.ModelEditor_AssetBrowser_ShowLowDetailParts)
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
            if (CFG.Current.ModelEditor_AssetBrowser_ShowAliases)
            {
                var aliasName = referenceDict[lowerName].name;

                UIHelper.DisplayAlias(aliasName);
            }

            // Tags
            if (CFG.Current.ModelEditor_AssetBrowser_ShowTags)
            {
                var tagString = string.Join(" ", referenceDict[lowerName].tags);
                AliasUtils.DisplayTagAlias(tagString);
            }
        }
    }

    public void ReloadModel()
    {
        switch (_selectedEntryType)
        {
            case ModelSelectionType.Character:
                ResManager.LoadCharacter(_selectedEntry);
                break;
            case ModelSelectionType.Asset:
                ResManager.LoadAsset(_selectedEntry);
                break;
            case ModelSelectionType.Part:
                ResManager.LoadPart(_selectedEntry);
                break;
            case ModelSelectionType.MapPiece:
                ResManager.LoadMapPiece(_selectedEntry, _selectedMapId);
                break;
        }
    }

    private string loosePath = "";

    private void DisplayLooseSection()
    {
        if (Smithbox.BankHandler.CharacterAliases.Aliases == null)
            return;

        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        if (ImGui.CollapsingHeader("Loose"))
        {
            if (ImGui.Button("Load Loose FLVER", defaultButtonSize))
            {
                var result = PlatformUtils.Instance.OpenFileDialog("Select loose FLVER...", new string[] { "png", "flver", "flv" }, out var loosePath);

                if (result)
                {
                    var name = Path.GetFileNameWithoutExtension(loosePath);
                    _selectedEntry = name;
                    _selectedEntryType = ModelSelectionType.Loose;
                    ResManager.LoadLooseFLVER(_selectedEntry, loosePath);
                }
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

                        if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        {
                            ResManager.LoadCharacter(_selectedEntry);
                        }
                    }

                    if (ImGui.IsItemVisible())
                    {
                        DisplaySelectableAlias(entry, Smithbox.AliasCacheHandler.AliasCache.Characters);
                    }

                    if (ImGui.BeginPopupContextItem($"CharacterModel_Context_{entry}"))
                    {
                        if (AssetCopyManager.IsSupportedProjectType() && entry != "c0000")
                        {
                            if (ImGui.Selectable("Copy as New Character"))
                            {
                                AssetCopyManager.OpenCharacterCopyMenu(entry);
                            }
                        }

                        if (ImGui.Selectable("Go to Alias"))
                        {
                            if (!Smithbox.WindowHandler.SettingsWindow.MenuOpenState)
                            {
                                Smithbox.WindowHandler.SettingsWindow.ToggleMenuVisibility();
                            }

                            Smithbox.WindowHandler.SettingsWindow.DisplayCharacterTab = true;
                            Smithbox.WindowHandler.SettingsWindow.TargetChrID = entry;
                        }

                        ImGui.EndPopup();
                    }
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

                        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            ResManager.LoadAsset(_selectedEntry);
                        }
                    }
                    if (ImGui.IsItemVisible())
                    {
                        DisplaySelectableAlias(entry, Smithbox.AliasCacheHandler.AliasCache.Assets);
                    }

                    if (ImGui.BeginPopupContextItem($"AssetModel_Context_{entry}"))
                    {
                        if (AssetCopyManager.IsSupportedProjectType())
                        {
                            if (ImGui.Selectable("Copy as New Asset"))
                            {
                                AssetCopyManager.OpenAssetCopyMenu(entry);
                            }
                        }

                        if (ImGui.Selectable("Go to Alias"))
                        {
                            if (!Smithbox.WindowHandler.SettingsWindow.MenuOpenState)
                            {
                                Smithbox.WindowHandler.SettingsWindow.ToggleMenuVisibility();
                            }

                            Smithbox.WindowHandler.SettingsWindow.DisplayAssetTab = true;
                            Smithbox.WindowHandler.SettingsWindow.TargetAssetID = entry;
                        }

                        ImGui.EndPopup();
                    }
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

                        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            ResManager.LoadPart(_selectedEntry);
                        }
                    }
                    if (ImGui.IsItemVisible())
                    {
                        DisplaySelectableAlias(entry, Smithbox.AliasCacheHandler.AliasCache.Parts);
                    }

                    if (ImGui.BeginPopupContextItem($"PartModel_Context_{entry}"))
                    {
                        if (AssetCopyManager.IsSupportedProjectType())
                        {
                            if (ImGui.Selectable("Copy as New Part"))
                            {
                                AssetCopyManager.OpenPartCopyMenu(entry);
                            }
                        }

                        if (ImGui.Selectable("Go to Alias"))
                        {
                            if (!Smithbox.WindowHandler.SettingsWindow.MenuOpenState)
                            {
                                Smithbox.WindowHandler.SettingsWindow.ToggleMenuVisibility();
                            }

                            Smithbox.WindowHandler.SettingsWindow.DisplayPartTab = true;
                            Smithbox.WindowHandler.SettingsWindow.TargetPartID = entry;
                        }

                        ImGui.EndPopup();
                    }
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
                if (Smithbox.AliasCacheHandler.AliasCache.MapPieceDict.ContainsKey(map))
                {
                    if (Smithbox.AliasCacheHandler.AliasCache.MapPieceDict[map].Count > 0)
                    {
                        MapPieceCollapsibleSection(map);
                    }
                }
            }
        }
    }

    private void MapPieceCollapsibleSection(string map)
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

            if (Smithbox.AliasCacheHandler.AliasCache.MapPieceDict.ContainsKey(map))
            {
                foreach (var entry in Smithbox.AliasCacheHandler.AliasCache.MapPieceDict[map])
                {
                    var mapPieceName = $"{entry.Replace(map, "m")}";

                    if (ImGui.Selectable(mapPieceName, entry == _selectedEntry, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _selectedEntry = entry;
                        _selectedEntryType = ModelSelectionType.MapPiece;

                        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            _selectedMapId = map;
                            ResManager.LoadMapPiece(_selectedEntry, map);
                        }
                    }
                    if (ImGui.IsItemVisible())
                    {
                        DisplaySelectableAlias(entry, Smithbox.AliasCacheHandler.AliasCache.MapPieces);
                    }

                    if (ImGui.BeginPopupContextItem($"MapPieceModel_Context_{entry}"))
                    {
                        if (ImGui.Selectable("Go to Alias"))
                        {
                            if (!Smithbox.WindowHandler.SettingsWindow.MenuOpenState)
                            {
                                Smithbox.WindowHandler.SettingsWindow.ToggleMenuVisibility();
                            }

                            Smithbox.WindowHandler.SettingsWindow.DisplayMapPieceTab = true;
                            Smithbox.WindowHandler.SettingsWindow.TargetMapPieceID = entry;
                        }

                        ImGui.EndPopup();
                    }
                }
            }
        }
    }
}
