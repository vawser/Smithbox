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
    private string CachedFilter;
    private bool CachedExact;
    private List<EMEVD.Event> Visible = new();

    public EmevdEventList(EmevdEditorView view)
    {
        View = view;
    }

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("EMEVD_EventList_Header"), LOC.Get("EMEVD_EventList_Header_TT"));

        ImGui.BeginChild("emevdEventListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("emevdEditor_EventList", ref Filter, ref ExactFilter);
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
        }

        ImGui.EndChild();
    }

    private void Refresh(EmevdFileWrapper file)
    {
        if (file == CachedFile && Filter == CachedFilter && ExactFilter == CachedExact)
            return;

        CachedFile = file;
        CachedFilter = Filter;
        CachedExact = ExactFilter;

        Visible = file.Emevd.Events
            .Where(e => EditorFilters.IsMatch(Filter, e.ID.ToString(), ExactFilter, e.Name ?? ""))
            .ToList();
    }

    private void DrawEvents(EmevdFileWrapper file)
    {
        var selection = View.Selection;

        if (selection.ScrollToEvent && selection.SelectedEvent != null)
        {
            var index = Visible.IndexOf(selection.SelectedEvent);
            if (index >= 0)
                ImGui.SetScrollY(index * ImGui.GetTextLineHeightWithSpacing());

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

                ImGui.SameLine();

                if (!string.IsNullOrEmpty(ev.Name))
                {
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, ev.Name);
                    ImGui.SameLine();
                }

                ImGui.TextDisabled(LOC.Get("EMEVD_EventList_Event_Info", ev.Instructions.Count));
            }
        }

        clipper.End();
    }
}
