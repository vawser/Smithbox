using StudioCore.Application;
using StudioCore.Editors.MaterialEditor;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// World Reloader (ER): asks the running game to save and load the same save again, so map changes written to the
/// game files show without going to the title screen. Uses the Material Editor's game link when it exists, so both
/// tools share one connection and one "busy" state; otherwise keeps its own.
/// </summary>
public class WorldReloaderTool
{
    private MapEditorView View;
    private ProjectEntry Project;

    private MatGameLink OwnLink;

    public WorldReloaderTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private MatGameLink Link => Project.Handler.MaterialEditor?.GameLink ?? (OwnLink ??= new MatGameLink());

    public bool CanUse() => Project.Descriptor.ProjectType is ProjectType.ER;

    public void OnToolWindow()
    {
        GUI.WrappedText(LOC.Get("MAP_WorldReloader_Description"));

        // Game
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_WorldReloader_Game_Header"), LOC.Get("MAP_WorldReloader_Game_Header_TT"));

        var message = Link.StateMessage;
        if (string.IsNullOrEmpty(message))
            message = LOC.Get("MAT_InGameTool_State_Waiting");

        if (Link.State is MatGameLink.LinkState.Failed)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, message);
        else
            GUI.WrappedText(message);

        // World
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_InGameTool_World_Header"), LOC.Get("MAP_WorldReloader_World_Header_TT"));

        if (Link.IsReloadingWorld)
        {
            GUI.WrappedText(LOC.Get("MAT_InGameTool_World_Running"));
        }
        else if (Link.IsReloading || Link.IsQuittingOut)
        {
            GUI.WrappedText(LOC.Get("MAP_WorldReloader_Busy"));
        }
        else
        {
            GUI.MultiButtonInput("mapWorldReloaderActions",
                "mapWorldReloaderReloadWorld", LOC.Get("MAT_InGameTool_World_Reload"), LOC.Get("MAT_InGameTool_World_Reload_TT"),
                Link.RequestReloadWorld);
        }

        var world = Link.LastWorldReload;
        if (world != null)
            GUI.WrappedText(world);
    }
}
