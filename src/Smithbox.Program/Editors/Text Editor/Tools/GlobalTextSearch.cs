using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TextEditor;

public class GlobalTextSearch
{
    public TextEditorScreen Editor;
    public ProjectEntry Project;

    private string searchTerm = "";
    private bool ignoreCase = false;
    private SearchFilterType filterType = SearchFilterType.PrimaryCategory;
    private SearchMatchType matchType = SearchMatchType.All;

    private List<TextResult> searchResults = new();

    private bool hasAlreadySearched = false;

    public GlobalTextSearch(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        var curView = Editor.ViewHandler.ActiveView;

        ImGui.BeginChild("TextSearchSection", ImGuiChildFlags.Borders);

        UIHelper.SimpleHeader("Search Filter", "");
        UIHelper.SinglelineTextInput("textSearchInput", ref searchTerm);

        UIHelper.SimpleHeader("Filter Type", "");

        UIHelper.SetInputWidth();
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

        UIHelper.SetInputWidth();
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
        UIHelper.Tooltip("Ignore case sensitivity if enabled.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("searchActions",
            "searchText", "Search", "", SearchText,
            "clearResults", "Clear Results", "", ClearResults,
            "copyResults", "Copy Results to Clipboard", "", CopyResultsToClipboard);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Results", "");

        ImGui.BeginChild("resultsSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (searchResults.Count > 0)
        {
            foreach (var result in searchResults)
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
                    fmgName = TextUtils.GetFmgDisplayName(Project, result.ContainerWrapper, result.FmgID, result.FmgName);
                }

                var displayText = $"{containerName} - {fmgName} - {result.Entry.ID}: {foundText}";

                if (ImGui.Selectable(displayText))
                {
                    EditorCommandQueue.AddCommand($"text/select/{category}/{result.ContainerName}/{result.FmgName}/{result.Entry.ID}");
                }
            }
        }
        else if(hasAlreadySearched)
        {
            UIHelper.WrappedText("No text entries found matching the filter.");
        }

        ImGui.EndChild();


        ImGui.EndChild();
    }

    public void SearchText()
    {
        var curView = Editor.ViewHandler.ActiveView;

        if (searchTerm == "")
        {
            Smithbox.LogError<GlobalTextSearch>("No search term specified.");
            return;
        }

        try
        {
            var match = Regex.Match("example", searchTerm);
        }
        catch (Exception ex)
        {
            Smithbox.LogError<GlobalTextReplacement>("Invalid regex in search filter", ex);
            return;
        }


        hasAlreadySearched = true;
        searchResults = TextFinder.GetGlobalTextResult(curView, searchTerm, filterType, matchType, ignoreCase);
    }

    public void ClearResults()
    {
        hasAlreadySearched = false;
        searchResults.Clear();
    }

    public void CopyResultsToClipboard()
    {
        if (searchResults.Count == 0)
        {
            Smithbox.LogError<GlobalTextSearch>("No results to copy.");
            return;
        }

        var list = new TextExportList();
        list.Entries = new();

        foreach (var result in searchResults)
        {
            var textExportEntry = new TextExportEntry();
            textExportEntry.ContainerName = result.ContainerName;
            textExportEntry.FmgName = result.FmgName;
            textExportEntry.EntryID = result.Entry.ID;
            textExportEntry.EntryText = result.Entry.Text;

            list.Entries.Add(textExportEntry);
        }

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            IncludeFields = true
        };

        var jsonText = JsonSerializer.Serialize(list, typeof(TextExportList), options);

        PlatformUtils.Instance.SetClipboardText(jsonText);
    }
}
