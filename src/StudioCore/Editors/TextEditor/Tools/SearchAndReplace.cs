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
using static SoulsFormats.FFXDLSE;

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
        UIHelper.WrappedText("Perform a search and replace throughout the Text Entries.");
        UIHelper.WrappedText("");

        UIHelper.WrappedText("Search Text:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputText("##searchText", ref CFG.Current.FMG_SearchAndReplace_SearchText, 255);
        UIHelper.ShowHoverTooltip("Text to search for. Supports regular expressions.");
        UIHelper.WrappedText("");

        UIHelper.WrappedText("Replace Text:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputText("##replaceText", ref CFG.Current.FMG_SearchAndReplace_ReplaceText, 255);
        UIHelper.ShowHoverTooltip("Text to replace the search text with. Supports regular expressions.");
        UIHelper.WrappedText("");

        UIHelper.WrappedText("Text Category:");
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
        UIHelper.ShowHoverTooltip("Text category to search in.");
        UIHelper.WrappedText("");

        UIHelper.WrappedText("Text Context:");
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
        UIHelper.ShowHoverTooltip("The target text context for the search and replace.");
        UIHelper.WrappedText("");

        ImGui.Checkbox("Ignore Case", ref CFG.Current.FMG_SearchAndReplace_Regex_IgnoreCase);
        UIHelper.ShowHoverTooltip("Specifies case-insensitive matching for regex.");

        ImGui.Checkbox("Multi-line", ref CFG.Current.FMG_SearchAndReplace_Regex_Multiline);
        UIHelper.ShowHoverTooltip("Multiline mode for regex. Changes the meaning of ^ and $ so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.");

        ImGui.Checkbox("Single-line", ref CFG.Current.FMG_SearchAndReplace_Regex_Singleline);
        UIHelper.ShowHoverTooltip("Specifies single-line mode for regex. Changes the meaning of the dot (.) so it matches every character (instead of every character except \\n).");

        ImGui.Checkbox("Ignore Pattern Whitespace", ref CFG.Current.FMG_SearchAndReplace_Regex_IgnorePatternWhitespace);
        UIHelper.ShowHoverTooltip("Eliminates unescaped white space from the pattern and enables comments marked with #. However, this value does not affect or eliminate white space in character classes, numeric quantifiers, or tokens that mark the beginning of individual regular expression language elements.");
        UIHelper.WrappedText("");

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
                    var actionList = ReplaceText(entryGroup);
                    if (actionList != null)
                    {
                        foreach (var entry in actionList)
                        {
                            actions.Add(entry);
                        }
                    }
                }
            }
        }

        if (CurrentTargetType == "Selected Category")
        {
            foreach (var fmgEntry in fmgInfo.Fmg.Entries)
            {
                FMGEntryGroup entryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(fmgEntry.ID, fmgInfo);

                var actionList = ReplaceText(entryGroup);
                if (actionList != null)
                {
                    foreach (var entry in actionList)
                    {
                        actions.Add(entry);
                    }
                }
            }
        }

        var compoundAction = new CompoundAction(actions);
        Smithbox.EditorHandler.TextEditor.EditorActionManager.ExecuteAction(compoundAction);
    }

    private static List<ReplaceFMGEntryTextAction> ReplaceText(FMGEntryGroup entry)
    {
        if (entry == null)
        {
            return null;
        }

        List<ReplaceFMGEntryTextAction> actions = new List<ReplaceFMGEntryTextAction>();

        FMG.Entry tempEntry = null;

        // Title
        if (CurrentTextCategory is "Title" or "All")
        {
            tempEntry = entry.Title;
        }

        // TextBody
        if (CurrentTextCategory is "Text Body" or "All")
        {
            tempEntry = entry.TextBody;
        }

        // Summary
        if (CurrentTextCategory is "Summary" or "All")
        {
            tempEntry = entry.Summary;
        }

        // ExtraText
        if (CurrentTextCategory is "Extra Text" or "All")
        {
            tempEntry = entry.ExtraText;
        }

        // Description
        if (CurrentTextCategory is "Description" or "All")
        {
            tempEntry = entry.Description;
        }

        if (tempEntry != null)
        {
            var action = PerformSearchAndReplace(tempEntry);
            if(action != null)
            {
                actions.Add(action);
            }
        }

        return actions;
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
