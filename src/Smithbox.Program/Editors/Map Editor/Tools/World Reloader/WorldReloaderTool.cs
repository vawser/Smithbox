using StudioCore.Application;
using StudioCore.Memory;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// World Reloader: asks the running game to save and load the same save again, so map changes written to the
/// game files show without going to the title screen. Uses the project's shared GameLink, so it shares one
/// connection and one "busy" state with the other live tools (the Material Reloader).
/// </summary>
public class WorldReloaderTool
{
    private MapEditorView View;
    private ProjectEntry Project;

    public WorldReloaderTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private GameLink Link => Project.Handler.GameLink;

    /// <summary>Games whose Game Link offsets offer a world reload (Assets/Game Link)</summary>
    public bool CanUse() => Link.OffersWorldReload;

    public void OnToolWindow()
    {
        GUI.WrappedText(LOC.Get("MAP_WorldReloader_Description"));

        // Game
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_WorldReloader_Game_Header"), LOC.Get("MAP_WorldReloader_Game_Header_TT"));

        var message = Link.StateMessage;
        if (string.IsNullOrEmpty(message))
            message = LOC.Get("GAMELINK_State_Waiting");

        if (Link.State is GameLink.LinkState.Failed)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, message);
        else
            GUI.WrappedText(message);

        // World
        GUI.Spacer();
        // ER and NR save first; AC6 loads the last save as it is
        var savesFirst = Link.WorldReloadSavesFirst;

        GUI.SimpleHeader(LOC.Get("GAMELINK_World_Header"),
            LOC.Get(savesFirst ? "MAP_WorldReloader_World_Header_TT" : "MAP_WorldReloader_World_Header_No_Save_TT"));

        if (!savesFirst)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GAMELINK_World_No_Save_Warning"));

        if (Link.IsReloadingWorld)
        {
            GUI.WrappedText(LOC.Get("GAMELINK_World_Running"));
        }
        else if (Link.IsBusy)
        {
            GUI.WrappedText(LOC.Get("MAP_WorldReloader_Busy"));
        }
        else
        {
            GUI.MultiButtonInput("mapWorldReloaderActions",
                "mapWorldReloaderReloadWorld", LOC.Get("GAMELINK_World_Reload"),
                LOC.Get(savesFirst ? "GAMELINK_World_Reload_TT" : "GAMELINK_World_Reload_No_Save_TT"),
                Link.RequestReloadWorld);
        }

        var world = Link.LastWorldReload;
        if (world != null)
            GUI.WrappedText(world);
    }
}
