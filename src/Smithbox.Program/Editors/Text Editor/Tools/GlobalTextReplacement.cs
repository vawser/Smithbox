using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
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

        UIHelper.SimpleHeader("Search Filter", "The term to find.\n\nCan use regular expressions.");
        UIHelper.SinglelineTextInput("textSearchInput", ref searchTerm);

        UIHelper.SimpleHeader("Replacement Filter", "The term the found term is replaced with.\n\nCan use regular expressions.");
        UIHelper.SinglelineTextInput("textReplaceInput", ref replaceTerm);

        UIHelper.SimpleHeader("Filter Type", "");

        DPI.ApplyStandardInputWidth();
        if (ImGui.BeginCombo("##searchFilterType", filterType.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(SearchFilterType)))
            {
                var filterEntry = (SearchFilterType)entry;

                if (ImGui.Selectable(filterEntry.GetDisplayName()))
                {
                    filterType = filterEntry;
                }
            }

            ImGui.EndCombo();
        }
        UIHelper.Tooltip("The search filter to use.");

        UIHelper.SimpleHeader("Match Type", "");

        DPI.ApplyStandardInputWidth();
        if (ImGui.BeginCombo("##searchMatchType", matchType.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(SearchMatchType)))
            {
                var matchType = (SearchMatchType)entry;

                if (ImGui.Selectable(matchType.GetDisplayName()))
                {
                    this.matchType = matchType;
                }
            }

            ImGui.EndCombo();
        }
        UIHelper.Tooltip("The contents to match with.");

        UIHelper.SimpleHeader("Options", "");

        ImGui.Checkbox("Ignore Case##ignoreCase", ref ignoreCase);
        UIHelper.Tooltip("Specifies case-insensitive matching for regex.");

        ImGui.Checkbox("Use Multi-line Regex##multilineRegex", ref applyMultilineRegex);
        UIHelper.Tooltip("Multiline mode for regex. Changes the meaning of ^ and $ so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.");

        ImGui.Checkbox("Use Single-line Regex##singleLineRegex", ref applySinglelineRegex);
        UIHelper.Tooltip("Specifies single-line mode for regex. Changes the meaning of the dot (.) so it matches every character (instead of every character except \\n).");

        ImGui.Checkbox("Ignore Pattern Whitespace##ignorePatternWhitespace", ref ignorePatternWhitespace);
        UIHelper.Tooltip("Eliminates unescaped white space from the pattern and enables comments marked with #. However, this value does not affect or eliminate white space in character classes, numeric quantifiers, or tokens that mark the beginning of individual regular expression language elements.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("replaceActions",
            "previewEdit", "Preview Edit", "", PreviewEdit,
            "clearPreview", "Clear Preview", "", ClearPreview,
            "applyReplacement", "Apply Replace", "All the entries listed in the list below will have the Replacement Input regex applied to them.", ApplyReplacement);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Entries to Edit", "");

        ImGui.BeginChild("previewSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (replacementResults.Count > 0)
        {
            UIHelper.WrappedText("Entries that will be affected:");

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
            UIHelper.WrappedText("No text entries found matching the filter.");
        }

        ImGui.EndChild();

        ImGui.EndChild();
    }

    public void PreviewEdit()
    {
        var curView = Editor.ViewHandler.ActiveView;

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