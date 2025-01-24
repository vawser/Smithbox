using DotNext.Collections.Generic;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Editors.TimeActEditor.Utils;
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
    public void DisplayRowContextMenu(bool isSelected, string key)
    {
        if (Smithbox.BankHandler.LightmapAtlasBank.IsSaving)
            return;

        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"RowContextMenu##RowContextMenu{key}"))
        {
            // Create
            if(ImGui.Selectable("Create"))
            {
                AtlasScreen.CreateEntryModal.Display();
            }

            // Duplicate
            if (ImGui.Selectable("Duplicate"))
            {
                LightmapAtlasUtils.DuplicateEntries(Screen, AtlasScreen);
            }

            // Delete
            if (ImGui.Selectable("Delete"))
            {
                LightmapAtlasUtils.DeleteEntries(Screen, AtlasScreen);
            }

            ImGui.EndPopup();
        }
    }
}
