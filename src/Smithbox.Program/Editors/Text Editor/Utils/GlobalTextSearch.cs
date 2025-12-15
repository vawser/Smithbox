using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Program.Editors.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace StudioCore.Editors.TextEditor;

public static class GlobalTextSearch
{
    private static string _globalSearchInput = "";
    private static bool IgnoreCase = false;
    private static SearchFilterType FilterType = SearchFilterType.PrimaryCategory;
    private static SearchMatchType MatchType = SearchMatchType.All;

    private static List<TextResult> SearchResults = new();

    private static bool HasSearched = false;

    public static void Display(TextEditorScreen editor)
    {
        var windowSize = DPI.GetWindowSize(editor.BaseEditor._context);
        var sectionWidth = ImGui.GetWindowWidth() * 0.95f;

        var resultSectionSize = new Vector2(sectionWidth * DPI.UIScale(), windowSize.Y * 0.6f * DPI.UIScale());
        var exportSectionSize = new Vector2(sectionWidth * DPI.UIScale(), windowSize.Y * 0.2f * DPI.UIScale());

        if (ImGui.BeginTable($"globalSearchTable", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            // Row 1
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Search Filter");

            ImGui.TableSetColumnIndex(1);

            DPI.ApplyInputWidth();
            ImGui.InputText("##globalSearchInput", ref _globalSearchInput, 255);

            // Row 2
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Filter Type");

            ImGui.TableSetColumnIndex(1);

            DPI.ApplyInputWidth();
            if (ImGui.BeginCombo("##searchFilterType", FilterType.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(SearchFilterType)))
                {
                    var filterEntry = (SearchFilterType)entry;

                    if (ImGui.Selectable(filterEntry.GetDisplayName()))
                    {
                        FilterType = filterEntry;
                    }
                }

                ImGui.EndCombo();
            }
            UIHelper.Tooltip("The search filter to use.");

            // Row 3
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Match Type");

            ImGui.TableSetColumnIndex(1);

            DPI.ApplyInputWidth();
            if (ImGui.BeginCombo("##searchMatchType", MatchType.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(SearchMatchType)))
                {
                    var matchType = (SearchMatchType)entry;

                    if (ImGui.Selectable(matchType.GetDisplayName()))
                    {
                        MatchType = matchType;
                    }
                }

                ImGui.EndCombo();
            }
            UIHelper.Tooltip("The contents to match with.");

            // Row 4
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Ignore Case");

            ImGui.TableSetColumnIndex(1);

            ImGui.Checkbox("##ignoreCase", ref IgnoreCase);
            UIHelper.Tooltip("Ignore case sensitivity if enabled.");

            ImGui.EndTable();
        }

        if (ImGui.Button("Search##executeSearch", DPI.HalfWidthButton(sectionWidth, 24)))
        {
            HasSearched = true;
            SearchResults = TextFinder.GetGlobalTextResult(editor, _globalSearchInput, FilterType, MatchType, IgnoreCase);
        }
        ImGui.SameLine();
        if (ImGui.Button("Clear##clearSearchResults", DPI.HalfWidthButton(sectionWidth, 24)))
        {
            HasSearched = false;
            SearchResults.Clear();
        }

        ImGui.Separator();

        ImGui.BeginChild("resultsSection", resultSectionSize, ImGuiChildFlags.Borders);

        if (SearchResults.Count > 0)
        {
            foreach (var result in SearchResults)
            {
                // Ignore results from unused containers if in Simple mode
                if (CFG.Current.TextEditor_SimpleFileList)
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
                if (CFG.Current.TextEditor_DisplayCommunityContainerName)
                {
                    containerName = result.ContainerWrapper.GetContainerDisplayName();
                }

                // FMG
                var fmgName = result.FmgName;
                if (CFG.Current.TextEditor_DisplayFmgPrettyName)
                {
                    fmgName = TextUtils.GetFmgDisplayName(editor.Project, result.ContainerWrapper, result.FmgID, result.FmgName);
                }

                var displayText = $"{containerName} - {fmgName} - {result.Entry.ID}: {foundText}";

                if (ImGui.Selectable(displayText))
                {
                    EditorCommandQueue.AddCommand($"text/select/{category}/{result.ContainerName}/{result.FmgName}/{result.Entry.ID}");
                }
            }
        }
        else if(HasSearched)
        {
            UIHelper.WrappedText("No text entries found matching the filter.");
        }

        ImGui.EndChild();


        ImGui.BeginChild("exportSection", exportSectionSize, ImGuiChildFlags.Borders);

        if (SearchResults.Count > 0)
        {
            if (ImGui.Button("Copy to Clipboard##copyToClipboardAction", DPI.HalfWidthButton(sectionWidth, 24)))
            {
                var list = new TextExportList();
                list.Entries = new();

                foreach (var result in SearchResults)
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

        ImGui.EndChild();
    }
}
