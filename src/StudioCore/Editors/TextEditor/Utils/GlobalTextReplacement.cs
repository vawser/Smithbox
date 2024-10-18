using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
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

    private static List<ReplacementResult> ReplacementResults = new();

    private static bool IgnoreCase = false;
    private static bool MultilineRegex = false;
    private static bool SinglelineRegex = false;
    private static bool IgnorePatternWhitespace = false;

    public static void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        if (ImGui.BeginTable($"globalReplacementTable", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            // Row 1
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Conditional Input");
            UIHelper.ShowHoverTooltip("The regex you want to match with.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            ImGui.InputText("##globalSearchInput", ref _globalSearchInput, 255);

            // Row 2
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Replacement Input");
            UIHelper.ShowHoverTooltip("The regex you want to replace with.");

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
            UIHelper.ShowHoverTooltip("The search filter to use.");

            // Row 3
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Ignore Case");

            ImGui.TableSetColumnIndex(1);

            ImGui.Checkbox("##ignoreCase", ref IgnoreCase);
            UIHelper.ShowHoverTooltip("Specifies case-insensitive matching for regex.");

            // Row 4
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Multi-line Regex");

            ImGui.TableSetColumnIndex(1);

            ImGui.Checkbox("##multilineRegex", ref MultilineRegex);
            UIHelper.ShowHoverTooltip("Multiline mode for regex. Changes the meaning of ^ and $ so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.");

            // Row 5
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Single-line Regex");

            ImGui.TableSetColumnIndex(1);

            ImGui.Checkbox("##singleLineRegex", ref SinglelineRegex);
            UIHelper.ShowHoverTooltip("Specifies single-line mode for regex. Changes the meaning of the dot (.) so it matches every character (instead of every character except \\n).");

            // Row 6
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Ignore Pattern Whitespace");

            ImGui.TableSetColumnIndex(1);

            ImGui.Checkbox("##ignorePatternWhitespace", ref IgnorePatternWhitespace);
            UIHelper.ShowHoverTooltip("Eliminates unescaped white space from the pattern and enables comments marked with #. However, this value does not affect or eliminate white space in character classes, numeric quantifiers, or tokens that mark the beginning of individual regular expression language elements.");

            ImGui.EndTable();
        }

        if (ImGui.Button("Preview Edit##executeSearch", UI.GetStandardHalfButtonSize()))
        {
            ReplacementResults = TextFinder.GetReplacementResult(_globalSearchInput, FilterType, IgnoreCase);
        }
        UIHelper.ShowHoverTooltip("Populate the edit preview list.");
        ImGui.SameLine();
        if (ImGui.Button("Clear Preview##clearSearchResults", UI.GetStandardHalfButtonSize()))
        {
            ReplacementResults.Clear();
        }
        UIHelper.ShowHoverTooltip("Clear the edit preview list.");

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
                    actions.Add(new ChangeFmgEntryText(result.Info, result.Entry, newText));
                }

                var groupedAction = new FmgGroupedAction(actions);
                Smithbox.EditorHandler.TextEditor.EditorActionManager.ExecuteAction(groupedAction);
            }
            UIHelper.ShowHoverTooltip("All the entries listed in the list below will have the Replacement Input regex applied to them.");

            UIHelper.WrappedText("Entries that will be affected:");

            foreach (var result in ReplacementResults)
            {
                var foundText = result.Entry.Text;
                if (foundText.Contains("\n"))
                {
                    var firstSection = foundText.Split("\n")[0];
                    foundText = $"{firstSection} <...>";
                }

                var category = result.Info.Category.ToString();

                // Container
                var containerName = result.ContainerName;
                if (CFG.Current.TextEditor_DisplayPrettyContainerName)
                {
                    containerName = TextUtils.GetPrettyContainerName(result.ContainerName);
                }

                // FMG
                var fmgName = result.FmgName;
                if (CFG.Current.TextEditor_DisplayFmgPrettyName)
                {
                    fmgName = TextUtils.GetFmgDisplayName(result.Info, result.FmgID, result.FmgName);
                }

                var displayText = $"{containerName} - {fmgName} - {result.Entry.ID}: {foundText}";

                if (ImGui.Selectable(displayText))
                {
                    EditorCommandQueue.AddCommand($"text/select/{category}/{result.ContainerName}/{result.FmgName}/{result.Entry.ID}");
                }
            }
        }
    }
}