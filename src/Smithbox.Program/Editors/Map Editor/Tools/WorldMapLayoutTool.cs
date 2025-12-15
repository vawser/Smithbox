using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.MapEditor;

public class WorldMapLayoutTool
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    private int xLargeOffset = 0;
    private int yLargeOffset = 0;

    private int xMediumOffset = 0;
    private int yMediumOffset = 0;

    private int xSmallOffset = 0;
    private int ySmallOffset = 0;

    private int SmallTile = 256;
    private int MediumTile = 512;
    private int LargeTile = 1024;

    public WorldMapLayoutTool(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        // Not shown here
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        // Not shown here
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        if (ImGui.CollapsingHeader("World Map Layout"))
        {
            ImGui.InputInt("xLargeOffset", ref xLargeOffset);
            ImGui.InputInt("yLargeOffset", ref yLargeOffset);

            ImGui.InputInt("xMediumOffset", ref xMediumOffset);
            ImGui.InputInt("yMediumOffset", ref yMediumOffset);

            ImGui.InputInt("xSmallOffset", ref xSmallOffset);
            ImGui.InputInt("ySmallOffset", ref ySmallOffset);

            ImGui.InputInt("SmallTile", ref SmallTile);
            ImGui.InputInt("MediumTile", ref MediumTile);
            ImGui.InputInt("LargeTile", ref LargeTile);

            if (ImGui.Button("Regenerate"))
            {
                Editor.WorldMapTool.GenerateWorldMapLayout_Limveld(
                    SmallTile, MediumTile, LargeTile,
                    xLargeOffset, yLargeOffset,
                    xMediumOffset, yMediumOffset,
                    xSmallOffset, ySmallOffset);
            }
        }
    }
}