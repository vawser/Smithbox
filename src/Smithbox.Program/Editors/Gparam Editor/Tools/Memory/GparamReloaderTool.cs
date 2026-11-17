using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Memory;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Gparam Reloader (Tools window, Game Memory): pushes the selected GPARAM into the running game, replacing the copy
/// the game uses, and lists the GPARAMs the game has loaded. See Claude/Documentation/Gparam Reloader Analysis.md.
/// </summary>
public class GparamReloaderTool
{
    private readonly GparamEditorView View;
    private readonly ProjectEntry Project;

    // Shared by every view
    private GparamReloader Reloader => View.Editor.Reloader;
    private GameLink Link => Reloader.Link;

    private const double RefreshSeconds = 3;

    public GparamReloaderTool(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("GPARAM_Reloader_Header")}##gparamReloaderToolHeader"))
            return;

        ImGui.BeginChild("GparamReloaderToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        // Games with Gparam Reloader offsets (Assets/Gparam Reloader)
        if (!Reloader.IsSupported)
        {
            GUI.WrappedText(LOC.Get("GPARAM_Reloader_Game_Not_Supported"));
            return;
        }

        GUI.WrappedText(LOC.Get("GPARAM_Reloader_Hint"));

        // Polls only while the tool is shown
        if (CFG.Current.GparamEditor_Reloader_AutoRefresh &&
            (Reloader.LastPoll == null || (DateTime.Now - Reloader.LastPoll.Value).TotalSeconds >= RefreshSeconds))
        {
            Reloader.RequestPoll();
        }

        DisplayConnection();
        DisplayPush();
        DisplayLoaded();
    }

    // Game
    private void DisplayConnection()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Reloader_Game_Header"), LOC.Get("GPARAM_Reloader_Game_Header_TT"));

        var message = Reloader.StateMessage;
        if (string.IsNullOrEmpty(message))
            message = LOC.Get("GAMELINK_State_Waiting");

        if (Reloader.State is GameLink.LinkState.Failed)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, message);
        else
            GUI.WrappedText(message);

        ImGui.Checkbox($"{LOC.Get("GPARAM_Reloader_Auto_Refresh")}##gparamReloaderAutoRefresh", ref CFG.Current.GparamEditor_Reloader_AutoRefresh);
        GUI.Tooltip(LOC.Get("GPARAM_Reloader_Auto_Refresh_TT", RefreshSeconds));

        GUI.MultiButtonInput("gparamReloaderRefreshActions",
            "gparamReloaderRefresh", LOC.Get("GPARAM_Reloader_Refresh"), LOC.Get("GPARAM_Reloader_Refresh_TT"),
            Reloader.RequestPoll,
            "gparamReloaderDetach", LOC.Get("GPARAM_Reloader_Detach"), LOC.Get("GPARAM_Reloader_Detach_TT"),
            () =>
            {
                CFG.Current.GparamEditor_Reloader_AutoRefresh = false;
                Reloader.Detach();
            });
    }

    // Push
    private void DisplayPush()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Reloader_Push_Header"), LOC.Get("GPARAM_Reloader_Push_Header_TT"));

        var gparam = View.Selection._selectedGparam;
        var key = View.Selection._selectedGparamKey;

        if (gparam == null || string.IsNullOrEmpty(key))
        {
            GUI.WrappedText(LOC.Get("GPARAM_Reloader_No_Selection"));
        }
        else
        {
            var gameName = GparamReloader.GameNameOf(key);
            GUI.WrappedText(LOC.Get("GPARAM_Reloader_Selected", gameName));

            if (Reloader.IsReady)
            {
                var loaded = Reloader.Loaded.FirstOrDefault(g => string.Equals(g.Name, gameName, StringComparison.OrdinalIgnoreCase));

                if (loaded == null)
                    GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("GPARAM_Reloader_Selected_Not_Loaded"));
                else
                    GUI.WrappedText(LOC.Get("GPARAM_Reloader_Selected_Loaded"));
            }

            if (Reloader.IsReloading)
            {
                GUI.WrappedText(LOC.Get("GPARAM_Reloader_Running"));
            }
            else
            {
                GUI.MultiButtonInput("gparamReloaderPushActions",
                    "gparamReloaderPush", LOC.Get("GPARAM_Reloader_Push"), LOC.Get("GPARAM_Reloader_Push_TT"),
                    () => Push(gameName, gparam));
            }
        }

        var last = Reloader.LastReload;
        if (last != null)
        {
            var text = $"{last.At:T}  {last.Message}";

            if (last.Outcome is GparamReloader.ReloadOutcome.Reloaded)
                GUI.WrappedText(text);
            else
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, text);
        }
    }

    private void Push(string gameName, SoulsFormats.GPARAM gparam)
    {
        byte[] bytes;

        try
        {
            // Uncompressed: the game decompresses .gparam.dcx before Init sees it, and Write() alone would use the
            // compression the file was read with (DCX for ER)
            bytes = gparam.Write(SoulsFormats.DCX.Type.None);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("GPARAM_Reloader_Write_Failed", gameName), e);
            return;
        }

        Reloader.RequestReload(gameName, bytes);
    }

    // Loaded in game
    private void DisplayLoaded()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_Reloader_Loaded_Header"), LOC.Get("GPARAM_Reloader_Loaded_Header_TT"));

        var loaded = Reloader.Loaded;

        if (loaded.Count == 0)
        {
            GUI.WrappedText(LOC.Get("GPARAM_Reloader_Loaded_None"));
            return;
        }

        var selectedName = string.IsNullOrEmpty(View.Selection._selectedGparamKey)
            ? null
            : GparamReloader.GameNameOf(View.Selection._selectedGparamKey);

        var bank = Project.Handler.GparamData.PrimaryBank;

        ImGui.BeginChild("gparamReloaderLoadedList", new System.Numerics.Vector2(0, 200 * DPI.UIScale()), ImGuiChildFlags.Borders);

        foreach (var entry in loaded)
        {
            var label = entry.Active || entry.ActiveIds > 0
                ? LOC.Get("GPARAM_Reloader_Loaded_Entry_Active", entry.Name)
                : entry.Name;

            var isSelected = string.Equals(entry.Name, selectedName, StringComparison.OrdinalIgnoreCase);

            if (ImGui.Selectable($"{label}##gparamReloaderLoaded_{entry.Name}", isSelected))
            {
                var file = bank.Entries.Keys.FirstOrDefault(k => string.Equals(GparamReloader.GameNameOf(k.Filename), entry.Name, StringComparison.OrdinalIgnoreCase));

                if (file != null)
                {
                    View.Selection.SetFileSelection(file);
                    View.Selection.FocusFile = true;
                }
            }

            GUI.Tooltip(LOC.Get("GPARAM_Reloader_Loaded_Entry_TT"));
        }

        ImGui.EndChild();
    }
}
