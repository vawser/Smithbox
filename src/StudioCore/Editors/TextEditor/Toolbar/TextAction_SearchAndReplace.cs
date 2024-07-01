using ImGuiNET;
using SoulsFormats;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
            if (ImGui.RadioButton("Search and Replace##tool_SearchAndReplace", TextToolbar.SelectedAction == TextEditorAction.SearchAndReplace))
            {
                TextToolbar.SelectedAction = TextEditorAction.SearchAndReplace;
            }
            ImguiUtils.ShowHoverTooltip("Search and replace text.");
        }

        public static void Configure()
        {
            if (TextToolbar.SelectedAction == TextEditorAction.SearchAndReplace)
            {
                ImguiUtils.WrappedText("Perform a search and replace upon FMG text.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Search Text:");
                ImGui.InputText("##searchText", ref CFG.Current.FMG_SearchAndReplace_SearchText, 255);
                ImguiUtils.ShowHoverTooltip("Text to search for. Supports regular expressions.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Replace Text:");
                ImGui.InputText("##replaceText", ref CFG.Current.FMG_SearchAndReplace_ReplaceText, 255);
                ImguiUtils.ShowHoverTooltip("Text to replace the search text with. Supports regular expressions.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Text Category:");
                if (ImGui.BeginCombo("##Text Category", CurrentTextCategory))
                {
                    foreach (string e in TextToolbar.TextCategories)
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
                if (ImGui.BeginCombo("##Target Type", CurrentTargetType))
                {
                    foreach (string e in TextToolbar.TargetTypes)
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
            }
        }

        public static void Act()
        {
            if (TextToolbar.SelectedAction == TextEditorAction.SearchAndReplace)
            {
                if (ImGui.Button("Apply##action_Selection_SearchAndReplace", new Vector2(200, 32)))
                {
                    if (CFG.Current.Interface_ParamEditor_PromptUser)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Search and Replace action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            ConductSearchAndReplace();
                        }
                    }
                    else
                    {
                        ConductSearchAndReplace();
                    }
                }
            }
        }

        private static void ConductSearchAndReplace()
        {
            var CurrentFmgInfo = Smithbox.EditorHandler.TextEditor._activeFmgInfo;
            var CurrentEntryGroup = Smithbox.EditorHandler.TextEditor._activeEntryGroup;

            if (CurrentTargetType == "Selected Entry")
            {
                StartSearchAndReplace(CurrentEntryGroup);
            }

            if (CurrentTargetType == "Selected Category")
            {
                foreach (var fmgEntry in CurrentFmgInfo.Fmg.Entries)
                {
                    FMGEntryGroup entryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(fmgEntry.ID, CurrentFmgInfo);

                    StartSearchAndReplace(entryGroup);
                }
            }
        }

        private static void StartSearchAndReplace(FMGEntryGroup entry)
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
