using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.TextEditor.FMGBank;

namespace StudioCore.Editors.TextEditor.Toolbar
{
    public class TextAction_BlockDuplicate
    {
        public static void Select()
        {
            if (ImGui.Selectable("Block Duplicate", TextEditorToolbar.SelectedAction == TextEditorAction.BlockDuplicate, ImGuiSelectableFlags.AllowDoubleClick))
            {
                TextEditorToolbar.SelectedAction = TextEditorAction.BlockDuplicate;

                if (ImGui.IsMouseDoubleClicked(0))
                {
                    if (CFG.Current.FMG_Toolbar_Prompt_User_Action)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Block Duplicate action. This action cannot be undone. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

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
            ImguiUtils.ShowHoverTooltip("Duplicate blocks of entries in a patterned fashion.");
        }
        public static void Configure()
        {
            if (TextEditorToolbar.SelectedAction == TextEditorAction.BlockDuplicate)
            {
                ImGui.InputInt("Block Start ID", ref CFG.Current.FMG_BlockDuplicate_StartID);
                ImguiUtils.ShowHoverTooltip("The first entry ID of your block.");

                ImGui.InputInt("Block End ID", ref CFG.Current.FMG_BlockDuplicate_EndID);
                ImguiUtils.ShowHoverTooltip("The last entry ID of your block.");

                ImGui.InputInt("Block Rebase ID", ref CFG.Current.FMG_BlockDuplicate_RebaseID);
                ImguiUtils.ShowHoverTooltip("The new ID for your first entry that the entire block will be rebased around.");
            }
        }
        public static void Act()
        {
            var CurrentFmgInfo = TextEditorScreen._activeFmgInfo;

            foreach (var fmgEntry in CurrentFmgInfo.Fmg.Entries)
            {
                EntryGroup entryGroup = StudioCore.TextEditor.FMGBank.GenerateEntryGroup(fmgEntry.ID, CurrentFmgInfo);

                // TODO
            }
        }
    }
}
