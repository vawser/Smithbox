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
    private List<long> Visible = new();
    private Dictionary<long, int> Calls = new();

    public EsdMachineList(EsdEditorView view)
    {
        View = view;
    }

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("ESD_MachineList_Header"), LOC.Get("ESD_MachineList_Header_TT"));

        ImGui.BeginChild("esdMachineListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("esdEditor_MachineList", ref Filter, ref ExactFilter);
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
        if (file == CachedFile && Filter == CachedFilter && ExactFilter == CachedExact)
            return;

        if (file != CachedFile)
            Calls = EsdLayout.CallCounts(file.Esd);

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
                ImGui.SetScrollY(index * ImGui.GetTextLineHeightWithSpacing());
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

                if (ImGui.IsItemHovered())
                    GUI.Tooltip(LOC.Get(EsdLayout.IsSubMachine(id) ? "ESD_MachineList_Sub_TT" : "ESD_MachineList_Entry_TT", id, $"0x{id:X}"));

                ImGui.SameLine();

                if (file.MachineNames.TryGetValue(id, out var name))
                {
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, name);
                    ImGui.SameLine();
                }

                var calls = Calls.TryGetValue(id, out var n) ? n : 0;
                ImGui.TextDisabled(calls > 0
                    ? LOC.Get("ESD_MachineList_Info_Called", machine.Count, calls)
                    : LOC.Get("ESD_MachineList_Info", machine.Count));
            }
        }

        clipper.End();
    }
}
