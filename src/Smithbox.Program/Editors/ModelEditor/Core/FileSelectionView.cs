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


    private bool FilterSelectionList(AliasEntry entry)
    {
        var lowerName = entry.ID.ToLower();

        var refName = "";
        var refTagList = new List<string>();

        refName = entry.Name;
        refTagList = entry.Tags;

        if (!CFG.Current.ModelEditor_AssetBrowser_ShowLowDetailParts)
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

        if (CFG.Current.ModelEditor_AssetBrowser_ShowAliases)
        {
            var aliasName = entry.Name;

            UIHelper.DisplayAlias(aliasName);
        }

        // Tags
        if (CFG.Current.ModelEditor_AssetBrowser_ShowTags)
        {
            var tagString = string.Join(" ", entry.Tags);
            AliasUtils.DisplayTagAlias(tagString);
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
            foreach (var entry in Editor.Project.Aliases.Characters)
            {
                if (FilterSelectionList(entry))
                {
                    if (ImGui.Selectable(entry.ID, entry.ID == Selection._selectedFileName, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Selection._selectedFileName = entry.ID;
                        Selection._selectedFileModelType = FileSelectionType.Character;

                        if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        {
                            ResManager.LoadCharacter(Selection._selectedFileName);
                        }
                    }

                    if (ImGui.IsItemVisible())
                    {
                        DisplaySelectableAlias(entry);
                    }

                    if (ImGui.BeginPopupContextItem($"CharacterModel_Context_{entry.ID}"))
                    {
                        if (AssetCopyManager.IsSupportedProjectType() && entry.ID != "c0000")
                        {
                            if (ImGui.Selectable("Copy as New Character"))
                            {
                                AssetCopyManager.OpenCharacterCopyMenu(entry.ID);
                            }
                        }

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
                    if (ImGui.Selectable(entry.ID, entry.ID == Selection._selectedFileName, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Selection._selectedFileName = entry.ID;
                        Selection._selectedFileModelType = FileSelectionType.Asset;

                        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            ResManager.LoadAsset(Selection._selectedFileName);
                        }
                    }
                    if (ImGui.IsItemVisible())
                    {
                        DisplaySelectableAlias(entry);
                    }

                    if (ImGui.BeginPopupContextItem($"AssetModel_Context_{entry.ID}"))
                    {
                        if (AssetCopyManager.IsSupportedProjectType())
                        {
                            if (ImGui.Selectable("Copy as New Asset"))
                            {
                                AssetCopyManager.OpenAssetCopyMenu(entry.ID);
                            }
                        }

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
                    if (ImGui.Selectable(entry.ID, entry.ID == Selection._selectedFileName, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Selection._selectedFileName = entry.ID;
                        Selection._selectedFileModelType = FileSelectionType.Part;

                        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            ResManager.LoadPart(Selection._selectedFileName);
                        }
                    }
                    if (ImGui.IsItemVisible())
                    {
                        DisplaySelectableAlias(entry);
                    }

                    if (ImGui.BeginPopupContextItem($"PartModel_Context_{entry.ID}"))
                    {
                        if (AssetCopyManager.IsSupportedProjectType())
                        {
                            if (ImGui.Selectable("Copy as New Part"))
                            {
                                AssetCopyManager.OpenPartCopyMenu(entry.ID);
                            }
                        }

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
                MapPieceCollapsibleSection(map);
            }
        }
    }

    private void MapPieceCollapsibleSection(string map)
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

                if (ImGui.Selectable(mapPieceName, entry.ID == Selection._selectedFileName, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    Selection._selectedFileName = entry.ID;
                    Selection._selectedFileModelType = FileSelectionType.MapPiece;

                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        Selection._selectedAssociatedMapID = map;
                        ResManager.LoadMapPiece(Selection._selectedFileName, map);
                    }
                }
                if (ImGui.IsItemVisible())
                {
                    DisplaySelectableAlias(entry);
                }

                if (ImGui.BeginPopupContextItem($"MapPieceModel_Context_{entry.ID}"))
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
    }
}
