using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Problems (Behavior Editor Tools window): the validator's results. In the File scope, for the shown file: checked
/// when a file is first shown, and again with Check; clicking a problem shows its object in the Outline. The Binder,
/// Modified and All scopes check those files in the background (HavokBehaviorScan), and clicking a problem opens its
/// file first. See Claude/Documentation/Behavior.md (Havok behavior tools) and UI.md (Tools window conventions).
/// </summary>
public class HavokBehaviorProblemsTool
{
    private readonly HavokEditorView View;
    private readonly ProjectEntry Project;

    private HavokBehaviorView Behavior => View.PropertyView.BehaviorView;

    // The File scope's problems, and the file and symbols they were found in
    private List<HavokBehaviorProblem> Problems;
    private HavokFileWrapper ProblemsFile;
    private HavokBehaviorSymbols ProblemsSymbols;

    private HavokBehaviorScope Scope = HavokBehaviorScope.File;
    private readonly HavokBehaviorScan Scan = new();

    public HavokBehaviorProblemsTool(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// The validator as a scan: one result per problem
    /// </summary>
    public static void CheckFile(HavokBehaviorSymbols symbols, ProjectEntry project, HavokScanSink sink)
    {
        foreach (var problem in HavokBehaviorValidator.Run(project, symbols))
        {
            if (sink.IsFull)
                return;

            sink.Add(problem.Message, problem.Target);
        }
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("HAVOK_ProblemsTool_Header")}##havokBehaviorProblemsToolHeader"))
            return;

        ImGui.BeginChild("HavokBehaviorProblemsToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    /// <summary>
    /// The File scope's problems for the shown file: checked again when another file is shown, and pruned when an
    /// edit rebuilds the symbols (it can remove what a problem points at)
    /// </summary>
    private void UpdateFileProblems()
    {
        var symbols = Behavior.Symbols;
        var file = View.Selection.SelectedFileWrapper;

        if (symbols == null)
        {
            Problems = null;
            ProblemsFile = null;
            ProblemsSymbols = null;
            return;
        }

        if (Problems == null || !ReferenceEquals(file, ProblemsFile))
        {
            CheckShownFile();
            return;
        }

        if (!ReferenceEquals(symbols, ProblemsSymbols))
        {
            Problems.RemoveAll(p => !symbols.Contains(p.Target));
            ProblemsSymbols = symbols;
        }
    }

    private void CheckShownFile()
    {
        Problems = HavokBehaviorValidator.Run(Project, Behavior.Symbols);
        ProblemsFile = View.Selection.SelectedFileWrapper;
        ProblemsSymbols = Behavior.Symbols;
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("HAVOK_ProblemsTool_Hint"));

        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Tool_Scope_Header"), LOC.Get("HAVOK_Tool_Scope_Header_TT"));

        var scope = Scope;
        var canRun = HavokBehaviorScan.DrawScope("havokProblemsScope", ref scope, Behavior.IsShown);

        if (scope != Scope)
        {
            Scope = scope;
            Scan.Clear();
        }

        if (Scope is HavokBehaviorScope.File)
            UpdateFileProblems();

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Tool_Actions_Header"), LOC.Get("HAVOK_Tool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("havokProblemsActions",
            "havokProblemsCheck", LOC.Get("HAVOK_ProblemsView_Check"),
            LOC.Get(Scope is HavokBehaviorScope.File ? "HAVOK_ProblemsView_Check_TT" : "HAVOK_ProblemsView_Check_Scope_TT"),
            Run, canRun && !Scan.IsRunning);

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Tool_Results_Header"), LOC.Get("HAVOK_ProblemsTool_Results_Header_TT"));

        if (Scope is HavokBehaviorScope.File)
        {
            DisplayFileResults();
            return;
        }

        var results = Scan.Snapshot();

        if (!Scan.HasRun && !Scan.IsRunning)
        {
            GUI.WrappedText(LOC.Get("HAVOK_ProblemsView_Not_Run"));
            return;
        }

        if (Scan.IsRunning)
            ImGui.TextDisabled(LOC.Get("HAVOK_ProblemsView_Count", results.Count));
        else if (results.Count == 0)
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("HAVOK_ProblemsView_None"));
        else
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_ProblemsView_Count_Files", results.Count,
                results.Select(r => (r.Binder, r.FilePath)).Distinct().Count()));

        Scan.DrawStatus("havokProblemsScan");

        HavokBehaviorScan.DrawResults("havokProblemsScan", results, Behavior, true);
    }

    private void Run()
    {
        if (Scope is HavokBehaviorScope.File)
            CheckShownFile();
        else
            Scan.Start(View, Project, Scope, CheckFile);
    }

    private void DisplayFileResults()
    {
        if (Problems == null)
            return;

        if (Problems.Count == 0)
        {
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("HAVOK_ProblemsView_None"));
            return;
        }

        ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_ProblemsView_Count", Problems.Count));

        ImGui.BeginChild("havokProblemsFileResults");

        var clipper = new ImGuiListClipper();
        clipper.Begin(Problems.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var problem = Problems[i];

                if (ImGui.Selectable($"{problem.Message}##problem{i}"))
                    Behavior.OutlineView.Navigate(problem.Target);

                GUI.Tooltip(LOC.Get("HAVOK_ProblemsView_Entry_TT", HavokBehaviorSymbols.GetLabel(problem.Target)));
            }
        }

        clipper.End();

        ImGui.EndChild();
    }
}
