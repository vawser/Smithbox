using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor.Tools.LightmapAtlasEditor;

public class LightmapAtlasView
{
    private MapEditorScreen Screen;
    private LightmapAtlasPropertyEditor PropertyEditor;

    public AtlasContainerInfo CurrentParent;
    public int CurrentEntryKey;
    public BTAB.Entry CurrentEntry;

    public LightmapMultiselect LightmapMultiselect;
    public LightmapAtlasContextMenu ContextMenu;

    public LightmapAtlasEntryModal CreateEntryModal;

    private string _searchInput = "";
    private bool SelectEntry = false;

    public LightmapAtlasView(MapEditorScreen screen)
    {
        Screen = screen;
        PropertyEditor = new LightmapAtlasPropertyEditor(Screen, this);
        LightmapMultiselect = new LightmapMultiselect(this);
        ContextMenu = new LightmapAtlasContextMenu(Screen, this);
        CreateEntryModal = new LightmapAtlasEntryModal(Screen, this);
    }

    public void Save()
    {
        foreach (KeyValuePair<string, ObjectContainer> m in Screen.Universe.LoadedObjectContainers)
        {
            Smithbox.BankHandler.LightmapAtlasBank.SaveBank(m.Key);
        }
    }

    public void OnGui()
    {
        var scale = DPI.GetUIScale();

        if (Smithbox.BankHandler == null)
            return;

        if (Smithbox.BankHandler.LightmapAtlasBank == null)
            return;

        if (!Smithbox.BankHandler.LightmapAtlasBank.UsesLightmapAtlases())
            return;

        if (!UI.Current.Interface_MapEditor_Viewport_LightmapAtlas)
            return;

        CreateEntryModal.OnGui();

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Lightmap Atlas Editor##MapEditor_LightmapAtlasEditor"))
        {
            ImGui.BeginTabBar("##LightmapAtlasMaps");

            var loadedMaps = Smithbox.EditorHandler.MapEditor.Universe.GetLoadedMapContainerList();

            foreach (var entry in Smithbox.BankHandler.LightmapAtlasBank.LightmapAtlases)
            {
                foreach (var map in loadedMaps)
                {
                    if (map.RootObject.Name == entry.Key)
                    {
                        if (ImGui.BeginTabItem(entry.Key))
                        {
                            DisplayLightmapAtlasForMap(map, entry);

                            ImGui.EndTabItem();
                        }
                    }
                }
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    public void DisplayLightmapAtlasForMap(MapContainer map, KeyValuePair<string, List<AtlasContainerInfo>> lightMapAtlases)
    {
        ImGui.Columns(2);

        ImGui.BeginChild("SelectionCol");

        DisplaySelectionList(map, lightMapAtlases);

        ImGui.EndChild();

        ImGui.NextColumn();

        ImGui.BeginChild("ActionCol");

        DisplayPropertyPanel(map, lightMapAtlases);

        ImGui.EndChild();

        ImGui.Columns(1);
    }

    private int _selectedConfigTab = -1;

    private void DisplaySelectionList(MapContainer map, KeyValuePair<string, List<AtlasContainerInfo>> lightMapAtlases)
    {
        ImGui.BeginTabBar("##lightmapAtlasMaterialConfigs");

        for (int k = 0; k < lightMapAtlases.Value.Count; k++)
        {
            var entry = lightMapAtlases.Value[k];

            if (CurrentParent == null)
                CurrentParent = entry;

            if (ImGui.BeginTabItem($"Material Config {k}"))
            {
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    if (_selectedConfigTab != k)
                    {
                        _selectedConfigTab = k;
                        LightmapMultiselect.Reset();
                    }
                }

                var width = ImGui.GetWindowWidth();
                var buttonSize = new Vector2(width / 2, 24);

                if (ImGui.Button("Add Entry", buttonSize))
                {
                    CreateEntryModal.Display();
                }
                UIHelper.ShowHoverTooltip("Add new entry with default values.");
                ImGui.SameLine();
                if (ImGui.Button("Clear Entries", buttonSize))
                {
                    CurrentParent = entry;
                    LightmapMultiselect.StoredEntries.Clear();

                    for (int i = 0; i < entry.LightmapAtlas.Entries.Count; i++)
                    {
                        var atlasEntry = entry.LightmapAtlas.Entries[i];

                        LightmapMultiselect.StoredEntries.Add(i, atlasEntry);
                    }

                    LightmapAtlasUtils.DeleteEntries(Screen, this);
                }
                UIHelper.ShowHoverTooltip("Remove all entries.");

                ImGui.InputText($"Search##entryFilter", ref _searchInput, 255);
                UIHelper.ShowHoverTooltip("Separate terms are split via the + character.");

                for (int i = 0; i < entry.LightmapAtlas.Entries.Count; i++)
                {
                    var lightmapEntry = entry.LightmapAtlas.Entries[i];

                    if (SearchFilters.IsBasicMatch(_searchInput, lightmapEntry.PartName))
                    {
                        var isSelected = false;
                        if (i == CurrentEntryKey || LightmapMultiselect.IsSelected(i))
                        {
                            isSelected = true;
                        }

                        // Row Select
                        if (ImGui.Selectable($"{lightmapEntry.PartName}##{i}_Select", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            LightmapMultiselect.HandleSelection(CurrentEntryKey, i, lightmapEntry);

                            CurrentParent = entry;
                            CurrentEntryKey = i;
                            CurrentEntry = lightmapEntry;

                            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                            {
                                EditorCommandQueue.AddCommand($"map/select/{map.Name}/{lightmapEntry.PartName}");
                            }
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && SelectEntry)
                        {
                            LightmapMultiselect.HandleSelection(CurrentEntryKey, i, lightmapEntry);

                            CurrentParent = entry;
                            CurrentEntryKey = i;
                            CurrentEntry = lightmapEntry;
                        }
                        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                        {
                            SelectEntry = true;
                        }

                        if (ImGui.IsItemVisible())
                        {
                            DisplaySelectableAlias(lightmapEntry.PartName, Smithbox.AliasCacheHandler.AliasCache.MapPieces);
                        }

                        if (CurrentParent == entry && LightmapMultiselect.IsCurrentSelection(i))
                        {
                            ContextMenu.DisplayRowContextMenu(isSelected, $"{CurrentEntryKey}");
                        }
                    }
                }

                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();
    }

    private void DisplayPropertyPanel(MapContainer map, KeyValuePair<string, List<AtlasContainerInfo>> lightMapAtlases)
    {
        var widthUnit = ImGui.GetWindowWidth() / 100;

        if (CurrentEntry != null)
        {
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Atlas ID");
            PropertyEditor.AtlasID(CurrentEntry, 1);
            ImGui.Text("Part Name");
            PropertyEditor.PartName(CurrentEntry, 2);
            ImGui.Text("Material Name");
            PropertyEditor.MaterialName(CurrentEntry, 3);
            ImGui.Text("UV Offset");
            PropertyEditor.UVOffset(CurrentEntry, 4);
            ImGui.Text("UV Scale");
            PropertyEditor.UVScale(CurrentEntry, 5);
        }
    }

    private void DisplaySelectableAlias(string name, Dictionary<string, AliasReference> referenceDict)
    {
        var lowerName = name.ToLower();

        if (referenceDict.ContainsKey(lowerName))
        {
            if (CFG.Current.MapEditor_AssetBrowser_ShowAliases)
            {
                var aliasName = referenceDict[lowerName].name;

                UIHelper.DisplayAlias(aliasName);
            }

            // Tags
            if (CFG.Current.MapEditor_AssetBrowser_ShowTags)
            {
                var tagString = string.Join(" ", referenceDict[lowerName].tags);
                AliasUtils.DisplayTagAlias(tagString);
            }
        }
    }
}
