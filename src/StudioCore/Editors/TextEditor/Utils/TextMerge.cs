using ImGuiNET;
using Octokit;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Utils;

public static class TextMerge
{
    public static string TargetProjectDir = "";

    public static bool ReplaceModifiedRows = false;

    public static void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        if (ImGui.BeginTable($"textMergeTable", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            // Row 1
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Target Project");
            UIHelper.ShowHoverTooltip("The project you want to merge text from.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth() * 0.75f); 
            ImGui.InputText("##targetProjectDir", ref TargetProjectDir, 255);
            ImGui.SameLine();
            if (ImGui.Button($@"{ForkAwesome.FileO}"))
            {
                if (PlatformUtils.Instance.OpenFolderDialog("Select project directory...", out var path))
                {
                    TargetProjectDir = path;
                }
            }

            // Row 2
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Replace Modified Entries");

            ImGui.TableSetColumnIndex(1);

            ImGui.Checkbox("##replaceModified", ref ReplaceModifiedRows);
            UIHelper.ShowHoverTooltip("If enabled, then modified rows from the target will overwrite existing rows in our project. If not, then they will be ignored, and only unique rows will be merged.");

            ImGui.EndTable();
        }

        if(ImGui.Button("Merge", defaultButtonSize))
        {
            ApplyMerge();
        }
    }

    private static void ApplyMerge()
    {
        if(TargetProjectDir == "")
        {
            TaskLogs.AddLog("Specified target directory is invalid!");
            return;
        }

        // Load target project text files in
        if (Directory.Exists(TargetProjectDir))
        {
            TextBank.LoadTargetTextFiles(TargetProjectDir);
        }

        StartFmgMerge();
    }

    private static void StartFmgMerge()
    {
        /// Filter through containers, only process FMGs for each if they match
        foreach (var entry in TextBank.FmgBank)
        {
            var primaryKey = Path.GetFileName(entry.Key);

            foreach (var pEntry in TextBank.TargetFmgBank)
            {
                var targetKey = Path.GetFileName(pEntry.Key);

                if (primaryKey == targetKey)
                {
                    var targetEntry = TextBank.TargetFmgBank[entry.Key];

                    foreach (var fmgInfo in entry.Value.FmgInfos)
                    {
                        foreach (var targetInfo in targetEntry.FmgInfos)
                        {
                            if (fmgInfo.ID == targetInfo.ID)
                            {
                                ProcessFmg(fmgInfo, targetInfo);
                            }
                        }
                    }
                }
            }
        }

        TaskLogs.AddLog($"Applied FMG Merge.");
    }

    private static void ProcessFmg(FmgInfo sourceInfo, FmgInfo targetInfo)
    {
        List<FMG.Entry> missingEntries = new();
        List<FMG.Entry> modifiedEntries = new();

        foreach (var entry in targetInfo.File.Entries)
        {
            // Entry ID not present in source, therefore add to missing entries
            if(!sourceInfo.File.Entries.Any(e => e.ID == entry.ID))
            {
                missingEntries.Add(entry);
            }

            // Entry ID is present in source,
            // Entry Text not present in source, therefore add to modified entries
            if (sourceInfo.File.Entries.Any(e => e.ID == entry.ID) &&
                !sourceInfo.File.Entries.Any(e => e.Text == entry.Text))
            {
                modifiedEntries.Add(entry);
            }
        }

        // Add Missing
        foreach(var entry in missingEntries)
        {
            TaskLogs.AddLog($"{entry.ID} {entry.Text}");
            sourceInfo.File.Entries.Add(entry);
        }

        // Change Modified
        foreach (var entry in modifiedEntries)
        {
            TaskLogs.AddLog($"{entry.ID} {entry.Text}");
            var targetEntry = sourceInfo.File.Entries[entry.ID];
            targetEntry.Text = entry.Text;
        }

        TaskLogs.AddLog($"Modified {sourceInfo.Name} FMG");
    }
}
