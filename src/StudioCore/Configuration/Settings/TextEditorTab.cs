using ImGuiNET;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class TextEditorTab
{
    public TextEditorTab() { }

    public void Display()
    {
        // Data
        if (ImGui.CollapsingHeader("Data", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Include Non-Primary Containers", ref CFG.Current.TextEditor_IncludeNonPrimaryContainers);
            UIHelper.ShowHoverTooltip("If enabled, non-primary FMG containers are loaded.");

            ImGui.Checkbox("Include Vanilla Cache", ref CFG.Current.TextEditor_IncludeVanillaCache);
            UIHelper.ShowHoverTooltip("If enabled, the vanilla cache is loaded, which enables the modified and unique difference features.");
        }

        // Primary Category
        if (ImGui.CollapsingHeader("Primary Category", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.BeginCombo("Primary Category##primaryCategoryCombo", CFG.Current.TextEditor_PrimaryCategory.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(TextContainerCategory)))
                {
                    var type = (TextContainerCategory)entry;

                    if (TextUtils.IsSupportedLanguage((TextContainerCategory)entry))
                    {
                        if (ImGui.Selectable(type.GetDisplayName()))
                        {
                            CFG.Current.TextEditor_PrimaryCategory = (TextContainerCategory)entry;

                            // Refresh the param editor FMG decorators when the category changes.
                            Smithbox.EditorHandler.ParamEditor.ClearFmgDecorators();
                        }
                    }
                }
                ImGui.EndCombo();
            }
            UIHelper.ShowHoverTooltip("Change the primary category, this determines which text files are used for FMG references and other stuff.");

            ImGui.Checkbox("Hide non-primary categories in list", ref CFG.Current.TextEditor_DisplayPrimaryCategoryOnly);
            UIHelper.ShowHoverTooltip("Hide the non-primary categories in the File List.");

        }

        // File List
        if (ImGui.CollapsingHeader("File List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display Community File Name", ref CFG.Current.TextEditor_DisplayPrettyContainerName);
            UIHelper.ShowHoverTooltip("If enabled, the names in the File List will be given a community name.");

            ImGui.Checkbox("Display Source Path", ref CFG.Current.TextEditor_DisplaySourcePath);
            UIHelper.ShowHoverTooltip("If enabled, the path of the source file will be displayed in the hover tooltip.");

        }

        // Text File List
        if (ImGui.CollapsingHeader("Text File List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display FMG ID", ref CFG.Current.TextEditor_DisplayFmgID);
            UIHelper.ShowHoverTooltip("Display the FMG ID in the Text File List by the name.");

            ImGui.Checkbox("Display Community FMG Name", ref CFG.Current.TextEditor_DisplayFmgPrettyName);
            UIHelper.ShowHoverTooltip("Display the FMG community name instead of the internal form.");
        }

        // Text Entries List
        if (ImGui.CollapsingHeader("Text Entries List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display Empty Text Placeholder", ref CFG.Current.TextEditor_DisplayNullPlaceholder);
            UIHelper.ShowHoverTooltip("Display placeholder text for rows that have no text.");

            ImGui.Checkbox("Display Empty Rows", ref CFG.Current.TextEditor_DisplayNullEntries);
            UIHelper.ShowHoverTooltip("Display FMG entries with empty text.");

            ImGui.Checkbox("Trucate Displayed Text", ref CFG.Current.TextEditor_TruncateTextDisplay);
            UIHelper.ShowHoverTooltip("Truncate the displayed text so it is always one line (does not affect the contents of the entry).");

            ImGui.Checkbox("Ignore ID on Duplication", ref CFG.Current.TextEditor_IgnoreIdOnDuplicate);
            UIHelper.ShowHoverTooltip("Keep the Entry ID the same on duplication. Useful if you want to manually edit the IDs afterwards.");
        }

        // Entry Properties
        if (ImGui.CollapsingHeader("Text Entry Properties", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display Grouped Entries", ref CFG.Current.TextEditor_Entry_DisplayGroupedEntries);
            UIHelper.ShowHoverTooltip("Include related entries in the Contents window, e.g. Title, Summary, Description, Effect entries that share the same ID.");
        }

        // Text Entry Copy
        if (ImGui.CollapsingHeader("Clipboard Action", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Include ID", ref CFG.Current.TextEditor_TextCopy_IncludeID);
            UIHelper.ShowHoverTooltip("Include the row ID when copying a Text Entry to the clipboard.");

            ImGui.Checkbox("Escape New Lines", ref CFG.Current.TextEditor_TextCopy_EscapeNewLines);
            UIHelper.ShowHoverTooltip("Escape the new lines characters when copying a Text Entry to the clipboard.");
        }
    }
}
