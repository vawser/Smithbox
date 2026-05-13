using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextMerge
{
    public TextEditorScreen Editor;
    public ProjectEntry Project;

    private ProjectEntry targetProject = null;
    private bool replaceModifiedRows = true;
    private bool isMergeInProgress = false;
    private bool mergePrimaryLanguageOnly = true;

    public TextMerge(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        var curView = Editor.ViewHandler.ActiveView;

        ImGui.BeginChild("TextMergeSection", ImGuiChildFlags.Borders);

        UIHelper.WrappedText("Use this to merge a target project's text files into your current project.");
        UIHelper.WrappedText("");
        UIHelper.WrappedText("Merging will bring all unique text from the target project into your project.\nIncludes modified text if enabled.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Target Project", "The project you want to merge text from.");

        // Project list
        ImGui.BeginChild("mergeProjectListSection", new Vector2(0, 200), ImGuiChildFlags.Borders);
        int index = 0;

        foreach (var proj in Smithbox.Orchestrator.Projects)
        {
            if (proj == null)
                continue;

            if (proj.Descriptor.ProjectType != curView.Project.Descriptor.ProjectType)
                continue;

            if (proj == Smithbox.Orchestrator.SelectedProject)
                continue;

            var isSelected = false;

            if (targetProject != null)
            {
                isSelected = targetProject.Descriptor.ProjectName == proj.Descriptor.ProjectName;
            }

            if (ImGui.Selectable($"{proj.Descriptor.ProjectName}##targetProject{index}", isSelected))
            {
                targetProject = proj;
            }

            index++;
        }
        ImGui.EndChild();

        UIHelper.SimpleHeader("Options", "");

        ImGui.Checkbox("Merge Primary Language Only##primaryLanguageOnly", ref mergePrimaryLanguageOnly);
        UIHelper.Tooltip("If enabled, then only the primary language FMGs will be merged.");

        ImGui.Checkbox("Replace Modified Entries##replaceModified", ref replaceModifiedRows);
        UIHelper.Tooltip("If enabled, then modified rows from the target will overwrite existing rows in our project. If not, then they will be ignored, and only unique rows will be merged.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("mergeActions",
            "mergeText", "Merge", "", MergeText);

        ImGui.EndChild();
    }

    public void MergeText()
    {
        if(targetProject == null)
        {
            Smithbox.LogError<TextMerge>("No project has been targeted.");
            return;
        }

        if(isMergeInProgress)
        {
            Smithbox.LogError<TextMerge>("Merge is already in progress.");
            return;
        }

        var curView = Editor.ViewHandler.ActiveView;

        HandleMergeAction(curView);
    }

    public async void HandleMergeAction(TextEditorView view)
    {
        isMergeInProgress = true;

        await view.Project.Handler.TextData.LoadAuxBank(targetProject, true);

        Task<bool> mergeTask = StartFmgMerge(view);
        bool mergeTaskResult = await mergeTask;

        if (mergeTaskResult)
        {
            Smithbox.Log(typeof(TextMerge), $"[Text Editor] Merged text from {targetProject.Descriptor.ProjectName} into this project.");
        }
        else
        {
            Smithbox.Log(typeof(TextMerge), $"[Text Editor] Failed to merge text from {targetProject.Descriptor.ProjectName}.");
        }

        isMergeInProgress = false;
    }

    private async Task<bool> StartFmgMerge(TextEditorView view)
    {
        await Task.Yield();

        if (!view.Project.Handler.TextData.AuxBanks.TryGetValue(targetProject.Descriptor.ProjectName, out var targetAuxBank))
            return false;

        foreach (var primaryEntry in view.Project.Handler.TextData.PrimaryBank.Containers)
        {
            var primaryKey = primaryEntry.Key.Filename;
            var currentContainer = primaryEntry.Value;

            if (mergePrimaryLanguageOnly &&
                CFG.Current.TextEditor_Primary_Category != currentContainer.ContainerDisplayCategory)
            {
                continue;
            }

            if (primaryEntry.Value.FmgWrappers == null || primaryEntry.Value.FmgWrappers.Count == 0)
            {
                view.Project.Handler.TextData.PrimaryBank.LoadFmgWrappers(primaryEntry.Value);
            }

            foreach (var targetEntry in targetAuxBank.Containers)
            {
                var targetKey = targetEntry.Key.Filename;
                var targetContainer = targetEntry.Value;

                if (primaryKey != targetKey ||
                    currentContainer.ContainerDisplayCategory != targetContainer.ContainerDisplayCategory)
                {
                    continue;
                }

                if (targetEntry.Value.FmgWrappers == null || targetEntry.Value.FmgWrappers.Count == 0)
                {
                    view.Project.Handler.TextData.PrimaryBank.LoadFmgWrappers(targetEntry.Value);
                }

                var targetWrapper = targetContainer;

                foreach (var curWrapper in currentContainer.FmgWrappers)
                {
                    foreach (var tarWrapper in targetWrapper.FmgWrappers)
                    {
                        if (curWrapper.ID == tarWrapper.ID)
                        {
                            targetContainer.IsModified = true;

                            await ProcessFmg(curWrapper, tarWrapper);
                        }
                    }
                }
            }
        }

        return true;
    }

    private async Task<bool> ProcessFmg(TextFmgWrapper sourceWrapper, TextFmgWrapper targetWrapper)
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

        if (replaceModifiedRows)
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
