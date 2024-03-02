using ImGuiNET;
using SoulsFormats;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static StudioCore.TextEditor.FMGBank;

namespace StudioCore.Editors.TextEditor.Toolbar
{
    public class TextAction_SearchAndReplace
    {
        private static string CurrentTargetType;
        private static string CurrentTextCategory;

        public static void Setup()
        {
            CurrentTargetType = "Selected Entry";
            CurrentTextCategory = "Description";
        }

        public static void Select()
        {
            if (ImGui.Selectable("Search and Replace", TextEditorToolbar.SelectedAction == TextEditorAction.SearchAndReplace, ImGuiSelectableFlags.AllowDoubleClick))
            {
                TextEditorToolbar.SelectedAction = TextEditorAction.SearchAndReplace;

                if (ImGui.IsMouseDoubleClicked(0))
                {
                    if (CFG.Current.FMG_Toolbar_Prompt_User_Action)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Search and Replace action. This action cannot be undone. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            Act();
                        }
                    }
                    else
                    {
                        Act();
                    }
                }
            }
            ImguiUtils.ShowHoverTooltip("Search and replace text.");
        }

        public static void Configure()
        {
            if (TextEditorToolbar.SelectedAction == TextEditorAction.SearchAndReplace)
            {
                ImGui.InputText("Search##searchText", ref CFG.Current.FMG_SearchAndReplace_SearchText, 255);
                ImguiUtils.ShowHoverTooltip("Text to search for. Supports regular expressions.");

                ImGui.InputText("Replace##replaceText", ref CFG.Current.FMG_SearchAndReplace_ReplaceText, 255);
                ImguiUtils.ShowHoverTooltip("Text to replace the search text with. Supports regular expressions.");

                if (ImGui.BeginCombo("Text Category", CurrentTextCategory))
                {
                    foreach (string e in TextEditorToolbar.TextCategories)
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

                if (ImGui.BeginCombo("Target Type", CurrentTargetType))
                {
                    foreach (string e in TextEditorToolbar.TargetTypes)
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

                ImGui.Checkbox("Ignore Case", ref CFG.Current.FMG_SearchAndReplace_Regex_IgnoreCase);
                ImguiUtils.ShowHoverTooltip("Specifies case-insensitive matching for regex.");

                ImGui.Checkbox("Multi-line", ref CFG.Current.FMG_SearchAndReplace_Regex_Multiline);
                ImguiUtils.ShowHoverTooltip("Multiline mode for regex. Changes the meaning of ^ and $ so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.");

                ImGui.Checkbox("Single-line", ref CFG.Current.FMG_SearchAndReplace_Regex_Singleline);
                ImguiUtils.ShowHoverTooltip("Specifies single-line mode for regex. Changes the meaning of the dot (.) so it matches every character (instead of every character except \\n).");

                ImGui.Checkbox("Ignore Pattern Whitespace", ref CFG.Current.FMG_SearchAndReplace_Regex_IgnorePatternWhitespace);
                ImguiUtils.ShowHoverTooltip("Eliminates unescaped white space from the pattern and enables comments marked with #. However, this value does not affect or eliminate white space in character classes, numeric quantifiers, or tokens that mark the beginning of individual regular expression language elements.");
            }
        }

        private static void Act()
        {
            var CurrentFmgInfo = TextEditorScreen._activeFmgInfo;
            var CurrentEntryGroup = TextEditorScreen._activeEntryGroup;

            if (CurrentTargetType == "Selected Entry")
            {
                StartSearchAndReplace(CurrentEntryGroup);
            }

            if (CurrentTargetType == "Selected Category")
            {
                foreach (var fmgEntry in CurrentFmgInfo.Fmg.Entries)
                {
                    EntryGroup entryGroup = StudioCore.TextEditor.FMGBank.GenerateEntryGroup(fmgEntry.ID, CurrentFmgInfo);

                    StartSearchAndReplace(entryGroup);
                }
            }
        }

        private static void StartSearchAndReplace(EntryGroup entry)
        {
            if(entry == null)
            {
                return;
            }

            // Title
            if(CurrentTextCategory is "Title" or "All")
            {
                PerformSearchAndReplace(entry.Title);
            }

            // TextBody
            if (CurrentTextCategory is "Text Body" or "All")
            {
                PerformSearchAndReplace(entry.TextBody);
            }

            // Summary
            if (CurrentTextCategory is "Summary" or "All")
            {
                PerformSearchAndReplace(entry.Summary);
            }

            // ExtraText
            if (CurrentTextCategory is "Extra Text" or "All")
            {
                PerformSearchAndReplace(entry.ExtraText);
            }

            // Description
            if (CurrentTextCategory is "Description" or "All")
            {
                PerformSearchAndReplace(entry.Description);
            }
        }

        private static void PerformSearchAndReplace(FMG.Entry entry)
        {
            if(entry == null)
            {
                return;
            }

            if (entry.Text == null)
            {
                return;
            }

            string searchText = CFG.Current.FMG_SearchAndReplace_SearchText;
            string replaceText = CFG.Current.FMG_SearchAndReplace_ReplaceText;

            RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;
            
            if(CFG.Current.FMG_SearchAndReplace_Regex_IgnoreCase)
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

            entry.Text = result;
        }
    }
}
