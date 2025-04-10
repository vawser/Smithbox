using Hexa.NET.ImGui;

namespace StudioCore.Editors.MapEditor.Tools.LightmapAtlasEditor;

public class LightmapAtlasContextMenu
{
    private MapEditorScreen Screen;
    private LightmapAtlasView AtlasScreen;

    public LightmapAtlasContextMenu(MapEditorScreen screen, LightmapAtlasView atlasScreen)
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
            if (ImGui.Selectable("Create"))
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
