using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Material Search and Mass Edit (Material Editor plan, F8): finds materials, params, samplers and textures across the
/// binders of the current tab by name, shader, value and path, and edits the results' values or paths as one undo step.
/// Searches run on the main thread: a whole-game search takes under 0.1 s (MaterialEditorSurvey query).
/// </summary>
public class MatSearchTool
{
    public MaterialEditorView View;
    public ProjectEntry Project;

    private enum SearchScope { Binder, All }

    // Inputs
    private SearchScope Scope = SearchScope.All;
    private string MaterialText = "";
    private string ShaderText = "";
    private string EntryText = "";
    private string ValueText = "";
    private string PathText = "";
    private MaterialComparison Comparison = MaterialComparison.Contains;
    private bool Exact;

    // Mass edit inputs
    private MaterialValueEdit.Operation ValueOperation = MaterialValueEdit.Operation.Set;
    private string ValueInput = "";
    private string FindText = "";
    private string ReplaceText = "";

    private record Hit(IMaterialBinder Binder, MaterialFileWrapper File, object Entry, string List, string Label);

    private List<Hit> Results;
    private MaterialQuery LastQuery;
    private string Message = "";

    private const int MaxShown = 20000;

    public MatSearchTool(MaterialEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private MaterialBank Bank => Project.Handler.MaterialData.PrimaryBank;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MAT_SearchTool_Header")}##materialSearchToolHeader"))
            return;

        ImGui.BeginChild("MaterialSearchToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MAT_SearchTool_Hint"));

        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_SearchTool_Scope_Header"), LOC.Get("MAT_SearchTool_Scope_Header_TT"));

        if (ImGui.RadioButton($"{LOC.Get("MAT_SearchTool_Scope_All")}##materialSearchScopeAll", Scope == SearchScope.All))
            Scope = SearchScope.All;
        if (ImGui.RadioButton($"{LOC.Get("MAT_SearchTool_Scope_Binder")}##materialSearchScopeBinder", Scope == SearchScope.Binder))
            Scope = SearchScope.Binder;

        // Material
        TextInput("MAT_SearchTool_Material", "##materialSearchMaterial", ref MaterialText);
        TextInput("MAT_SearchTool_Shader", "##materialSearchShader", ref ShaderText);

        // Entry
        TextInput("MAT_SearchTool_Entry", "##materialSearchEntry", ref EntryText);

        // Value, with its comparison
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_SearchTool_Value_Header"), LOC.Get("MAT_SearchTool_Value_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##materialSearchComparison", LOC.Get($"MAT_SearchTool_Comparison_{Comparison}")))
        {
            foreach (var comparison in Enum.GetValues<MaterialComparison>())
            {
                if (ImGui.Selectable(LOC.Get($"MAT_SearchTool_Comparison_{comparison}"), comparison == Comparison))
                    Comparison = comparison;
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAT_SearchTool_Comparison_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##materialSearchValue", ref ValueText, 255);
        GUI.Tooltip(LOC.Get("MAT_SearchTool_Value_Header_TT"));

        // Path
        TextInput("MAT_SearchTool_Path", "##materialSearchPath", ref PathText);

        ImGui.Checkbox($"{LOC.Get("MAT_SearchTool_Exact")}##materialSearchExact", ref Exact);
        GUI.Tooltip(LOC.Get("MAT_SearchTool_Exact_TT"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_SearchTool_Actions_Header"), LOC.Get("MAT_SearchTool_Actions_Header_TT"));

        var query = CurrentQuery();
        var canSearch = query.HasAny && (Scope == SearchScope.All || View.Selection.SelectedBinder != null);

        GUI.ConditionalMultiButtonInput("materialSearchActions",
            "materialSearchRun", LOC.Get("MAT_SearchTool_Search"), LOC.Get("MAT_SearchTool_Search_TT"),
            () => Search(query), canSearch,

            "materialSearchClear", LOC.Get("MAT_SearchTool_Clear"), LOC.Get("MAT_SearchTool_Clear_TT"),
            Clear, true);

        if (Scope == SearchScope.Binder && View.Selection.SelectedBinder == null)
            GUI.WrappedText(LOC.Get("MAT_SearchTool_No_Binder"));

        DisplayResults();
        DisplayMassEdit();
    }

    private static void TextInput(string key, string id, ref string text)
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get($"{key}_Header"), LOC.Get($"{key}_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText(id, ref text, 255);
        GUI.Tooltip(LOC.Get($"{key}_Header_TT"));
    }

    private MaterialQuery CurrentQuery() =>
        new(MaterialText, ShaderText, EntryText, ValueText, PathText, Comparison, Exact);

    private void Clear()
    {
        MaterialText = ShaderText = EntryText = ValueText = PathText = "";
        Comparison = MaterialComparison.Contains;
        Exact = false;
        Results = null;
        LastQuery = null;
        Message = "";
    }

    #region Search
    private IEnumerable<IMaterialBinder> Binders()
    {
        if (Scope == SearchScope.Binder)
            return View.Selection.SelectedBinder is { } binder ? [binder] : [];

        return View.Selection.SourceType is MaterialSourceType.MTD
            ? Bank.MTDs.Values.OrderBy(b => b.FilePath)
            : Bank.MATBINs.Values.OrderBy(b => b.FilePath);
    }

    private void Search(MaterialQuery query)
    {
        var results = new List<Hit>();

        foreach (var binder in Binders())
        {
            foreach (var key in binder.FileOrder)
            {
                var file = binder.GetFile(key);
                if (file == null)
                    continue;

                var shortName = MaterialBank.ShortNameOf(key);
                var material = MaterialVanilla.MaterialOf(file);

                if (!MaterialQueryMatcher.MatchMaterial(query, shortName, material))
                    continue;

                if (!query.MatchesEntries)
                {
                    results.Add(new Hit(binder, file, null, null, shortName));
                    continue;
                }

                foreach (var (entry, list) in MaterialQueryMatcher.Entries(material))
                {
                    if (MaterialQueryMatcher.MatchEntry(query, entry))
                        results.Add(new Hit(binder, file, entry, list, $"{shortName} · {Describe(entry)}"));
                }
            }
        }

        Results = results;
        LastQuery = query;
        Message = "";
    }

    private static string Describe(object entry) => entry switch
    {
        MTD.Param p => $"{p.Name} = {MaterialQueryMatcher.ValueText(p.Value)}",
        MATBIN.Param p => $"{p.Name} = {MaterialQueryMatcher.ValueText(p.Value)}",
        MATBIN.Sampler s => $"{s.Type} = {s.Path}",
        MTD.Texture t => $"{t.Type} = {t.Path}",
        _ => ""
    };

    private void DisplayResults()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_SearchTool_Results_Header"), LOC.Get("MAT_SearchTool_Results_Header_TT"));

        if (Results == null)
        {
            GUI.WrappedText(LOC.Get("MAT_SearchTool_Not_Run"));
            return;
        }

        var materials = Results.Select(r => r.File).Distinct().Count();
        GUI.WrappedText(LOC.Get("MAT_SearchTool_Results_Summary", Results.Count, materials));

        if (Results.Count > MaxShown)
            GUI.WrappedText(LOC.Get("MAT_SearchTool_Results_Capped", MaxShown));

        if (Results.Count == 0)
            return;

        var height = Math.Min(Results.Count, 12) * ImGui.GetTextLineHeightWithSpacing() + 8;

        if (ImGui.BeginChild("materialSearchResults", new Vector2(0, height), ImGuiChildFlags.Borders))
        {
            var clipper = new ImGuiListClipper();
            clipper.Begin(Math.Min(Results.Count, MaxShown));

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    if (ImGui.Selectable($"{Results[i].Label}##materialSearchResult{i}"))
                        JumpTo(Results[i]);
                }
            }

            clipper.End();
        }

        ImGui.EndChild();
    }

    private void JumpTo(Hit hit)
    {
        // The file may have been removed or renamed since the search
        if (hit.Binder.GetFile(hit.File.Key) != hit.File)
            return;

        var entry = Bank.FindBinderEntry(hit.Binder);
        if (entry == null)
            return;

        View.Selection.JumpTo(Bank, new MaterialReference(hit.Binder.SourceType, entry, hit.File.Key, hit.File));

        var name = MaterialVanilla.EntryName(hit.Entry);
        if (!string.IsNullOrEmpty(name))
            View.Properties.SetFilter(name);
    }
    #endregion

    #region Mass Edit
    private void DisplayMassEdit()
    {
        if (Results == null || Results.Count == 0 || LastQuery == null || !LastQuery.MatchesEntries)
            return;

        var paramHits = Results.Where(r => r.Entry is MTD.Param or MATBIN.Param).ToList();
        var pathHits = Results.Where(r => r.Entry is MATBIN.Sampler or MTD.Texture).ToList();

        // Param values
        if (paramHits.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAT_SearchTool_Edit_Values_Header", paramHits.Count), LOC.Get("MAT_SearchTool_Edit_Values_Header_TT"));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo("##materialMassEditOperation", LOC.Get($"MAT_SearchTool_Operation_{ValueOperation}")))
            {
                foreach (var operation in Enum.GetValues<MaterialValueEdit.Operation>())
                {
                    if (ImGui.Selectable(LOC.Get($"MAT_SearchTool_Operation_{operation}"), operation == ValueOperation))
                        ValueOperation = operation;
                }
                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("MAT_SearchTool_Operation_TT"));

            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##materialMassEditValue", ref ValueInput, 255);
            GUI.Tooltip(LOC.Get("MAT_SearchTool_Operation_TT"));

            GUI.ConditionalMultiButtonInput("materialMassEditValueActions",
                "materialMassEditValueApply", LOC.Get("MAT_SearchTool_Apply"), LOC.Get("MAT_SearchTool_Apply_Values_TT"),
                () => ApplyValues(paramHits), !string.IsNullOrWhiteSpace(ValueInput));
        }

        // Sampler / texture paths
        if (pathHits.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAT_SearchTool_Edit_Paths_Header", pathHits.Count), LOC.Get("MAT_SearchTool_Edit_Paths_Header_TT"));

            ImGui.Text(LOC.Get("MAT_SearchTool_Find"));
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##materialMassEditFind", ref FindText, 255);
            GUI.Tooltip(LOC.Get("MAT_SearchTool_Edit_Paths_Header_TT"));

            ImGui.Text(LOC.Get("MAT_SearchTool_Replace"));
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##materialMassEditReplace", ref ReplaceText, 255);
            GUI.Tooltip(LOC.Get("MAT_SearchTool_Edit_Paths_Header_TT"));

            GUI.ConditionalMultiButtonInput("materialMassEditPathActions",
                "materialMassEditPathApply", LOC.Get("MAT_SearchTool_Apply"), LOC.Get("MAT_SearchTool_Apply_Paths_TT"),
                () => ApplyPaths(pathHits), !string.IsNullOrEmpty(FindText));
        }

        if (!string.IsNullOrEmpty(Message))
            GUI.WrappedText(Message);
    }

    /// <summary>
    /// Sets, adds to or multiplies the value of every param result, as one undo step. Values the input doesn't fit
    /// (a number for a bool, three numbers for a two-number value) are left and counted.
    /// </summary>
    private void ApplyValues(List<Hit> hits)
    {
        var actions = new List<EditorAction>();
        var skipped = 0;

        foreach (var hit in hits)
        {
            var (value, prop) = hit.Entry switch
            {
                MTD.Param p => (p.Value, typeof(MTD.Param).GetProperty(nameof(MTD.Param.Value))),
                MATBIN.Param p => (p.Value, typeof(MATBIN.Param).GetProperty(nameof(MATBIN.Param.Value))),
                _ => ((object)null, (PropertyInfo)null)
            };

            if (prop == null || !MaterialValueEdit.TryApply(value, ValueOperation, ValueInput, out var result))
            {
                skipped++;
                continue;
            }

            actions.Add(new MatPropertyChange(View, hit.File, prop, hit.Entry, result));
        }

        Finish(actions, skipped);
    }

    /// <summary>
    /// Replaces text in the path of every sampler / texture result, as one undo step
    /// </summary>
    private void ApplyPaths(List<Hit> hits)
    {
        var actions = new List<EditorAction>();
        var skipped = 0;

        foreach (var hit in hits)
        {
            var (path, prop) = hit.Entry switch
            {
                MATBIN.Sampler s => (s.Path, typeof(MATBIN.Sampler).GetProperty(nameof(MATBIN.Sampler.Path))),
                MTD.Texture t => (t.Path, typeof(MTD.Texture).GetProperty(nameof(MTD.Texture.Path))),
                _ => ((string)null, (PropertyInfo)null)
            };

            var replaced = prop == null ? null : MaterialValueEdit.ReplaceInPath(path, FindText, ReplaceText);

            if (replaced == null)
            {
                skipped++;
                continue;
            }

            actions.Add(new MatPropertyChange(View, hit.File, prop, hit.Entry, replaced));
        }

        Finish(actions, skipped);
    }

    private void Finish(List<EditorAction> actions, int skipped)
    {
        if (actions.Count > 0)
            View.ActionManager.ExecuteAction(new CompoundAction(actions));

        Message = LOC.Get("MAT_SearchTool_Applied", actions.Count, skipped);

        // The results show the new values
        if (LastQuery != null)
        {
            var message = Message;
            Search(LastQuery);
            Message = message;
        }
    }
    #endregion
}
