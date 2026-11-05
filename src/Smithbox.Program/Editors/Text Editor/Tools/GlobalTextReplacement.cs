using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TextEditor;

public class GlobalTextReplacement
{
    public TextEditorView View;
    public ProjectEntry Project;

    private string searchTerm = "";
    private string replaceTerm = "";

    private SearchFilterType filterType = SearchFilterType.PrimaryCategory;
    private SearchMatchType matchType = SearchMatchType.All;

    private List<ReplacementResult> replacementResults = new();

    private bool ignoreCase = false;
    private bool applyMultilineRegex = false;
    private bool applySinglelineRegex = false;
    private bool ignorePatternWhitespace = false;

    private bool hasAlreadySearched = false;

    public GlobalTextReplacement(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        ImGui.BeginChild("TextReplaceSection", ImGuiChildFlags.Borders);

        // Search Filter
        GUI.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Search_Filter"),
            LOC.Get("TEXT_TextReplacement_Header_Search_Filter_TT"));

        GUI.SinglelineTextInput("textSearchInput", ref searchTerm);

        // Replacement Filter
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Replacement_Filter"),
            LOC.Get("TEXT_TextReplacement_Header_Replacement_Filter_TT"));

        GUI.SinglelineTextInput("textReplaceInput", ref replaceTerm);

        // Filter Type
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Filter_Type"),
            LOC.Get("TEXT_TextReplacement_Header_Filter_Type_TT"));

        var filterPreviewName = LOC.Get(filterType.GetDisplayName());

        GUI.SetInputWidth();
        if (ImGui.BeginCombo("##searchFilterType", filterPreviewName))
        {
            foreach (var entry in Enum.GetValues(typeof(SearchFilterType)))
            {
                var filterEntry = (SearchFilterType)entry;

                var displayName = LOC.Get(filterEntry.GetDisplayName());

                if (ImGui.Selectable(displayName))
                {
                    filterType = filterEntry;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TEXT_TextReplacement_Filter_Type_TT"));

        // Match Type
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Match_Type"),
            LOC.Get("TEXT_TextReplacement_Header_Match_Type_TT"));

        var matchPreviewName = LOC.Get(matchType.GetDisplayName());

        GUI.SetInputWidth();
        if (ImGui.BeginCombo("##searchMatchType", matchPreviewName))
        {
            foreach (var entry in Enum.GetValues(typeof(SearchMatchType)))
            {
                var matchType = (SearchMatchType)entry;

                var displayName = LOC.Get(matchType.GetDisplayName());

                if (ImGui.Selectable(displayName))
                {
                    this.matchType = matchType;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TEXT_TextReplacement_Match_Type_TT"));

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Options"),
            LOC.Get("TEXT_TextReplacement_Header_Options_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXT_TextReplacement_Checkbox_Ignore_Case")}##ignoreCase", ref ignoreCase);
        GUI.Tooltip(LOC.Get("TEXT_TextReplacement_Checkbox_Ignore_Case_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXT_TextReplacement_Checkbox_Multiline_Regex")}##multilineRegex", ref applyMultilineRegex);
        GUI.Tooltip(LOC.Get("TEXT_TextReplacement_Checkbox_Multiline_Regex_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXT_TextReplacement_Checkbox_Singleline_Regex")}##singleLineRegex", ref applySinglelineRegex);
        GUI.Tooltip(LOC.Get("TEXT_TextReplacement_Checkbox_Singleline_Regex_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXT_TextReplacement_Checkbox_Ignore_Pattern_Whitespace")}##ignorePatternWhitespace", ref ignorePatternWhitespace);
        GUI.Tooltip(LOC.Get("TEXT_TextReplacement_Checkbox_Ignore_Pattern_Whitespace_TT"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Actions"),
            LOC.Get("TEXT_TextReplacement_Header_Actions_TT"));

        GUI.MultiButtonInput("replaceActions",
            "previewEdit", 
            LOC.Get("TEXT_TextReplacement_Action_Preview_Edit"),
            LOC.Get("TEXT_TextReplacement_Action_Preview_Edit_TT"),
            PreviewEdit,

            "clearPreview",
            LOC.Get("TEXT_TextReplacement_Action_Clear_Preview"),
            LOC.Get("TEXT_TextReplacement_Action_Clear_Preview_TT"), 
            ClearPreview,

            "applyReplacement",
            LOC.Get("TEXT_TextReplacement_Apply_Replacement"),
            LOC.Get("TEXT_TextReplacement_Apply_Replacement_TT"), 
            ApplyReplacement);

        // Preview
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Preview_List"),
            LOC.Get("TEXT_TextReplacement_Header_Preview_List_TT"));

        if (replacementResults.Count > 0)
        {
            // Counts and selection buttons stay outside the list, so they're always visible
            var includedCount = replacementResults.Count(r => r.Included);

            GUI.WrappedText(LOC.Get("TEXT_TextReplacement_Preview_List_Hint"));
            GUI.WrappedText(LOC.Get("TEXT_TextReplacement_Preview_Selected_Count", includedCount, replacementResults.Count));

            GUI.MultiButtonInput("replacePreviewSelection",
                "selectAllResults",
                LOC.Get("TEXT_TextReplacement_Select_All"),
                LOC.Get("TEXT_TextReplacement_Select_All_TT"),
                () => SetAllIncluded(true),

                "selectNoResults",
                LOC.Get("TEXT_TextReplacement_Select_None"),
                LOC.Get("TEXT_TextReplacement_Select_None_TT"),
                () => SetAllIncluded(false));
        }

        ImGui.BeginChild("previewSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (replacementResults.Count > 0)
        {
            // Clipped: a broad pattern can match thousands of entries
            var clipper = new ImGuiListClipper();
            clipper.Begin(replacementResults.Count);

            while (clipper.Step())
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var result = replacementResults[i];

                // Per-result opt out: unticked entries are left unchanged by Apply
                var included = result.Included;
                if (ImGui.Checkbox($"##replaceInclude{i}", ref included))
                {
                    result.Included = included;
                }
                GUI.Tooltip(LOC.Get("TEXT_TextReplacement_Include_TT"));

                ImGui.SameLine();

                var foundText = result.Entry.Text;
                if (foundText != null)
                {
                    if (foundText.Contains("\n"))
                    {
                        var firstSection = foundText.Split("\n")[0];
                        foundText = $"{firstSection} <...>";
                    }
                }
                else
                {
                    foundText = $"<null>";
                }

                var category = result.ContainerWrapper.ContainerDisplayCategory.ToString();

                // Container
                var containerName = result.ContainerName;
                if (CFG.Current.TextEditor_Container_List_Display_Community_Names)
                {
                    containerName = result.ContainerWrapper.GetContainerDisplayName();
                }

                // FMG
                var fmgName = result.FmgName;
                if (CFG.Current.TextEditor_Text_File_List_Display_Community_Names)
                {
                    fmgName = TextUtils.GetFmgDisplayName(View.Project, result.ContainerWrapper, result.FmgID, result.FmgName);
                }

                var displayText = $"{containerName} - {fmgName} - {result.Entry.ID}: {foundText}";

                if (ImGui.Selectable($"{displayText}##replaceResult{i}"))
                {
                    EditorCommandQueue.AddCommand($"text/select/{category}/{result.ContainerName}/{result.FmgName}/{result.Entry.ID}");
                }
                GUI.Tooltip(LOC.Get("TEXT_TextReplacement_Preview_Entry_TT", result.Entry.Text, result.NewText));
            }

            clipper.End();
        }
        else if (hasAlreadySearched)
        {
            GUI.WrappedText(LOC.Get("TEXT_TextReplacement_Already_Searched"));
        }

        ImGui.EndChild();

        ImGui.EndChild();
    }

    /// <summary>
    /// The one regex used by both Preview and Apply, so they always agree.
    /// Ignore case is a regex option rather than lowercasing the pattern, which turned
    /// escapes like \S, \D and \W into \s, \d and \w. No ExplicitCapture, so $1 works.
    /// </summary>
    private Regex BuildSearchRegex()
    {
        RegexOptions options = RegexOptions.CultureInvariant;

        if (ignoreCase)
        {
            options = options | RegexOptions.IgnoreCase;
        }
        if (applyMultilineRegex)
        {
            options = options | RegexOptions.Multiline;
        }
        if (applySinglelineRegex)
        {
            options = options | RegexOptions.Singleline;
        }
        if (ignorePatternWhitespace)
        {
            options = options | RegexOptions.IgnorePatternWhitespace;
        }

        return new Regex(searchTerm, options);
    }

    public void PreviewEdit()
    {
        Regex searchRegex;

        try
        {
            searchRegex = BuildSearchRegex();
        }
        catch (Exception ex)
        {
            Smithbox.LogError<GlobalTextReplacement>(LOC.Get("TEXT_TextReplacement_Invalid_Regex_Search"), ex);
            return;
        }

        // The replacement term is a substitution string, not a pattern, so it isn't
        // validated as a regex: "(" or "[" are valid replacements.

        hasAlreadySearched = true;

        var results = TextFinder.GetReplacementResult(View, searchRegex, searchTerm, filterType, matchType);

        replacementResults = new List<ReplacementResult>();

        foreach (var result in results)
        {
            result.NewText = searchRegex.Replace(result.Entry.Text, replaceTerm);

            // Only list entries the replacement would actually change
            if (result.NewText != result.Entry.Text)
            {
                replacementResults.Add(result);
            }
        }
    }

    private void SetAllIncluded(bool included)
    {
        foreach (var result in replacementResults)
        {
            result.Included = included;
        }
    }

    public void ClearPreview()
    {
        hasAlreadySearched = false;
        replacementResults.Clear();
    }

    public void ApplyReplacement()
    {
        if (replacementResults.Count == 0)
        {
            Smithbox.Log(this, LOC.Get("TEXT_TextReplacement_Preview_Required"));
            return;
        }

        Regex searchRegex;

        try
        {
            searchRegex = BuildSearchRegex();
        }
        catch (Exception ex)
        {
            Smithbox.LogError<GlobalTextReplacement>(LOC.Get("TEXT_TextReplacement_Invalid_Regex_Search"), ex);
            return;
        }

        List<EditorAction> actions = new List<EditorAction>();

        foreach (var result in replacementResults)
        {
            // Unticked in the preview
            if (!result.Included)
                continue;

            if (result.Entry.Text == null)
                continue;

            // Recompute from the current text, in case the entry was edited since the preview
            var newText = searchRegex.Replace(result.Entry.Text, replaceTerm);

            if (newText == result.Entry.Text)
                continue;

            actions.Add(new ChangeFmgEntryText(View, result.ContainerWrapper, result.Entry, newText));
        }

        if (actions.Count > 0)
        {
            var groupedAction = new FmgGroupedAction(actions);
            View.ActionManager.ExecuteAction(groupedAction);
        }

        Smithbox.Log(this, LOC.Get("TEXT_TextReplacement_Applied", actions.Count));

        // The preview is now stale; clear it so a second click can't apply the replacement again.
        replacementResults.Clear();
        hasAlreadySearched = false;
    }
}