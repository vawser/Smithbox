using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.EntryFileList;
using StudioCore.MsbEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.LightmapAtlasEditor;

public class LightmapAtlasScreen
{
    private LightmapAtlasPropertyEditor PropertyEditor;

    private bool DisplayScreen = false;

    public LightmapAtlasScreen()
    {
        PropertyEditor = new LightmapAtlasPropertyEditor();
    }

    public void ToggleDisplay()
    {
        DisplayScreen = !DisplayScreen;
    }

    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();

        if (!Smithbox.BankHandler.LightmapAtlasBank.UsesLightmapAtlases())
            return;

        if (!DisplayScreen)
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

    public static LightmapAtlasInfo _selectedParentEntry;
    public static BTAB.Entry _selectedEntry;

    private void DisplaySelectionList(MapContainer map, KeyValuePair<string, List<LightmapAtlasInfo>> lightMapAtlases)
    {
        var index = 0;

        foreach (var entry in lightMapAtlases.Value)
        {
            foreach (var atlasEntry in entry.LightmapAtlas.Entries)
            {
                if(ImGui.Selectable($"{atlasEntry.PartName}##{index}_Select", _selectedEntry == atlasEntry))
                {
                    _selectedParentEntry = entry;
                    _selectedEntry = atlasEntry;
                }

                index++;
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

            }
        }
    }

    private void DisplayActionSection(MapContainer map, KeyValuePair<string, List<LightmapAtlasInfo>> lightMapAtlases)
    {
        var widthUnit = ImGui.GetWindowWidth() / 100;

        if (ImGui.Button("Add New Entry", new Vector2(widthUnit * 100, 32)))
        {

        }
    }
}
