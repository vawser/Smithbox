using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TextEditor;

public class GlobalTextReplacement
{
    public TextEditorScreen Editor;
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

    public GlobalTextReplacement(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        var curView = Editor.ViewHandler.ActiveView;

        ImGui.BeginChild("TextReplaceSection", ImGuiChildFlags.Borders);

        // Search Filter
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Search_Filter"),
            LOC.Get("TEXT_TextReplacement_Header_Search_Filter_TT"));

        UIHelper.SinglelineTextInput("textSearchInput", ref searchTerm);

        // Replacement Filter
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Replacement_Filter"),
            LOC.Get("TEXT_TextReplacement_Header_Replacement_Filter_TT"));

        UIHelper.SinglelineTextInput("textReplaceInput", ref replaceTerm);

        // Filter Type
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Filter_Type"),
            LOC.Get("TEXT_TextReplacement_Header_Filter_Type_TT"));

        var filterPreviewName = filterType.GetDisplayName();

        UIHelper.SetInputWidth();
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
        UIHelper.Tooltip(LOC.Get("TEXT_TextReplacement_Filter_Type_TT"));

        // Match Type
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Match_Type"),
            LOC.Get("TEXT_TextReplacement_Header_Match_Type_TT"));

        var matchPreviewName = LOC.Get(matchType.GetDisplayName());

        UIHelper.SetInputWidth();
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
        UIHelper.Tooltip(LOC.Get("TEXT_TextReplacement_Match_Type_TT"));

        // Options
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Options"),
            LOC.Get("TEXT_TextReplacement_Header_Options_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXT_TextReplacement_Checkbox_Ignore_Case")}##ignoreCase", ref ignoreCase);
        UIHelper.Tooltip(LOC.Get("TEXT_TextReplacement_Checkbox_Ignore_Case_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXT_TextReplacement_Checkbox_Multiline_Regex")}##multilineRegex", ref applyMultilineRegex);
        UIHelper.Tooltip(LOC.Get("TEXT_TextReplacement_Checkbox_Multiline_Regex_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXT_TextReplacement_Checkbox_Singleline_Regex")}##singleLineRegex", ref applySinglelineRegex);
        UIHelper.Tooltip(LOC.Get("TEXT_TextReplacement_Checkbox_Singleline_Regex_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXT_TextReplacement_Checkbox_Ignore_Pattern_Whitespace")}##ignorePatternWhitespace", ref ignorePatternWhitespace);
        UIHelper.Tooltip(LOC.Get("TEXT_TextReplacement_Checkbox_Ignore_Pattern_Whitespace_TT"));

        // Actions
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Actions"),
            LOC.Get("TEXT_TextReplacement_Header_Actions_TT"));

        UIHelper.MultiButtonInput("replaceActions",
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
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextReplacement_Header_Preview_List"),
            LOC.Get("TEXT_TextReplacement_Header_Preview_List_TT"));

        ImGui.BeginChild("previewSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (replacementResults.Count > 0)
        {
            UIHelper.WrappedText(LOC.Get("TEXT_TextReplacement_Preview_List_Hint"));

            foreach (var result in replacementResults)
            {
                // Ignore results from unused containers if in Simple mode
                if (CFG.Current.TextEditor_Container_List_Hide_Unused_Containers)
                {
                    if (result.ContainerWrapper.IsContainerUnused())
                    {
                        continue;
                    }
                }

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
                    fmgName = TextUtils.GetFmgDisplayName(curView.Project, result.ContainerWrapper, result.FmgID, result.FmgName);
                }

                var displayText = $"{containerName} - {fmgName} - {result.Entry.ID}: {foundText}";

                if (ImGui.Selectable(displayText))
                {
                    EditorCommandQueue.AddCommand($"text/select/{category}/{result.ContainerName}/{result.FmgName}/{result.Entry.ID}");
                }
            }
        }
        else if (hasAlreadySearched)
        {
            UIHelper.WrappedText(LOC.Get("TEXT_TextReplacement_Already_Searched"));
        }

        ImGui.EndChild();

        ImGui.EndChild();
    }

    public void PreviewEdit()
    {
        var curView = Editor.ViewHandler.ActiveView;

        try
        {
            var match = Regex.Match("example", searchTerm);
        }
        catch (Exception ex)
        {
            Smithbox.LogError<GlobalTextReplacement>(LOC.Get("TEXT_TextReplacement_Invalid_Regex_Search"), ex);
            return;
        }

        try
        {
            var match = Regex.Match("example", replaceTerm);
        }
        catch (Exception ex)
        {
            Smithbox.LogError<GlobalTextReplacement>(LOC.Get("TEXT_TextReplacement_Invalid_Regex_Replace"), ex);
            return;
        }

        hasAlreadySearched = true;
        replacementResults = TextFinder.GetReplacementResult(curView, searchTerm, filterType, matchType, ignoreCase);
    }

    public void ClearPreview()
    {
        hasAlreadySearched = false;
        replacementResults.Clear();
    }

    public void ApplyReplacement()
    {
        RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;

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

        var curView = Editor.ViewHandler.ActiveView;

        List<EditorAction> actions = new List<EditorAction>();

        string searchText = searchTerm;
        string replaceText = replaceTerm;

        foreach (var result in replacementResults)
        {
            var newText = Regex.Replace(result.Entry.Text, searchText, replaceText, options);
            actions.Add(new ChangeFmgEntryText(curView, result.ContainerWrapper, result.Entry, newText));
        }

        var groupedAction = new FmgGroupedAction(actions);
        curView.ActionManager.ExecuteAction(groupedAction);
    }
}