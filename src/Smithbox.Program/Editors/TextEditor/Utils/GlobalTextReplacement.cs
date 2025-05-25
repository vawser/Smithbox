using Hexa.NET.ImGui;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Utils;

public static class GlobalTextReplacement
{
    private static string _globalSearchInput = "";
    private static string _globalReplaceInput = "";
    private static SearchFilterType FilterType = SearchFilterType.PrimaryCategory;
    private static SearchMatchType MatchType = SearchMatchType.All;

    private static List<ReplacementResult> ReplacementResults = new();

    private static bool IgnoreCase = false;
    private static bool MultilineRegex = false;
    private static bool SinglelineRegex = false;
    private static bool IgnorePatternWhitespace = false;

    private static bool HasSearched = false;
    public static void Display(TextEditorScreen editor)
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        var buttonSize = new Vector2(windowWidth / 2, 32 * DPI.GetUIScale());

        if (ImGui.BeginTable($"globalReplacementTable", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            // Row 1
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Conditional Input");
            UIHelper.Tooltip("The regex you want to match with.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            ImGui.InputText("##globalSearchInput", ref _globalSearchInput, 255);

            // Row 2
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Replacement Input");
            UIHelper.Tooltip("The regex you want to replace with.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            ImGui.InputText("##globalReplaceInput", ref _globalReplaceInput, 255);

            // Row 3
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

            // Row 4
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

            // Row 5
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Ignore Case");

            ImGui.TableSetColumnIndex(1);

            ImGui.Checkbox("##ignoreCase", ref IgnoreCase);
            UIHelper.Tooltip("Specifies case-insensitive matching for regex.");

            // Row 6
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Multi-line Regex");

            ImGui.TableSetColumnIndex(1);

            ImGui.Checkbox("##multilineRegex", ref MultilineRegex);
            UIHelper.Tooltip("Multiline mode for regex. Changes the meaning of ^ and $ so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.");

            // Row 7
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Single-line Regex");

            ImGui.TableSetColumnIndex(1);

            ImGui.Checkbox("##singleLineRegex", ref SinglelineRegex);
            UIHelper.Tooltip("Specifies single-line mode for regex. Changes the meaning of the dot (.) so it matches every character (instead of every character except \\n).");

            // Row 8
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Ignore Pattern Whitespace");

            ImGui.TableSetColumnIndex(1);

            ImGui.Checkbox("##ignorePatternWhitespace", ref IgnorePatternWhitespace);
            UIHelper.Tooltip("Eliminates unescaped white space from the pattern and enables comments marked with #. However, this value does not affect or eliminate white space in character classes, numeric quantifiers, or tokens that mark the beginning of individual regular expression language elements.");

            ImGui.EndTable();
        }

        if (ImGui.Button("Preview Edit##executeSearch", buttonSize))
        {
            HasSearched = true;
            ReplacementResults = TextFinder.GetReplacementResult(editor, _globalSearchInput, FilterType, MatchType, IgnoreCase);
        }
        UIHelper.Tooltip("Populate the edit preview list.");
        ImGui.SameLine();
        if (ImGui.Button("Clear Preview##clearSearchResults", buttonSize))
        {
            HasSearched = false;
            ReplacementResults.Clear();
        }
        UIHelper.Tooltip("Clear the edit preview list.");

        ImGui.Separator();

        if (ReplacementResults.Count > 0)
        {
            RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;

            if (IgnoreCase)
            {
                options = options | RegexOptions.IgnoreCase;
            }
            if (MultilineRegex)
            {
                options = options | RegexOptions.Multiline;
            }
            if (SinglelineRegex)
            {
                options = options | RegexOptions.Singleline;
            }
            if (IgnorePatternWhitespace)
            {
                options = options | RegexOptions.IgnorePatternWhitespace;
            }

            if (ImGui.Button("Apply Replacement", defaultButtonSize))
            {
                List<EditorAction> actions = new List<EditorAction>();

                string searchText = _globalSearchInput;
                string replaceText = _globalReplaceInput;

                foreach (var result in ReplacementResults)
                {
                    var newText = Regex.Replace(result.Entry.Text, searchText, replaceText, options);
                    actions.Add(new ChangeFmgEntryText(editor, result.ContainerWrapper, result.Entry, newText));
                }

                var groupedAction = new FmgGroupedAction(actions);
                editor.EditorActionManager.ExecuteAction(groupedAction);
            }
            UIHelper.Tooltip("All the entries listed in the list below will have the Replacement Input regex applied to them.");

            UIHelper.WrappedText("Entries that will be affected:");

            foreach (var result in ReplacementResults)
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
        else if (HasSearched)
        {
            UIHelper.WrappedText("No text entries found matching the filter.");
        }
    }
}