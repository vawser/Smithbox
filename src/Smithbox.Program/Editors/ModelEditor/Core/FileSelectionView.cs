using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public class FileSelectionView
{
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    private ModelResourceManager ResManager;
    private ModelAssetCopyManager AssetCopyManager;

    public string _searchInput = "";

    public FileSelectionView(ModelEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
        ResManager = screen.ResManager;
        AssetCopyManager = screen.AssetCopyManager;
    }

    public void Display()
    {
        var scale = DPI.GetUIScale();

        if (!CFG.Current.Interface_ModelEditor_AssetBrowser)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Asset Browser##ModelAssetBrower"))
        {
            Selection.SwitchWindowContext(ModelEditorContext.File);

            ImGui.InputText($"Search", ref _searchInput, 255);
            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                _assetCacheDirty = true;
            }
            UIHelper.Tooltip("Separate terms are split via the + character.");

            ImGui.BeginChild("AssetBrowserLists");
            Selection.SwitchWindowContext(ModelEditorContext.File);

            DisplayLooseSection();
            DisplayCharacterList();
            DisplayAssetList();
            DisplayPartList();
            DisplayMapPieceList();

            ImGui.EndChild();
        }

        AssetCopyManager.CharacterCopyMenu();
        AssetCopyManager.AssetCopyMenu();
        AssetCopyManager.PartCopyMenu();
        AssetCopyManager.MapPieceCopyMenu();

        ImGui.End();

        ImGui.PopStyleColor(1);
    }


    private bool FilterSelectionList(string id, string name)
    {
        var lowerName = id.ToLower();

        var refName = "";
        var refTagList = new List<string>();

        refName = name;

        if (!CFG.Current.ModelEditor_AssetBrowser_ShowLowDetailParts)
        {
            if (id.Substring(id.Length - 2) == "_l")
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

    private void DisplaySelectableAlias(string id, string name)
    {
        var lowerName = id.ToLower();

        if (CFG.Current.ModelEditor_AssetBrowser_ShowAliases)
        {
            UIHelper.DisplayAlias(name);
        }
    }

    public void ReloadModel()
    {
        var name = Selection._selectedFileName;
        var mapId = Selection._selectedAssociatedMapID;

        switch (Selection._selectedFileModelType)
        {
            case FileSelectionType.Character:
                ResManager.LoadCharacter(name);
                break;
            case FileSelectionType.Enemy:
                ResManager.LoadEnemy(name);
                break;
            case FileSelectionType.Asset:
                ResManager.LoadAsset(name);
                break;
            case FileSelectionType.Part:
                ResManager.LoadPart(name);
                break;
            case FileSelectionType.MapPiece:
                ResManager.LoadMapPiece(name, mapId);
                break;
        }
    }

    private void DisplayLooseSection()
    {
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

                    Selection._selectedFileName = name;
                    Selection._selectedFileModelType = FileSelectionType.Loose;

                    ResManager.LoadLooseFLVER(Selection._selectedFileName, loosePath);
                }
            }
        }
    }

    private void DisplayCharacterList()
    {
        if (ImGui.CollapsingHeader("Characters"))
        {
            var filteredEntries = new List<(string Id, string AliasName)>();
            foreach (var entry in Editor.Project.VisualData.CharacterModels.Entries)
            {
                var id = entry.Filename;

                var aliasName = AliasUtils.GetAliasName(Editor.Project.Aliases.Characters, id);

                if (FilterSelectionList(id, aliasName))
                {
                    filteredEntries.Add((id, aliasName));
                }
            }

            var clipper = new ImGuiListClipper();
            clipper.Begin(filteredEntries.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var (id, aliasName) = filteredEntries[i];

                    if (ImGui.Selectable($"{id}##characterFileEntry{id}", id == Selection._selectedFileName, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Selection._selectedFileName = id;
                        Selection._selectedFileModelType = FileSelectionType.Character;

                        if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        {
                            ResManager.LoadCharacter(Selection._selectedFileName);
                        }
                    }

                    if (ImGui.IsItemVisible())
                    {
                        DisplaySelectableAlias(id, aliasName);
                    }

                    if (ImGui.BeginPopupContextItem($"CharacterModel_Context_{id}"))
                    {
                        if (AssetCopyManager.IsSupportedProjectType() && id != "c0000")
                        {
                            if (ImGui.Selectable("Copy as New Character"))
                            {
                                AssetCopyManager.OpenCharacterCopyMenu(id);
                            }
                        }

                        ImGui.EndPopup();
                    }
                }
            }

            clipper.End();
        }
    }


    private List<(string Id, string AliasName)> _cachedFilteredAssets = new();
    private bool _assetCacheDirty = true;

    private void RebuildAssetCache()
    {
        _cachedFilteredAssets.Clear();

        foreach (var entry in Editor.Project.VisualData.AssetModels.Entries)
        {
            var id = entry.Filename;

            var aliasName = AliasUtils.GetAliasName(Editor.Project.Aliases.Assets, id);

            if (FilterSelectionList(id, aliasName))
            {
                _cachedFilteredAssets.Add((id, aliasName));
            }
        }

        _assetCacheDirty = false;
    }

    private void DisplayAssetList()
    {
        var assetLabel = "Objects";
        if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            assetLabel = "Assets";
        }

        if (ImGui.CollapsingHeader(assetLabel))
        {
            if (_assetCacheDirty)
                RebuildAssetCache();

            var clipper = new ImGuiListClipper();
            clipper.Begin(_cachedFilteredAssets.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var (id, aliasName) = _cachedFilteredAssets[i];

                    if (ImGui.Selectable($"{id}##assetFileEntry{id}", id == Selection._selectedFileName, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Selection._selectedFileName = id;
                        Selection._selectedFileModelType = FileSelectionType.Asset;

                        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            ResManager.LoadAsset(Selection._selectedFileName);
                        }
                    }

                    if (ImGui.IsItemVisible())
                    {
                        DisplaySelectableAlias(id, aliasName);
                    }

                    if (ImGui.BeginPopupContextItem($"AssetModel_Context_{id}"))
                    {
                        if (AssetCopyManager.IsSupportedProjectType())
                        {
                            if (ImGui.Selectable("Copy as New Asset"))
                            {
                                AssetCopyManager.OpenAssetCopyMenu(id);
                            }
                        }

                        ImGui.EndPopup();
                    }
                }
            }

            clipper.End();
        }
    }

    private void DisplayPartList()
    {
        if (ImGui.CollapsingHeader("Parts"))
        {
            var filteredEntries = new List<(string Id, string AliasName)>();
            foreach (var entry in Editor.Project.VisualData.PartModels.Entries)
            {
                var id = entry.Filename;

                var aliasName = AliasUtils.GetAliasName(Editor.Project.Aliases.Parts, id);

                if (FilterSelectionList(id, aliasName))
                {
                    filteredEntries.Add((id, aliasName));
                }
            }

            var clipper = new ImGuiListClipper();
            clipper.Begin(filteredEntries.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var (id, aliasName) = filteredEntries[i];

                    if (ImGui.Selectable($"{id}##assetFileEntry{id}", id == Selection._selectedFileName, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Selection._selectedFileName = id;
                        Selection._selectedFileModelType = FileSelectionType.Part;

                        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            ResManager.LoadPart(Selection._selectedFileName);
                        }
                    }

                    if (ImGui.IsItemVisible())
                    {
                        DisplaySelectableAlias(id, aliasName);
                    }

                    if (ImGui.BeginPopupContextItem($"PartModel_Context_{id}"))
                    {
                        if (AssetCopyManager.IsSupportedProjectType())
                        {
                            if (ImGui.Selectable("Copy as New Part"))
                            {
                                AssetCopyManager.OpenPartCopyMenu(id);
                            }
                        }

                        ImGui.EndPopup();
                    }
                }
            }

            clipper.End();
        }
    }

    private void DisplayMapPieceList()
    {
        var maps = MapLocator.GetFullMapList(Editor.Project);

        if (ImGui.CollapsingHeader("Map Pieces"))
        {
            foreach (var map in maps)
            {
                MapPieceCollapsibleSection(map);
            }
        }
    }

    private void MapPieceCollapsibleSection(string map)
    {
        var displayedMapName = $"{map} - {AliasUtils.GetMapNameAlias(Editor.Project, map)}";

        if (ImGui.CollapsingHeader($"{displayedMapName}"))
        {
            var displayedName = map.Replace($"{map}_", "m");

            if (Editor.Project.ProjectType == ProjectType.DS1 || Editor.Project.ProjectType == ProjectType.DS1R)
            {
                displayedName = displayedName.Replace($"A{map.Substring(1, 2)}", "");
            }

            var filteredEntries = new List<(string Id, string AliasName, string DisplayName)>();
            foreach (var entry in Editor.Project.VisualData.MapPieceModels.Entries)
            {
                var id = entry.Filename;

                if (!id.StartsWith(map))
                    continue;

                var aliasName = AliasUtils.GetAliasName(Editor.Project.Aliases.MapPieces, id);

                var mapPieceName = id.Replace(map, "m");

                filteredEntries.Add((id, aliasName, mapPieceName));
            }

            var clipper = new ImGuiListClipper();
            clipper.Begin(filteredEntries.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var (id, aliasName, mapPieceName) = filteredEntries[i];

                    if (ImGui.Selectable(mapPieceName, id == Selection._selectedFileName, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Selection._selectedFileName = id;
                        Selection._selectedFileModelType = FileSelectionType.MapPiece;

                        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            Selection._selectedAssociatedMapID = map;
                            ResManager.LoadMapPiece(Selection._selectedFileName, map);
                        }
                    }

                    if (ImGui.IsItemVisible())
                    {
                        DisplaySelectableAlias(id, aliasName);
                    }

                    if (ImGui.BeginPopupContextItem($"MapPieceModel_Context_{id}"))
                    {
                        /*
                        if (AssetCopyManager.IsSupportedProjectType())
                        {
                            if (ImGui.Selectable("Copy as New Map Piece"))
                            {
                                AssetCopyManager.OpenMapPieceCopyMenu(entry);
                            }
                        }
                        */
                        ImGui.EndPopup();
                    }
                }
            }

            clipper.End();
        }
    }
}
