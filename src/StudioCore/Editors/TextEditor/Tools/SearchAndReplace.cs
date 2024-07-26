using DotNext.Resources;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Tools;

public static class SearchAndReplace
{
    private static string CurrentTargetType = "Selected Entries";
    private static string CurrentTextCategory = "Description";

    public static List<string> TargetTypes = new List<string>
        {
            "Selected Category",
            "Selected Entries"
        };

    public static List<string> TextCategories = new List<string>
        {
            "Title",
            "Description",
            "Summary",
            "Text Body",
            "Extra Text",
            "All"
        };

    public static void DisplayConfiguration(Vector2 defaultButtonSize)
    {
        ImguiUtils.WrappedText("Perform a search and replace throughout the Text Entries.");
        ImguiUtils.WrappedText("");

        ImguiUtils.WrappedText("Search Text:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputText("##searchText", ref CFG.Current.FMG_SearchAndReplace_SearchText, 255);
        ImguiUtils.ShowHoverTooltip("Text to search for. Supports regular expressions.");
        ImguiUtils.WrappedText("");

        ImguiUtils.WrappedText("Replace Text:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputText("##replaceText", ref CFG.Current.FMG_SearchAndReplace_ReplaceText, 255);
        ImguiUtils.ShowHoverTooltip("Text to replace the search text with. Supports regular expressions.");
        ImguiUtils.WrappedText("");

        ImguiUtils.WrappedText("Text Category:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        if (ImGui.BeginCombo("##Text Category", CurrentTextCategory))
        {
            foreach (string e in TextCategories)
            {
                if (ImGui.Selectable(e))
                {
                    CurrentTextCategory = e;
                    break;
                }
            }
            ImGui.EndCombo();
        }
        ImguiUtils.ShowHoverTooltip("Text category to search in.");
        ImguiUtils.WrappedText("");

        ImguiUtils.WrappedText("Text Context:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        if (ImGui.BeginCombo("##Target Type", CurrentTargetType))
        {
            foreach (string e in TargetTypes)
            {
                if (ImGui.Selectable(e.ToString()))
                {
                    CurrentTargetType = e;
                    break;
                }
            }
            ImGui.EndCombo();
        }
        ImguiUtils.ShowHoverTooltip("The target text context for the search and replace.");
        ImguiUtils.WrappedText("");

        ImGui.Checkbox("Ignore Case", ref CFG.Current.FMG_SearchAndReplace_Regex_IgnoreCase);
        ImguiUtils.ShowHoverTooltip("Specifies case-insensitive matching for regex.");

        ImGui.Checkbox("Multi-line", ref CFG.Current.FMG_SearchAndReplace_Regex_Multiline);
        ImguiUtils.ShowHoverTooltip("Multiline mode for regex. Changes the meaning of ^ and $ so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.");

        ImGui.Checkbox("Single-line", ref CFG.Current.FMG_SearchAndReplace_Regex_Singleline);
        ImguiUtils.ShowHoverTooltip("Specifies single-line mode for regex. Changes the meaning of the dot (.) so it matches every character (instead of every character except \\n).");

        ImGui.Checkbox("Ignore Pattern Whitespace", ref CFG.Current.FMG_SearchAndReplace_Regex_IgnorePatternWhitespace);
        ImguiUtils.ShowHoverTooltip("Eliminates unescaped white space from the pattern and enables comments marked with #. However, this value does not affect or eliminate white space in character classes, numeric quantifiers, or tokens that mark the beginning of individual regular expression language elements.");
        ImguiUtils.WrappedText("");

        if (ImGui.Button("Apply##action_Selection_SearchAndReplace", defaultButtonSize))
        {
            ConductSearchAndReplace();
        }
    }

    public static void ConductSearchAndReplace()
    {
        var entryIds = Smithbox.EditorHandler.TextEditor.SelectionHandler.EntryIds;
        var entries = Smithbox.EditorHandler.TextEditor._EntryLabelCacheFiltered;
        var fmgInfo = Smithbox.EditorHandler.TextEditor._activeFmgInfo;

        List<EditorAction> actions = new List<EditorAction>();

        if (CurrentTargetType == "Selected Entries")
        {
            for (var i = 0; i < entries.Count; i++)
            {
                FMG.Entry r = entries[i];

                if (entryIds.Contains(r.ID))
                {
                    var entryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, fmgInfo);
                    var action = ReplaceText(entryGroup);
                    if (action != null)
                    {
                        actions.Add(action);
                    }
                }
            }
        }

        if (CurrentTargetType == "Selected Category")
        {
            foreach (var fmgEntry in fmgInfo.Fmg.Entries)
            {
                FMGEntryGroup entryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(fmgEntry.ID, fmgInfo);

                var action = ReplaceText(entryGroup);
                if (action != null)
                {
                    actions.Add(action);
                }
            }
        }

        var compoundAction = new CompoundAction(actions);
        Smithbox.EditorHandler.TextEditor.EditorActionManager.ExecuteAction(compoundAction);
    }

    private static ReplaceFMGEntryTextAction ReplaceText(FMGEntryGroup entry)
    {
        if (entry == null)
        {
            return null;
        }

        ReplaceFMGEntryTextAction action = null;

        // Title
        if (CurrentTextCategory is "Title" or "All")
        {
            action = PerformSearchAndReplace(entry.Title);
        }

        // TextBody
        if (CurrentTextCategory is "Text Body" or "All")
        {
            action = PerformSearchAndReplace(entry.TextBody);
        }

        // Summary
        if (CurrentTextCategory is "Summary" or "All")
        {
            action = PerformSearchAndReplace(entry.Summary);
        }

        // ExtraText
        if (CurrentTextCategory is "Extra Text" or "All")
        {
            action = PerformSearchAndReplace(entry.ExtraText);
        }

        // Description
        if (CurrentTextCategory is "Description" or "All")
        {
            action = PerformSearchAndReplace(entry.Description);
        }

        return action;
    }

    private static ReplaceFMGEntryTextAction PerformSearchAndReplace(FMG.Entry entry)
    {
        if (entry == null)
        {
            return null;
        }

        if (entry.Text == null)
        {
            return null;
        }

        string searchText = CFG.Current.FMG_SearchAndReplace_SearchText;
        string replaceText = CFG.Current.FMG_SearchAndReplace_ReplaceText;

        RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;

        if (CFG.Current.FMG_SearchAndReplace_Regex_IgnoreCase)
        {
            options = options | RegexOptions.IgnoreCase;
        }
        if (CFG.Current.FMG_SearchAndReplace_Regex_Multiline)
        {
            options = options | RegexOptions.Multiline;
        }
        if (CFG.Current.FMG_SearchAndReplace_Regex_Singleline)
        {
            options = options | RegexOptions.Singleline;
        }
        if (CFG.Current.FMG_SearchAndReplace_Regex_IgnorePatternWhitespace)
        {
            options = options | RegexOptions.IgnorePatternWhitespace;
        }

        var result = Regex.Replace(entry.Text, searchText, replaceText, options);

        var action = new ReplaceFMGEntryTextAction(entry, result);
        return action;
    }
}
