using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TextEditor;

public class GlobalTextSearch
{
    public TextEditorView View;
    public ProjectEntry Project;

    private string searchTerm = "";
    private bool ignoreCase = false;
    private SearchFilterType filterType = SearchFilterType.PrimaryCategory;
    private SearchMatchType matchType = SearchMatchType.All;

    private List<TextResult> searchResults = new();

    private bool hasAlreadySearched = false;

    public GlobalTextSearch(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        ImGui.BeginChild("TextSearchSection", ImGuiChildFlags.Borders);

        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextSearch_Header_Search_Filter"),
            LOC.Get("TEXT_TextSearch_Header_Search_Filter_TT"));

        UIHelper.SinglelineTextInput("textSearchInput", ref searchTerm);

        // Filter Type
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextSearch_Header_Filter_Type"),
            LOC.Get("TEXT_TextSearch_Header_Filter_Type_TT"));

        var filterPreviewName = LOC.Get(filterType.GetDisplayName());

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
        UIHelper.Tooltip(LOC.Get("TEXT_TextSearch_Filter_Type_TT"));

        // Match Type
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextSearch_Header_Match_Type"),
            LOC.Get("TEXT_TextSearch_Header_Match_Type_TT"));

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
        UIHelper.Tooltip(LOC.Get("TEXT_TextSearch_Match_Type_TT"));

        // Options
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextSearch_Header_Options"),
            LOC.Get("TEXT_TextSearch_Header_Options_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXT_TextSearch_Checkbox_Ignore_Case")}##ignoreCase", ref ignoreCase);
        UIHelper.Tooltip(LOC.Get("TEXT_TextSearch_Checkbox_Ignore_Case_TT"));

        // Actions
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextSearch_Heder_Actions"),
            LOC.Get("TEXT_TextSearch_Heder_Actions_TT"));

        UIHelper.MultiButtonInput("searchActions",
            "searchText", 
            LOC.Get("TEXT_TextSearch_Action_Search"),
            LOC.Get("TEXT_TextSearch_Action_Search_TT"),
            SearchText,

            "clearResults",
            LOC.Get("TEXT_TextSearch_Action_Clear_Result"),
            LOC.Get("TEXT_TextSearch_Action_Clear_Result_TT"), 
            ClearResults,

            "copyResults",
            LOC.Get("TEXT_TextSearch_Action_Copy_Results"),
            LOC.Get("TEXT_TextSearch_Action_Copy_Results_TT"), 
            CopyResultsToClipboard);

        // Results
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextSearch_Header_Results"),
            LOC.Get("TEXT_TextSearch_Header_Results_TT"));

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
            UIHelper.WrappedText(LOC.Get("TEXT_TextSearch_Already_Searched"));
        }

        ImGui.EndChild();


        ImGui.EndChild();
    }

    public void SearchText()
    {
        if (searchTerm == "")
        {
            Smithbox.LogError<GlobalTextSearch>(LOC.Get("TEXT_TextSearch_No_Search_Term"));
            return;
        }

        // Check the regex before entering GetGlobalTextResult
        try
        {
            var match = Regex.Match("example", searchTerm);
        }
        catch (Exception ex)
        {
            Smithbox.LogError<GlobalTextReplacement>(LOC.Get("TEXT_TextSearch_Invalid_Regex"), ex);
            return;
        }


        hasAlreadySearched = true;
        searchResults = TextFinder.GetGlobalTextResult(View, searchTerm, filterType, matchType, ignoreCase);
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
            Smithbox.LogError<GlobalTextSearch>(LOC.Get("TEXT_TextSearch_No_Results_To_Copy"));
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
