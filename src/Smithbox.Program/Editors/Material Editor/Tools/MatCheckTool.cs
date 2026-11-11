using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Material Check: runs MaterialChecks over a set of materials and lists what it finds. Vanilla materials give
/// no findings in any surveyed game, so everything listed differs from how the game's own files are made.
/// </summary>
public class MatCheckTool
{
    public MaterialEditorView View;
    public ProjectEntry Project;

    private enum CheckScope { Modified, Binder, All }

    private CheckScope Scope = CheckScope.Modified;

    private record Result(IMaterialBinder Binder, MaterialFileWrapper File, MaterialFinding Finding);

    private List<Result> Results;
    private int CheckedCount;

    public MatCheckTool(MaterialEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private MaterialBank Bank => Project.Handler.MaterialData.PrimaryBank;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MAT_CheckTool_Header")}##materialCheckToolHeader"))
            return;

        ImGui.BeginChild("MaterialCheckToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MAT_CheckTool_Hint"));

        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_CheckTool_Scope_Header"), LOC.Get("MAT_CheckTool_Scope_Header_TT"));

        if (ImGui.RadioButton($"{LOC.Get("MAT_CheckTool_Scope_Modified")}##materialCheckScopeModified", Scope == CheckScope.Modified))
            Scope = CheckScope.Modified;
        if (ImGui.RadioButton($"{LOC.Get("MAT_CheckTool_Scope_Binder")}##materialCheckScopeBinder", Scope == CheckScope.Binder))
            Scope = CheckScope.Binder;
        if (ImGui.RadioButton($"{LOC.Get("MAT_CheckTool_Scope_All")}##materialCheckScopeAll", Scope == CheckScope.All))
            Scope = CheckScope.All;

        if (Scope == CheckScope.Binder && View.Selection.SelectedBinder == null)
        {
            GUI.WrappedText(LOC.Get("MAT_CheckTool_No_Binder"));
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_CheckTool_Actions_Header"), LOC.Get("MAT_CheckTool_Actions_Header_TT"));

        var canRun = Scope != CheckScope.Binder || View.Selection.SelectedBinder != null;

        GUI.ConditionalMultiButtonInput("materialCheckActions",
            "materialCheckRun", LOC.Get("MAT_CheckTool_Run"), LOC.Get("MAT_CheckTool_Run_TT"),
            Run, canRun);

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_CheckTool_Results_Header"), LOC.Get("MAT_CheckTool_Results_Header_TT"));

        if (Results == null)
        {
            GUI.WrappedText(LOC.Get("MAT_CheckTool_Not_Run"));
            return;
        }

        var errors = Results.Count(r => r.Finding.Severity is MaterialCheckSeverity.Error);
        GUI.WrappedText(LOC.Get("MAT_CheckTool_Results_Summary", CheckedCount, errors, Results.Count - errors));

        if (Results.Count == 0)
            return;

        ImGui.BeginChild("MaterialCheckResults", ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(Results.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var result = Results[i];

                ImGui.TextDisabled(MaterialBank.ShortNameOf(result.File.Key));
                ImGui.SameLine();

                if (MaterialEditorChecks.DisplayFinding(result.Finding, $"materialCheckResult{i}"))
                {
                    JumpTo(result);
                }
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    private IEnumerable<IMaterialBinder> AllBinders() =>
        Bank.MTDs.Values.Cast<IMaterialBinder>().Concat(Bank.MATBINs.Values);

    private void Run()
    {
        var files = new List<(IMaterialBinder binder, MaterialFileWrapper file)>();

        switch (Scope)
        {
            case CheckScope.Modified:
                foreach (var binder in AllBinders())
                    files.AddRange(binder.GetFiles().Where(f => f.IsDirty).Select(f => (binder, f)));
                break;

            case CheckScope.Binder:
                var selected = View.Selection.SelectedBinder;
                if (selected != null)
                    files.AddRange(selected.GetFiles().Select(f => (selected, f)));
                break;

            case CheckScope.All:
                foreach (var binder in AllBinders())
                    files.AddRange(binder.GetFiles().Select(f => (binder, f)));
                break;
        }

        // Errors first, then by binder and binder order
        Results = files
            .SelectMany(x => MaterialEditorChecks.CheckFile(Bank, x.binder, x.file, View.Editor).Select(f => new Result(x.binder, x.file, f)))
            .OrderBy(r => r.Finding.Severity)
            .ThenBy(r => r.Binder.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        CheckedCount = files.Count;
    }

    private void JumpTo(Result result)
    {
        // The file may have been removed or renamed since the check ran
        if (result.Binder.GetFile(result.File.Key) != result.File)
            return;

        var entry = Bank.FindBinderEntry(result.Binder);
        if (entry == null)
            return;

        View.Selection.JumpTo(Bank, new MaterialReference(result.Binder.SourceType, entry, result.File.Key, result.File));

        if (result.Finding.Target != "")
            View.Properties.SetFilter(result.Finding.Target);
    }
}
