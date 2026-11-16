using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The selected ESD's machines: entry machines (the ones the game starts) first, then sub-machines (x0, x1...,
/// started by bank 6 calls), each with its state count and how many calls in the file start it.
/// </summary>
public class EsdMachineList
{
    private readonly EsdEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;

    // The filtered list, rebuilt when the file or filter changes
    private EsdFileWrapper CachedFile;
    private string CachedFilter;
    private bool CachedExact;
    private int CachedRevision = -1;
    private int CachedAliasVersion = -1;
    private List<long> Visible = new();
    private Dictionary<long, int> Calls = new();

    private readonly EsdAliasMenu AliasMenu = new();

    public EsdMachineList(EsdEditorView view)
    {
        View = view;
    }

    private EsdAliases Aliases => View.Project.Handler.EsdData?.Aliases;

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("ESD_MachineList_Header"), LOC.Get("ESD_MachineList_Header_TT"));

        ImGui.BeginChild("esdMachineListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("esdEditor_MachineList", ref Filter, ref ExactFilter);

        // Toggle: state counts
        GUI.DisplayToggleButton("esdMachineStateCountToggle", Icons.Hashtag,
            ref CFG.Current.EsdEditor_Machine_List_Display_State_Count,
            "ESD_MachineList_State_Toggle_Hide",
            "ESD_MachineList_State_Toggle_Show",
            "ESD_MachineList_State_Toggle_TT");

        // Toggle: call counts
        GUI.DisplayToggleButton("esdMachineCallCountToggle", Icons.Phone,
            ref CFG.Current.EsdEditor_Machine_List_Display_Call_Count,
            "ESD_MachineList_Call_Toggle_Hide",
            "ESD_MachineList_Call_Toggle_Show",
            "ESD_MachineList_Call_Toggle_TT");

        // Toggle: aliases
        GUI.DisplayToggleButton("esdMachineAliasToggle", Icons.StickyNote,
            ref CFG.Current.EsdEditor_Machine_List_Display_Aliases,
            "ESD_MachineList_Alias_Toggle_Hide",
            "ESD_MachineList_Alias_Toggle_Show",
            "ESD_MachineList_Alias_Toggle_TT");

        ImGui.EndChild();

        ImGui.BeginChild("esdMachineList", Vector2.Zero, ImGuiChildFlags.Borders);

        var file = View.Selection.SelectedFile;

        if (file == null)
            ImGui.TextDisabled(LOC.Get(View.Selection.SelectedContainer?.IsBinder == true ? "ESD_MachineList_Pick_Esd" : "ESD_MachineList_No_File"));
        else if (file.Esd == null)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, file.LoadError ?? LOC.Get("ESD_MachineList_Not_Loaded"));
        else
        {
            Refresh(file);
            DrawMachines(file);
        }

        ImGui.EndChild();
    }

    private void Refresh(EsdFileWrapper file)
    {
        // Edits (new / deleted machines, calls) change the list: rebuild on the container's revision too
        // An alias change bumps the aliases' version: the filter matches aliases too
        var revision = file.Container.Revision;
        var aliasVersion = Aliases?.Version ?? 0;
        if (file == CachedFile && Filter == CachedFilter && ExactFilter == CachedExact && revision == CachedRevision
            && aliasVersion == CachedAliasVersion)
            return;

        if (file != CachedFile || revision != CachedRevision)
            Calls = EsdLayout.CallCounts(file.Esd);

        CachedRevision = revision;
        CachedAliasVersion = aliasVersion;
        CachedFile = file;
        CachedFilter = Filter;
        CachedExact = ExactFilter;

        Visible = file.Esd.StateGroups.Keys
            .Where(id => EditorFilters.IsMatch(Filter, EsdFormat.MachineLabel(file, id), ExactFilter, id.ToString()))
            .OrderBy(id => EsdLayout.IsSubMachine(id) ? 1 : 0)
            .ThenBy(id => EsdLayout.IsSubMachine(id) ? EsdLayout.SubMachineTop - id : id)
            .ToList();
    }

    private void DrawMachines(EsdFileWrapper file)
    {
        var selection = View.Selection;

        if (selection.ScrollToMachine && selection.SelectedMachine is long sel)
        {
            var index = Visible.IndexOf(sel);
            if (index >= 0)
            {
                // Only scroll when the entry is out of view, and then center it,
                // so clicking a visible entry doesn't move the list
                var lineHeight = ImGui.GetTextLineHeightWithSpacing();
                var itemTop = ImGui.GetCursorPosY() + index * lineHeight;
                var itemBottom = itemTop + lineHeight;
                var viewTop = ImGui.GetScrollY();
                var viewHeight = ImGui.GetWindowHeight();
                var viewBottom = viewTop + viewHeight;

                if (itemTop < viewTop || itemBottom > viewBottom)
                    ImGui.SetScrollY(Math.Max(0f, itemTop - (viewHeight - lineHeight) * 0.5f));
            }
            selection.ScrollToMachine = false;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(Visible.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var id = Visible[i];
                var machine = file.Esd.StateGroups[id];
                var selected = selection.SelectedMachine == id;

                if (ImGui.Selectable($"{EsdLayout.MachineLabel(id)}##esdMachine_{id}", selected))
                    selection.SelectMachine(id);

                var name = EsdFormat.MachineName(file, id);
                var eddName = EddName(file, id);

                if (ImGui.IsItemHovered())
                {
                    var tip = LOC.Get(EsdLayout.IsSubMachine(id) ? "ESD_MachineList_Sub_TT" : "ESD_MachineList_Entry_TT", id, $"0x{id:X}");
                    // A translated or renamed machine: its original .edd name too
                    if (!string.IsNullOrEmpty(eddName) && eddName != name)
                        tip += "\n" + LOC.Get("ESD_Alias_Edd_Name", eddName);
                    GUI.Tooltip(tip);
                }

                if (ImGui.BeginPopupContextItem($"##esdMachineMenu_{id}"))
                {
                    DrawAliasMenu(file, id, eddName);
                    ImGui.Separator();
                    View.Commands.MachineMenu(id);
                    ImGui.EndPopup();
                }

                ImGui.SameLine();

                if (name.Length > 0 && CFG.Current.EsdEditor_Machine_List_Display_Aliases)
                {
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, name);
                    ImGui.SameLine();
                }

                var calls = CFG.Current.EsdEditor_Machine_List_Display_Call_Count && Calls.TryGetValue(id, out var n) ? n : 0;
                var states = CFG.Current.EsdEditor_Machine_List_Display_State_Count;
                var info = (states, calls > 0) switch
                {
                    (true, true) => LOC.Get("ESD_MachineList_Info_Called", machine.Count, calls),
                    (true, false) => LOC.Get("ESD_MachineList_Info", machine.Count),
                    (false, true) => LOC.Get("ESD_MachineList_Info_Called_Only", calls),
                    _ => null
                };
                if (info != null)
                    ImGui.TextDisabled(info);
                else
                    ImGui.NewLine(); // ends the SameLine after the label / alias
            }
        }

        clipper.End();

        // Right-click on empty space: a new machine (no alias: there's no machine yet)
        if (ImGui.BeginPopupContextWindow("##esdMachinesMenu", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
        {
            View.Commands.MachineMenu(null);
            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// The machine's original .edd name: the project's .edd (DS2), else the one a translated built-in alias keeps as
    /// its description; null if none
    /// </summary>
    private string EddName(EsdFileWrapper file, long id)
    {
        if (file.MachineNames.TryGetValue(id, out var name))
            return name;

        var entry = Aliases?.MachineAliasEntry(file, id);
        return entry?.Tags?.Contains("edd") == true && !string.IsNullOrEmpty(entry.Description) ? entry.Description : null;
    }

    private void DrawAliasMenu(EsdFileWrapper file, long id, string eddName)
    {
        var aliases = Aliases;
        if (aliases == null)
            return;

        // The field starts from the alias, or from the .edd name so it can be adjusted rather than retyped
        var alias = aliases.MachineAlias(file, id);
        var current = !string.IsNullOrEmpty(alias) ? alias : eddName ?? "";
        var hint = !string.IsNullOrEmpty(eddName) ? LOC.Get("ESD_Alias_Edd_Name", eddName) : null;

        if (AliasMenu.Draw("esdMachineAlias", current, LOC.Get("ESD_Alias_Menu_Machine_TT"), hint, out var name))
        {
            aliases.SetMachineAlias(file, id, name);
            ImGui.CloseCurrentPopup();
        }
    }
}
