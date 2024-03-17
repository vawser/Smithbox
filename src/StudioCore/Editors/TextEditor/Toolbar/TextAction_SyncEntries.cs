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
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Toolbar
{
    public static class TextAction_SyncEntries
    {
        private static bool holdingSyncText = false;
        private static string syncText = "";
        private static int syncBaseId = 0;

        private static string CurrentTargetType;
        private static string CurrentTextCategory;

        public static void Setup()
        {
            CurrentTargetType = "Selected Entry";
            CurrentTextCategory = "Description";

        }

        public static void Select()
        {
            if (ImGui.RadioButton("Sync Entries##tool_SyncEntries", TextEditorToolbar.SelectedAction == TextEditorAction.SyncEntries))
            {
                TextEditorToolbar.SelectedAction = TextEditorAction.SyncEntries;
            }
            ImguiUtils.ShowHoverTooltip("Use this to change all 'sub' entries that link to a 'base' entry.\n\nFor example, a weapon has a 'sub' entry for each infusion. This action can change all of the infusion entry descriptions to match the base entry.");
        }

        public static void Configure()
        {
            if (TextEditorToolbar.SelectedAction == TextEditorAction.SyncEntries)
            {
                ImGui.Text("Synchronize child entries (as defined below) with their parent entry.");
                ImGui.Text("");

                ImGui.Text("Text Category:");
                if (ImGui.BeginCombo("##Text Category", CurrentTextCategory))
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
                ImguiUtils.ShowHoverTooltip("Text category to sync.");
                ImGui.Text("");

                ImGui.Text("Parent Modulus:");
                ImGui.InputInt("##Modulus", ref CFG.Current.FMG_SyncEntries_Modulus);
                ImguiUtils.ShowHoverTooltip("The modulus used to detect which FMG entries are considered 'base' entries.\n\nExample:\nThe weapon entries are spaced by 10000 or more. Therefore the modulus will match with the 'base' entries, but 'sub' entries will not. This means we can then assume any entries after the 'base' entry but below the 'base' entry ID plus the modulus are 'sub' entries.");
                ImGui.Text("");
            }
        }

        public static void Act()
        {
            if (TextEditorToolbar.SelectedAction == TextEditorAction.SyncEntries)
            {
                if (ImGui.Button("Apply##action_Selection_SyncEntries", new Vector2(200, 32)))
                {
                    if (CFG.Current.Interface_ParamEditor_PromptUser)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Sync Entries action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            PerformEntrySync();
                        }
                    }
                    else
                    {
                        PerformEntrySync();
                    }
                }

            }
        }

        private static void PerformEntrySync()
        {
            foreach (var entry in StudioCore.TextEditor.FMGBank.FmgInfoBank)
            {
                if (entry.EntryCategory == TextEditorScreen._activeFmgInfo.EntryCategory)
                {
                    // Reset these after each iteration
                    holdingSyncText = false;
                    syncText = "";
                    syncBaseId = 0;

                    if (entry == null)
                    {
                        return;
                    }

                    // Title
                    if (CurrentTextCategory is "Title" or "All")
                    {
                        if (entry.EntryType == FmgEntryTextType.Title)
                        {
                            foreach (var fmg in entry.Fmg.Entries)
                            {
                                SyncText(fmg);
                            }
                        }
                    }

                    // TextBody
                    if (CurrentTextCategory is "Text Body" or "All")
                    {
                        if (entry.EntryType == FmgEntryTextType.TextBody)
                        {
                            foreach (var fmg in entry.Fmg.Entries)
                            {
                                SyncText(fmg);
                            }
                        }
                    }

                    // Summary
                    if (CurrentTextCategory is "Summary" or "All")
                    {
                        if (entry.EntryType == FmgEntryTextType.Summary)
                        {
                            foreach (var fmg in entry.Fmg.Entries)
                            {
                                SyncText(fmg);
                            }
                        }
                    }

                    // ExtraText
                    if (CurrentTextCategory is "Extra Text" or "All")
                    {
                        if (entry.EntryType == FmgEntryTextType.ExtraText)
                        {
                            foreach (var fmg in entry.Fmg.Entries)
                            {
                                SyncText(fmg);
                            }
                        }
                    }

                    // Description
                    if (CurrentTextCategory is "Description" or "All")
                    {
                        if (entry.EntryType == FmgEntryTextType.Description)
                        {
                            foreach (var fmg in entry.Fmg.Entries)
                            {
                                SyncText(fmg);
                            }
                        }
                    }
                }
            }
        }

        private static void SyncText(FMG.Entry entry)
        {
            // Grab base text
            if (entry.ID % CFG.Current.FMG_SyncEntries_Modulus == 0)
            {
                holdingSyncText = true;
                syncText = entry.Text;
                syncBaseId = entry.ID;
            }

            // Base text has been grabbed
            if (holdingSyncText)
            {
                // If current FMG is a child of the base ID, then apply the
                // new description
                if (entry.ID > syncBaseId && entry.ID < (syncBaseId + CFG.Current.FMG_SyncEntries_Modulus))
                {
                    TaskLogs.AddLog($"Updated FMG entry {entry.ID}.");
                    entry.Text = syncText;
                }

                // After exceeding the child id range, reset the grab bool
                if (entry.ID > (syncBaseId + CFG.Current.FMG_SyncEntries_Modulus))
                {
                    holdingSyncText = false;
                }
            }
        }
    }
}
