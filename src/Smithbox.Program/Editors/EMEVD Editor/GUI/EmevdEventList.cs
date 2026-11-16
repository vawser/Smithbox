using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The selected file's events, by ID and EMELD name
/// </summary>
public class EmevdEventList
{
    private readonly EmevdEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;

    // The filtered list, rebuilt when the file or filter changes
    private EmevdFileWrapper CachedFile;
    private int CachedRevision = -1;
    private string CachedFilter;
    private bool CachedExact;
    private int CachedAliasVersion = -1;
    private List<EMEVD.Event> Visible = new();

    private readonly EmevdAliasMenu AliasMenu = new();

    public EmevdEventList(EmevdEditorView view)
    {
        View = view;
    }

    private EmevdAliases Aliases => View.Project.Handler.EmevdData?.Aliases;

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("EMEVD_EventList_Header"), LOC.Get("EMEVD_EventList_Header_TT"));

        ImGui.BeginChild("emevdEventListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("emevdEditor_EventList", ref Filter, ref ExactFilter);

        // Toggle: line counts
        GUI.DisplayToggleButton("emevdEventCountToggle", Icons.Hashtag,
            ref CFG.Current.EmevdEditor_Event_List_Display_Line_Count,
            "EMEVD_EventList_Count_Toggle_Hide",
            "EMEVD_EventList_Count_Toggle_Show",
            "EMEVD_EventList_Count_Toggle_TT");

        // Toggle: aliases
        GUI.DisplayToggleButton("emevdEventAliasToggle", Icons.StickyNote,
            ref CFG.Current.EmevdEditor_Event_List_Display_Aliases,
            "EMEVD_EventList_Alias_Toggle_Hide",
            "EMEVD_EventList_Alias_Toggle_Show",
            "EMEVD_EventList_Alias_Toggle_TT");

        ImGui.EndChild();

        ImGui.BeginChild("emevdEventList", Vector2.Zero, ImGuiChildFlags.Borders);

        var file = View.Selection.SelectedFile;

        if (file == null)
        {
            ImGui.TextDisabled(LOC.Get("EMEVD_EventList_No_File"));
        }
        else if (!file.IsLoaded)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, file.LoadError ?? LOC.Get("EMEVD_EventList_Not_Loaded"));
        }
        else
        {
            Refresh(file);
            DrawEvents(file);

            // Right-click on the empty space below the events: New Event
            if (ImGui.BeginPopupContextWindow("##emevdEventListMenu", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
            {
                View.EventCommands.MenuItems();
                ImGui.EndPopup();
            }
        }

        ImGui.EndChild();
    }

    private void Refresh(EmevdFileWrapper file)
    {
        // Adding, removing or renumbering an event is an edit, so the revision catches it; an alias change bumps the
        // aliases' version
        var aliases = Aliases;
        var aliasVersion = aliases?.Version ?? 0;

        if (file == CachedFile && file.Revision == CachedRevision && Filter == CachedFilter && ExactFilter == CachedExact
            && aliasVersion == CachedAliasVersion)
            return;

        CachedFile = file;
        CachedRevision = file.Revision;
        CachedFilter = Filter;
        CachedExact = ExactFilter;
        CachedAliasVersion = aliasVersion;

        Visible = file.Emevd.Events
            .Where(e => EditorFilters.IsMatch(Filter, e.ID.ToString(), ExactFilter, DisplayName(file, e)))
            .ToList();
    }

    private void DrawEvents(EmevdFileWrapper file)
    {
        var selection = View.Selection;

        if (selection.ScrollToEvent && selection.SelectedEvent != null)
        {
            var index = Visible.IndexOf(selection.SelectedEvent);
            if (index >= 0)
                GUI.ScrollClippedRowIntoView(ImGui.GetCursorPosY(), index);

            selection.ScrollToEvent = false;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(Visible.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var ev = Visible[i];
                var selected = selection.SelectedEvent == ev;

                if (ImGui.Selectable($"{ev.ID}##emevdEvent_{i}", selected))
                    selection.SelectEvent(ev);

                // Right-click selects the event and offers the event commands
                if (ImGui.BeginPopupContextItem($"##emevdEventMenu_{i}"))
                {
                    selection.SelectEvent(ev);
                    View.EventCommands.MenuItems();
                    DrawAliasMenu(file, ev);
                    ImGui.EndPopup();
                }

                var name = CFG.Current.EmevdEditor_Event_List_Display_Aliases ? DisplayName(file, ev) : "";
                if (!string.IsNullOrEmpty(name))
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, name);
                }

                if (CFG.Current.EmevdEditor_Event_List_Display_Line_Count)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled(LOC.Get("EMEVD_EventList_Event_Info", ev.Instructions.Count));
                }
            }
        }

        clipper.End();

        HandleKeys();
    }

    /// <summary>Up / Down step through the visible events while the list has focus</summary>
    private void HandleKeys()
    {
        if (!ImGui.IsWindowFocused() || ImGui.IsAnyItemActive() || Visible.Count == 0)
            return;

        var dir = GUI.ListStepKey();
        if (dir == 0)
            return;

        var selection = View.Selection;
        var at = selection.SelectedEvent == null ? -1 : Visible.IndexOf(selection.SelectedEvent);

        selection.SelectEvent(Visible[Math.Clamp(at + dir, 0, Visible.Count - 1)]);
        selection.ScrollToEvent = true;
    }

    /// <summary>The event's alias, else its .emeld name, else empty</summary>
    private string DisplayName(EmevdFileWrapper file, EMEVD.Event ev)
    {
        return Aliases?.EventDisplayName(file, ev) ?? ev.Name ?? "";
    }

    private void DrawAliasMenu(EmevdFileWrapper file, EMEVD.Event ev)
    {
        var aliases = Aliases;
        if (aliases == null)
            return;

        ImGui.Separator();

        // The field starts from the alias, or from the .emeld name so it can be adjusted rather than retyped
        var alias = aliases.EventAlias(file, ev);
        var current = !string.IsNullOrEmpty(alias) ? alias : ev.Name ?? "";
        var hint = !string.IsNullOrEmpty(ev.Name) ? LOC.Get("EMEVD_Alias_Emeld_Name", ev.Name) : null;

        if (AliasMenu.Draw("emevdEventAlias", current, LOC.Get("EMEVD_Alias_Menu_Event_TT"), hint, out var name))
        {
            aliases.SetEventAlias(file, ev, name);
            ImGui.CloseCurrentPopup();
        }
    }
}
