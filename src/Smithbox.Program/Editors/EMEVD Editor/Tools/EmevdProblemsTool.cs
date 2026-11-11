using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Problems: the checks pass (EmevdChecks) over every event of the selected file, rerun when the file changes. Click
/// a problem to go to it. Vanilla files have a few of these too (unmatched gotos, short calls), so not every one is a
/// mistake; block edits are refused when they'd add one.
/// </summary>
public class EmevdProblemsTool
{
    private readonly EmevdEditorView View;

    private EmevdFileWrapper CachedFile;
    private int CachedRevision = -1;
    private List<(EMEVD.Event Event, EmevdProblem Problem)> Problems = new();

    private string Filter = "";
    private bool ExactFilter = false;
    private string CachedFilter;
    private bool CachedExact;
    private List<(EMEVD.Event Event, EmevdProblem Problem)> Visible = new();

    private bool ShowList = true;

    public EmevdProblemsTool(EmevdEditorView view)
    {
        View = view;
    }

    public int Count => Problems.Count;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("EMEVD_ProblemsTool_Header")}##emevdProblemsToolHeader"))
            return;

        ImGui.BeginChild("EmevdProblemsToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("EMEVD_ProblemsTool_Hint"));

        var file = View.Selection.SelectedFile;

        if (file is not { IsLoaded: true })
        {
            GUI.WrappedText(LOC.Get("EMEVD_ProblemsTool_No_File"));
            return;
        }

        Refresh(file);

        // Summary
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_ProblemsTool_Summary_Header"), LOC.Get("EMEVD_ProblemsTool_Summary_Header_TT"));

        if (Problems.Count == 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("EMEVD_ProblemsTool_None", file.Name));
            return;
        }

        GUI.WrappedText(LOC.Get("EMEVD_ProblemsTool_Count", Problems.Count, file.Name,
            string.Join(", ", Problems.GroupBy(p => p.Problem.Kind).Select(g => $"{LOC.Get($"EMEVD_Problem_{g.Key}")} {g.Count()}"))));

        // Problems
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("EMEVD_ProblemsTool_List_Header"), LOC.Get("EMEVD_ProblemsTool_List_Header_TT"), ref ShowList);

        if (!ShowList)
            return;

        EditorFilters.DisplayFramedListFilter("emevdProblemsFilter", ref Filter, ref ExactFilter);
        RefreshVisible();

        ImGui.BeginChild("emevdProblemsList", Vector2.Zero, ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(Visible.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var (ev, p) = Visible[i];
                var where = p.Line >= 0 ? LOC.Get("EMEVD_ProblemsTool_Where_Line", ev.ID, p.Line) : LOC.Get("EMEVD_ProblemsTool_Where_Event", ev.ID);

                if (ImGui.Selectable($"{where}##emevdProblem{i}"))
                    View.Selection.JumpTo(file, ev, p.Line);
                GUI.Tooltip(LOC.Get($"EMEVD_Problem_{p.Kind}_TT"));

                ImGui.SameLine();
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get($"EMEVD_Problem_{p.Kind}"));
                ImGui.SameLine();
                ImGui.TextDisabled(p.Detail);
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    private void Refresh(EmevdFileWrapper file)
    {
        if (file == CachedFile && file.Revision == CachedRevision)
            return;

        CachedFile = file;
        CachedRevision = file.Revision;
        CachedFilter = null;
        Problems = new();

        var catalog = View.Project.Handler.EmevdData?.Catalog;
        var bank = View.Project.Handler.EmevdData?.PrimaryBank;

        foreach (var ev in file.Emevd.Events)
        {
            var script = EmevdScript.Build(catalog, ev);
            foreach (var p in EmevdChecks.Run(script, id => bank?.ResolveEvent(file, id).ev))
                Problems.Add((ev, p));
        }
    }

    /// <summary>The filtered list, rebuilt when the filter or the problems change; matches the event ID or problem name</summary>
    private void RefreshVisible()
    {
        if (CachedFilter == Filter && CachedExact == ExactFilter)
            return;

        CachedFilter = Filter;
        CachedExact = ExactFilter;

        Visible = Problems
            .Where(x => EditorFilters.IsMatch(Filter, x.Event.ID.ToString(), ExactFilter, LOC.Get($"EMEVD_Problem_{x.Problem.Kind}")))
            .ToList();
    }
}
