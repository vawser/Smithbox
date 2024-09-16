using ImGuiNET;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor.LightmapAtlasEditor;

public class LightmapAtlasScreen
{
    private MapEditorScreen Screen;
    private LightmapAtlasPropertyEditor PropertyEditor;

    public LightmapAtlasInfo _selectedParentEntry;
    public int _selectedEntryKey;
    public BTAB.Entry _selectedEntry;

    public LightmapMultiselect LightmapMultiselect;

    private string _searchInput = "";
    private bool SelectEntry = false;

    public LightmapAtlasScreen(MapEditorScreen screen)
    {
        Screen = screen;
        PropertyEditor = new LightmapAtlasPropertyEditor(this);
        LightmapMultiselect = new LightmapMultiselect(this);
    }

    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();

        if (Smithbox.BankHandler == null)
            return;

        if (Smithbox.BankHandler.LightmapAtlasBank == null)
            return;

        if (!Smithbox.BankHandler.LightmapAtlasBank.UsesLightmapAtlases())
            return;

        if (!CFG.Current.Interface_MapEditor_Viewport_LightmapAtlas)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Lightmap Atlas Editor##MapEditor_LightmapAtlasEditor"))
        {
            ImGui.BeginTabBar("##LightmapAtlasMaps");

            var loadedMaps = Smithbox.EditorHandler.MapEditor.Universe.GetLoadedMaps();

            foreach (var entry in Smithbox.BankHandler.LightmapAtlasBank.LightmapAtlases)
            {
                foreach(var map in loadedMaps)
                {
                    if(map.RootObject.Name == entry.Key)
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

    public void DisplayLightmapAtlasForMap(MapContainer map, KeyValuePair<string, List<LightmapAtlasInfo>> lightMapAtlases)
    {
        // Tab by opened maps

        // In each tab, columns:
        // First column: list of the PartNames (with internal index for order)
        // Second column: properties to edit for selected Part (watch out for PartName change)
        // Search bar to filter 1st column
        // Add New Entry button to side

        // Assistance:
        // Alias support for PartName and Map Name
        // Quick-link to frame/view Part (if it exists)

        // Content side:
        // Property editor for the properties
        // Delete Selected Entry button
        // Duplicate Selected entry button

        DisplayActionSection(map, lightMapAtlases);

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
    private void DisplaySelectionList(MapContainer map, KeyValuePair<string, List<LightmapAtlasInfo>> lightMapAtlases)
    {
        foreach (var entry in lightMapAtlases.Value)
        {
            for (int i = 0; i < entry.LightmapAtlas.Entries.Count; i++)
            {
                var lightmapEntry = entry.LightmapAtlas.Entries[i];

                if (SearchFilters.IsBasicMatch(_searchInput, lightmapEntry.PartName))
                {
                    var isSelected = false;
                    if (i == _selectedEntryKey || LightmapMultiselect.IsLightmapSelected(i))
                    {
                        isSelected = true;
                    }

                    // Row Select
                    if (ImGui.Selectable($"{lightmapEntry.PartName}##{i}_Select", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        LightmapMultiselect.LightMapSelect(_selectedEntryKey, i);

                        _selectedParentEntry = entry;
                        _selectedEntryKey = i;
                        _selectedEntry = lightmapEntry;

                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            EditorCommandQueue.AddCommand($"map/select/{map.Name}/{lightmapEntry.PartName}");
                        }
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && SelectEntry)
                    {
                        LightmapMultiselect.LightMapSelect(_selectedEntryKey, i);

                        _selectedParentEntry = entry;
                        _selectedEntryKey = i;
                        _selectedEntry = lightmapEntry;
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        SelectEntry = true;
                    }

                    if (ImGui.IsItemVisible())
                    {
                        DisplaySelectableAlias(lightmapEntry.PartName, Smithbox.AliasCacheHandler.AliasCache.MapPieces);
                    }
                }
            }
            ImGui.Separator();
        }
    }

    private void DisplayPropertyPanel(MapContainer map, KeyValuePair<string, List<LightmapAtlasInfo>> lightMapAtlases)
    {
        var widthUnit = ImGui.GetWindowWidth() / 100;

        if (_selectedEntry != null)
        {
            ImGui.AlignTextToFramePadding();

            ImGui.Text("Atlas ID");
            PropertyEditor.AtlasID(_selectedEntry, 1);
            ImGui.Text("Part Name");
            PropertyEditor.PartName(_selectedEntry, 2);
            ImGui.Text("Material Name");
            PropertyEditor.MaterialName(_selectedEntry, 3);
            ImGui.Text("UV Offset");
            PropertyEditor.UVOffset(_selectedEntry, 4);
            ImGui.Text("UV Scale");
            PropertyEditor.UVScale(_selectedEntry, 5);

            ImGui.Separator();

            if (ImGui.Button("Delete Entry", new Vector2(widthUnit * 100, 32)))
            {
                foreach(var entry in LightmapMultiselect.StoredLightmapEntries)
                {
                    _selectedParentEntry.LightmapAtlas.Entries.RemoveAt(entry.Key);
                }

                LightmapMultiselect.Reset();
            }
        }
    }

    private void DisplayActionSection(MapContainer map, KeyValuePair<string, List<LightmapAtlasInfo>> lightMapAtlases)
    {
        var widthUnit = ImGui.GetWindowWidth() / 100;

        if (ImGui.Button("Add New Entry", new Vector2(widthUnit * 100, 32)))
        {
            // TODO: add
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
}
