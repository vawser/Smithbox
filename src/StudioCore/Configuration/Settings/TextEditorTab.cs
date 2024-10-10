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
        if (ImGui.CollapsingHeader("Primary Category", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.BeginCombo("Primary Category##primaryCategoryCombo", CFG.Current.TextEditor_PrimaryCategory.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(TextContainerCategory)))
                {
                    var type = (TextContainerCategory)entry;

                    if (ImGui.Selectable(type.GetDisplayName()))
                    {
                        CFG.Current.TextEditor_PrimaryCategory = (TextContainerCategory)entry;
                    }
                }
                ImGui.EndCombo();
            }
            UIHelper.ShowHoverTooltip("Change the primary category, this determines which text files are used for FMG references and other stuff.");
        }

        if (ImGui.CollapsingHeader("File List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Hide non-Primary Languages in List", ref CFG.Current.TextEditor_DisplayPrimaryLanguageOnly);
            UIHelper.ShowHoverTooltip("Hide the non-Primary Language groups in the File List.");

            ImGui.Checkbox("Display Community File Name", ref CFG.Current.TextEditor_DisplayPrettyContainerName);
            UIHelper.ShowHoverTooltip("If enabled, the names in the File List will be given a community name.");

            ImGui.Checkbox("Display Source Path", ref CFG.Current.TextEditor_DisplaySourcePath);
            UIHelper.ShowHoverTooltip("If enabled, the path of the source file will be displayed in the hover tooltip.");

        }

        if (ImGui.CollapsingHeader("Text File List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display FMG ID", ref CFG.Current.TextEditor_DisplayFmgID);
            UIHelper.ShowHoverTooltip("Display the FMG ID in the Text File List by the name.");

            ImGui.Checkbox("Display Community FMG Name", ref CFG.Current.TextEditor_DisplayFmgPrettyName);
            UIHelper.ShowHoverTooltip("Display the FMG community name instead of the internal form.");
        }

        if (ImGui.CollapsingHeader("Text Entries List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display Empty Rows", ref CFG.Current.TextEditor_DisplayNullEntries);
            UIHelper.ShowHoverTooltip("Display FMG entries with empty text.");
        }
    }
}
