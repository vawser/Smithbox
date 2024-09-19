using ImGuiNET;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.TimeActEditor.Bank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.LightmapAtlasEditor;

public class LightmapAtlasContextMenu
{
    private MapEditorScreen Screen;
    private LightmapAtlasScreen AtlasScreen;

    public LightmapAtlasContextMenu(MapEditorScreen screen, LightmapAtlasScreen atlasScreen)
    {
        Screen = screen;
        AtlasScreen = atlasScreen;
    }

    /// <summary>
    /// Context menu for the Entry List
    /// </summary>
    public void EntryListMenu(bool isSelected, string key)
    {
        if (Smithbox.BankHandler.LightmapAtlasBank.IsSaving)
            return;

        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"EntryContextMenu##EntryContextMenu{key}"))
        {
            // Create

            // Duplicate

            // Delete

            ImGui.EndPopup();
        }
    }
}
