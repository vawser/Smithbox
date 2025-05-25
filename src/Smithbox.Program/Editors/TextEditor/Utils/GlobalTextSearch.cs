using Hexa.NET.ImGui;
using Org.BouncyCastle.Crypto;
using Silk.NET.SDL;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor.Enums;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Utils;

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
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        var buttonSize = new Vector2(windowWidth / 2, 32 * DPI.GetUIScale());

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

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            ImGui.InputText("##globalSearchInput", ref _globalSearchInput, 255);

            // Row 2
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Filter Type");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
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

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
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

        if (ImGui.Button("Search##executeSearch", buttonSize))
        {
            HasSearched = true;
            SearchResults = TextFinder.GetGlobalTextResult(editor, _globalSearchInput, FilterType, MatchType, IgnoreCase);
        }
        ImGui.SameLine();
        if (ImGui.Button("Clear##clearSearchResults", buttonSize))
        {
            HasSearched = false;
            SearchResults.Clear();
        }

        ImGui.Separator();

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
    }
}
