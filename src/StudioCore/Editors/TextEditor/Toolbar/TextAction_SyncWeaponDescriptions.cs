using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Toolbar
{
    public static class TextAction_SyncWeaponDescriptions
    {
        public static void Select()
        {
            if (ImGui.Selectable("Sync Weapon Descriptions", TextEditorToolbar.SelectedAction == TextEditorAction.SyncWeaponDescription, ImGuiSelectableFlags.AllowDoubleClick))
            {
                TextEditorToolbar.SelectedAction = TextEditorAction.SyncWeaponDescription;

                if (ImGui.IsMouseDoubleClicked(0))
                {
                    if (CFG.Current.FMG_Toolbar_Prompt_User_Action)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Sync Weapon Descriptions action. This action cannot be undone. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if(result == DialogResult.Yes)
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
            ImguiUtils.ShowHoverTooltip("Changes all 'infusion' or 'affinity' entries so their descriptions match the base entry for their weapon.");
        }

        public static void Configure()
        {
            if (TextEditorToolbar.SelectedAction == TextEditorAction.SyncWeaponDescription)
            {
                ImGui.InputInt("Sync Start ID", ref CFG.Current.FMG_SyncWeaponEntries_StartID);
                ImguiUtils.ShowHoverTooltip("Start FMG ID from which the sync will take affect.");

                ImGui.InputInt("Sync End ID", ref CFG.Current.FMG_SyncWeaponEntries_EndID);
                ImguiUtils.ShowHoverTooltip("End FMG ID from which the sync will stop taking affect.");

                ImGui.InputInt("Modulus", ref CFG.Current.FMG_SyncWeaponEntries_Modulus);
                ImguiUtils.ShowHoverTooltip("The modulus used to detect which FMG entries are considered 'base' entries.");
            }
        }

        private static void Act()
        {
            bool holdingSyncDescription = false;
            string syncDescription = "";
            int syncBaseId = 0;

            foreach (var entry in StudioCore.TextEditor.FMGBank.FmgInfoBank)
            {
                if (entry.EntryCategory == FmgEntryCategory.Weapons)
                {
                    if (entry.EntryType == FmgEntryTextType.Description)
                    {
                        foreach (var fmg in entry.Fmg.Entries)
                        {
                            if (fmg.ID >= CFG.Current.FMG_SyncWeaponEntries_StartID && fmg.ID <= CFG.Current.FMG_SyncWeaponEntries_EndID)
                            {
                                // Grab base description
                                if (fmg.ID % CFG.Current.FMG_SyncWeaponEntries_Modulus == 0)
                                {
                                    holdingSyncDescription = true;
                                    syncDescription = fmg.Text;
                                    syncBaseId = fmg.ID;
                                }

                                // Base description has been grabbed
                                if (holdingSyncDescription)
                                {
                                    // If current FMG is a child of the base ID, then apply the
                                    // new description
                                    if (fmg.ID > syncBaseId && fmg.ID < (syncBaseId + CFG.Current.FMG_SyncWeaponEntries_Modulus))
                                    {
                                        TaskLogs.AddLog($"Updated FMG entry {fmg.ID}.");
                                        fmg.Text = syncDescription;
                                    }

                                    // After exceeding the child id range, reset the grab bool
                                    if (fmg.ID > (syncBaseId + CFG.Current.FMG_SyncWeaponEntries_Modulus))
                                    {
                                        holdingSyncDescription = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
