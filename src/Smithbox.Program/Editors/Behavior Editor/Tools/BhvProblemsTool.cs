using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// Problems: runs <see cref="BhvChecks"/> over the files in scope. TAE animation IDs are looked up in the
/// character's time acts through the Time Act Editor, and param rows in the Param Editor's data; each check is
/// skipped where that data isn't loaded. Vanilla baseline: no errors; 434 missing-TAE warnings in 22 files.
/// </summary>
public class BhvProblemsTool
{
    private readonly BhvEditorView View;
    private readonly ProjectEntry Project;

    private readonly BhvToolScope Scope;
    private bool ShowNotes = false;
    private bool CheckTae = true;

    private List<(BhvSeverity Severity, BhvToolResult Result)> Results;
    private List<BhvToolResult> Visible;
    private bool VisibleNotes;
    private int CheckedFiles, Errors, Warnings, Notes;
    private string Skipped = "";

    public BhvProblemsTool(BhvEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        Scope = new(view, "bhvProblemsScope", BhvScope.Binder);
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("BHV_ProblemsTool_Header")}##behaviorProblemsToolHeader"))
            return;

        ImGui.BeginChild("BehaviorProblemsToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("BHV_ProblemsTool_Hint"));

        var canRun = Scope.Draw();

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("BHV_ProblemsTool_Options_Header"), LOC.Get("BHV_ProblemsTool_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("BHV_ProblemsTool_Check_Tae")}##behaviorProblemsCheckTae", ref CheckTae);
        GUI.Tooltip(LOC.Get("BHV_ProblemsTool_Check_Tae_TT"));

        ImGui.Checkbox($"{LOC.Get("BHV_ProblemsTool_Show_Notes")}##behaviorProblemsShowNotes", ref ShowNotes);
        GUI.Tooltip(LOC.Get("BHV_ProblemsTool_Show_Notes_TT"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("BHV_ProblemsTool_Actions_Header"), LOC.Get("BHV_ProblemsTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("behaviorProblemsActions",
            "behaviorProblemsRun", LOC.Get("BHV_ProblemsTool_Run"), LOC.Get("BHV_ProblemsTool_Run_TT"),
            Run, canRun);

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("BHV_ProblemsTool_Results_Header"), LOC.Get("BHV_ProblemsTool_Results_Header_TT"));

        if (Results == null)
        {
            GUI.WrappedText(LOC.Get("BHV_Tool_Not_Run"));
            return;
        }

        GUI.WrappedText(LOC.Get("BHV_ProblemsTool_Summary", CheckedFiles, Errors, Warnings, Notes));
        if (Skipped.Length > 0)
            GUI.WrappedText(Skipped);

        if (Visible == null || VisibleNotes != ShowNotes)
        {
            VisibleNotes = ShowNotes;
            Visible = Results.Where(r => ShowNotes || r.Severity != BhvSeverity.Note).Take(BhvResultList.MaxResults).Select(r => r.Result).ToList();
        }

        if (Results.Count(r => ShowNotes || r.Severity != BhvSeverity.Note) > Visible.Count)
            GUI.WrappedText(LOC.Get("BHV_Tool_Results_Capped", Visible.Count));

        BhvResultList.Draw(View, "BehaviorProblemsResults", Visible);
    }

    private void Run()
    {
        var files = Scope.Files();
        var context = BuildContext(files, out var skipped);

        Results = new();
        Errors = Warnings = Notes = 0;

        foreach (var file in files)
        {
            var b = file.BHV;
            var ctx = context(file);

            foreach (var f in BhvChecks.Check(b, ctx))
            {
                switch (f.Severity)
                {
                    case BhvSeverity.Error: Errors++; break;
                    case BhvSeverity.Warning: Warnings++; break;
                    default: Notes++; break;
                }

                var (tag, color) = f.Severity switch
                {
                    BhvSeverity.Error => (LOC.Get("BHV_ProblemsTool_Error"), UI.Current.ImGui_Warning_Text_Color),
                    BhvSeverity.Warning => (LOC.Get("BHV_ProblemsTool_Warning"), UI.Current.ImGui_Invalid_Text_Color),
                    _ => (LOC.Get("BHV_ProblemsTool_Note"), UI.Current.ImGui_Default_Text_Color),
                };

                var where = BhvDisplay.LocationLabel(b, f.Kind, f.State, f.Transition, f.Condition, f.Index);
                Results.Add((f.Severity, new(file, f.Kind, f.State, f.Transition, f.Condition, f.Index, tag, color,
                    $"{where}:  {LOC.Get($"BHV_Check_{f.Code}", f.Args)}")));
            }
        }

        // Errors first across every file, then warnings, then notes (stable: file order within each)
        Results = Results.OrderBy(r => r.Severity).ToList();
        Visible = null;
        CheckedFiles = files.Count;
        Skipped = skipped;
    }

    /// <summary>
    /// The lookups outside the files: param rows (one set per param for the run) and each character's TAE IDs.
    /// <paramref name="skipped"/> says which checks had no data.
    /// </summary>
    private Func<BhvFileWrapper, BhvCheckContext> BuildContext(List<BhvFileWrapper> files, out string skipped)
    {
        var notes = new List<string>();

        var matrixRows = ParamRows(BhvDisplay.ChangeStateMatrixParam);
        var spEffectRows = ParamRows(BhvDisplay.SpEffectParam);
        if (matrixRows == null || spEffectRows == null)
            notes.Add(LOC.Get("BHV_ProblemsTool_Skipped_Params"));

        var taeByCharacter = new Dictionary<string, HashSet<long>>(StringComparer.OrdinalIgnoreCase);
        var missingCharacters = new List<string>();

        if (CheckTae)
        {
            foreach (var name in files.Select(f => f.Binder.Name).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var ids = TaeIds(name);
                if (ids != null)
                    taeByCharacter[name] = ids;
                else
                    missingCharacters.Add(name);
            }

            if (missingCharacters.Count > 0)
                notes.Add(LOC.Get("BHV_ProblemsTool_Skipped_Tae", missingCharacters.Count, string.Join(", ", missingCharacters.Take(8))));
        }

        skipped = string.Join("\n", notes);

        return file =>
        {
            var ctx = new BhvCheckContext();

            if (matrixRows != null)
                ctx.HasChangeStateMatrixRow = id => matrixRows.Contains(id);
            if (spEffectRows != null)
                ctx.HasSpEffectRow = id => spEffectRows.Contains(id);
            if (taeByCharacter.TryGetValue(file.Binder.Name, out var tae))
                ctx.HasTaeAnimation = id => tae.Contains(id);

            return ctx;
        };
    }

    private HashSet<int> ParamRows(string param)
    {
        var bank = Project.Handler.ParamData?.PrimaryBank;
        if (bank?.Params == null || !bank.Params.TryGetValue(param, out var p))
            return null;

        return p.Rows.Select(r => r.ID).ToHashSet();
    }

    /// <summary>
    /// Every full animation ID in a character's time acts (as the Time Act Editor numbers them, which is how the
    /// BHV's TAE IDs read), or null if the Time Act Editor doesn't list the character
    /// </summary>
    private HashSet<long> TaeIds(string character)
    {
        var source = Project.Handler.TimeActData?.PrimaryBank?.GetCharacters()
            .FirstOrDefault(c => c.Name.Equals(character, StringComparison.OrdinalIgnoreCase));

        if (source?.TimeActs == null)
            return null;

        var size = source.AnimationCategorySize;
        var ids = new HashSet<long>();

        foreach (var entry in source.TimeActs)
        {
            entry.Load();
            if (entry.TimeAct == null)
                continue;

            foreach (var animation in entry.TimeAct.Animations)
                ids.Add(entry.GetFullAnimationID(animation, size));
        }

        return ids.Count > 0 ? ids : null;
    }
}
