using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The project's .emevd files: common, common_func, then the maps
/// </summary>
public class EmevdFileList
{
    private readonly EmevdEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;

    public bool ScrollToSelection;

    public EmevdFileList(EmevdEditorView view)
    {
        View = view;
    }

    private EmevdBank Bank => View.Project.Handler.EmevdData?.PrimaryBank;

    private EmevdAliases Aliases => View.Project.Handler.EmevdData?.Aliases;

    private readonly EmevdAliasMenu AliasMenu = new();

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("EMEVD_FileList_Header"), LOC.Get("EMEVD_FileList_Header_TT"));

        ImGui.BeginChild("emevdFileListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("emevdEditor_FileList", ref Filter, ref ExactFilter);

        // Toggle: event counts
        GUI.DisplayToggleButton("emevdFileCountToggle", Icons.Hashtag,
            ref CFG.Current.EmevdEditor.EmevdEditor_File_List_Display_Event_Count,
            "EMEVD_FileList_Count_Toggle_Hide",
            "EMEVD_FileList_Count_Toggle_Show",
            "EMEVD_FileList_Count_Toggle_TT");

        // Toggle: aliases
        GUI.DisplayToggleButton("emevdFileAliasToggle", Icons.StickyNote,
            ref CFG.Current.EmevdEditor.EmevdEditor_File_List_Display_Aliases,
            "EMEVD_FileList_Alias_Toggle_Hide",
            "EMEVD_FileList_Alias_Toggle_Show",
            "EMEVD_FileList_Alias_Toggle_TT");

        ImGui.EndChild();

        ImGui.BeginChild("emevdFileList", Vector2.Zero, ImGuiChildFlags.Borders);

        var bank = Bank;
        if (bank != null)
        {
            if (bank.Files.Count == 0)
                ImGui.TextDisabled(LOC.Get("EMEVD_FileList_No_Files"));

            var aliases = Aliases;
            Visible.Clear();

            foreach (var file in bank.Files)
            {
                if (!EditorFilters.IsMatch(Filter, file.Name, ExactFilter, () => file.VanillaStatus, aliases?.FileAlias(file) ?? ""))
                    continue;

                Visible.Add(file);
                DrawFile(file);
            }

            HandleKeys();
        }

        ImGui.EndChild();
    }

    /// <summary>The files drawn this frame (those matching the filter), top to bottom</summary>
    private readonly List<EmevdFileWrapper> Visible = new();

    /// <summary>Up / Down step through the visible files while the list has focus</summary>
    private void HandleKeys()
    {
        if (!ImGui.IsWindowFocused() || ImGui.IsAnyItemActive() || Visible.Count == 0)
            return;

        var dir = GUI.ListStepKey();
        if (dir == 0)
            return;

        var selection = View.Selection;
        var at = selection.SelectedFile == null ? -1 : Visible.IndexOf(selection.SelectedFile);

        selection.SelectFile(Visible[Math.Clamp(at + dir, 0, Visible.Count - 1)]);
        ScrollToSelection = true;
    }

    private void DrawFile(EmevdFileWrapper file)
    {
        var selected = View.Selection.SelectedFile == file;

        var label = file.IsDirty ? $"{file.Name}*" : file.Name;
        var status = file.VanillaStatus;

        var coloured = VanillaMarkers.PushColour(status);
        var clicked = ImGui.Selectable($"{label}##emevdFile_{file.Name}", selected);
        VanillaMarkers.PopColour(coloured);
        if (coloured)
            VanillaMarkers.Tooltip(status);

        if (clicked)
        {
            View.Selection.SelectFile(file);
        }

        var alias = Aliases?.FileAlias(file) ?? "";

        // Right-click: the file's alias
        if (Aliases != null && ImGui.BeginPopupContextItem($"##emevdFileMenu_{file.Name}"))
        {
            if (AliasMenu.Draw("emevdFileAlias", alias, LOC.Get("EMEVD_Alias_Menu_File_TT"), null, out var name))
            {
                Aliases.SetFileAlias(file, name);
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        // Centre the selection only when it's out of view, so stepping with the arrow keys doesn't recentre every press
        if (selected && ScrollToSelection)
        {
            if (!GUI.IsLastItemFullyInView())
                ImGui.SetScrollHereY();
            ScrollToSelection = false;
        }

        if (CFG.Current.EmevdEditor.EmevdEditor_File_List_Display_Aliases && !string.IsNullOrEmpty(alias))
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, alias);
        }

        if (file.LoadError != null)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, Icons.ExclamationTriangle);
            GUI.Tooltip(file.LoadError);
        }
        else if (file.IsLoaded && CFG.Current.EmevdEditor.EmevdEditor_File_List_Display_Event_Count)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("EMEVD_FileList_Event_Count", file.Emevd.Events.Count));
        }
    }
}
