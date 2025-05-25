using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Utils;

public static class TextMerge
{
    public static ProjectEntry TargetProject = null;

    public static bool ReplaceModifiedRows = true;

    public static bool MergeInProgress = false;

    public static bool PrimaryLanguageOnly = true;

    public static void Display(TextEditorScreen editor)
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        UIHelper.WrappedText("Use this to merge a target project's text files into your current project.");
        UIHelper.WrappedText("");
        UIHelper.WrappedText("Merging will bring all unique text from the target project into your project.\nIncludes modified text if enabled.");
        UIHelper.WrappedText("");

        UIHelper.SimpleHeader("targetProject", "Target Project", "The project you want to merge text from.", UI.Current.ImGui_AliasName_Text);

        // Project list

        foreach (var proj in editor.Project.BaseEditor.ProjectManager.Projects)
        {
            if (proj == null)
                continue;

            if (proj.ProjectType != editor.Project.ProjectType)
                continue;

            if (proj == editor.Project.BaseEditor.ProjectManager.SelectedProject)
                continue;

            var isSelected = false;

            if (TargetProject != null)
            {
                isSelected = TargetProject.ProjectName == proj.ProjectName;
            }

            if (ImGui.Selectable($"{proj.ProjectName}", isSelected))
            {
                TargetProject = proj;
            }
        }

        UIHelper.WrappedText("");
        ImGui.Checkbox("Merge Primary Language Only##primaryLanguageOnly", ref PrimaryLanguageOnly);
        UIHelper.Tooltip("If enabled, then only the primary language FMGs will be merged.");

        ImGui.Checkbox("Replace Modified Entries##replaceModified", ref ReplaceModifiedRows);
        UIHelper.Tooltip("If enabled, then modified rows from the target will overwrite existing rows in our project. If not, then they will be ignored, and only unique rows will be merged.");

        if (TargetProject == null || MergeInProgress)
        {
            ImGui.BeginDisabled();
            if (ImGui.Button("Merge##action_MergeText", defaultButtonSize))
            {
            }
            ImGui.EndDisabled();
        }
        else if (!MergeInProgress)
        {
            if (ImGui.Button("Merge##action_MergeText", defaultButtonSize))
            {
                HandleMergeAction(editor);
            }
        }

        if(MergeInProgress)
        {
            UIHelper.WrappedText("");
            UIHelper.WrappedText("Text merge is in progress...");
        }
    }

    public static async void HandleMergeAction(TextEditorScreen editor)
    {
        MergeInProgress = true;

        await editor.Project.TextData.LoadAuxBank(TargetProject, true);

        Task<bool> mergeTask = StartFmgMerge(editor);
        bool mergeTaskResult = await mergeTask;

        if (mergeTaskResult)
        {
            TaskLogs.AddLog($"[{editor.Project.ProjectName}:Text Editor] Merged text from {TargetProject.ProjectName} into this project.");
        }
        else
        {
            TaskLogs.AddLog($"[{editor.Project.ProjectName}:Text Editor] Failed to merge text from {TargetProject.ProjectName}.");
        }

        MergeInProgress = false;
    }

    private static async Task<bool> StartFmgMerge(TextEditorScreen editor)
    {
        await Task.Yield();

        if (!editor.Project.TextData.AuxBanks.TryGetValue(TargetProject.ProjectName, out var targetAuxBank))
            return false;

        foreach (var primaryEntry in editor.Project.TextData.PrimaryBank.Entries)
        {
            var primaryKey = primaryEntry.Key.Filename;
            var currentContainer = primaryEntry.Value;

            if (PrimaryLanguageOnly &&
                CFG.Current.TextEditor_PrimaryCategory != currentContainer.ContainerDisplayCategory)
            {
                continue;
            }

            foreach (var targetEntry in targetAuxBank.Entries)
            {
                var targetKey = targetEntry.Key.Filename;
                var targetContainer = targetEntry.Value;

                // Skip if not same file or category
                if (primaryKey != targetKey ||
                    currentContainer.ContainerDisplayCategory != targetContainer.ContainerDisplayCategory)
                {
                    continue;
                }

                // Directly access the matching wrapper (skip Where clause)
                var targetWrapper = targetContainer;

                foreach (var curWrapper in currentContainer.FmgWrappers)
                {
                    foreach (var tarWrapper in targetWrapper.FmgWrappers)
                    {
                        if (curWrapper.ID == tarWrapper.ID)
                        {
                            await ProcessFmg(curWrapper, tarWrapper);
                        }
                    }
                }
            }
        }

        return true;
    }

    private static async Task<bool> ProcessFmg(TextFmgWrapper sourceWrapper, TextFmgWrapper targetWrapper)
    {
        await Task.Yield();

        var sourceLookup = sourceWrapper.File.Entries.ToLookup(e => e.ID);
        List<FMG.Entry> missingEntries = new();
        List<FMG.Entry> modifiedEntries = new();

        foreach (var entry in targetWrapper.File.Entries)
        {
            var matchingSourceEntries = sourceLookup[entry.ID];

            if (!matchingSourceEntries.Any())
            {
                missingEntries.Add(entry);
            }
            else if (!matchingSourceEntries.Any(e => e.Text == entry.Text))
            {
                modifiedEntries.Add(entry);
            }
        }

        missingEntries.Sort((a, b) => a.ID.CompareTo(b.ID));
        var sourceList = sourceWrapper.File.Entries;

        foreach (var entry in missingEntries)
        {
            int insertIndex = sourceList.FindLastIndex(e => e.ID < entry.ID);

            var newEntry = entry.Clone();
            newEntry.Parent = sourceWrapper.File;

            sourceList.Insert(insertIndex + 1, newEntry);
        }

        if (ReplaceModifiedRows)
        {
            foreach (var entry in modifiedEntries)
            {
                var toModify = sourceWrapper.File.Entries.FirstOrDefault(e => e.ID == entry.ID);
                if (toModify != null)
                {
                    toModify.Text = entry.Text;
                }
            }
        }

        return true;
    }
}
