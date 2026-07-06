using Hexa.NET.ImGui;
using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

public class TextMerge
{
    public TextEditorView View;
    public ProjectEntry Project;

    private ProjectEntry targetProject = null;
    private bool replaceModifiedRows = true;
    private bool isMergeInProgress = false;
    private bool mergePrimaryLanguageOnly = true;

    public TextMerge(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        ImGui.BeginChild("TextMergeSection", ImGuiChildFlags.Borders);

        UIHelper.WrappedText(LOC.Get("TEXT_TextMerge_Hint"));

        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextMerge_Header_Target_Project"),
            LOC.Get("TEXT_TextMerge_Header_Target_Project"));

        // Project list
        ImGui.BeginChild("mergeProjectListSection", new Vector2(0, 200), ImGuiChildFlags.Borders);
        int index = 0;

        foreach (var proj in Smithbox.Orchestrator.Projects)
        {
            if (proj == null)
                continue;

            if (proj.Descriptor.ProjectType != View.Project.Descriptor.ProjectType)
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

        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextMerge_Header_Options"),
            LOC.Get("TEXT_TextMerge_Header_Options_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXT_TextMerge_Checkbox_Merge_Primary_Lang")}##primaryLanguageOnly", ref mergePrimaryLanguageOnly);
        UIHelper.Tooltip(LOC.Get("TEXT_TextMerge_Checkbox_Merge_Primary_Lang_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXT_TextMerge_Checkbox_Replace_Modified")}##replaceModified", ref replaceModifiedRows);
        UIHelper.Tooltip(LOC.Get("TEXT_TextMerge_Checkbox_Replace_Modified_TT"));

        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_TextMerge_Header_Actions"),
            LOC.Get("TEXT_TextMerge_Header_Actions_TT"));

        UIHelper.MultiButtonInput("mergeActions",
            "mergeText", 
            LOC.Get("TEXT_TextMerge_Action_Merge"), 
            LOC.Get("TEXT_TextMerge_Action_Merge_TT"), 
            MergeText);

        ImGui.EndChild();
    }

    public void MergeText()
    {
        if(targetProject == null)
        {
            Smithbox.LogError<TextMerge>(LOC.Get("TEXT_TextMerge_Log_No_Target_Project"));
            return;
        }

        if(isMergeInProgress)
        {
            Smithbox.LogError<TextMerge>(LOC.Get("TEXT_TextMerge_Log_Merge_In_Progress"));
            return;
        }

        HandleMergeAction(View);
    }

    public async void HandleMergeAction(TextEditorView view)
    {
        isMergeInProgress = true;

        await view.Project.Handler.TextData.LoadAuxBank(targetProject, true);

        Task<bool> mergeTask = StartFmgMerge(view);
        bool mergeTaskResult = await mergeTask;

        if (mergeTaskResult)
        {
            Smithbox.Log(typeof(TextMerge),
                LOC.Get("TEXT_TextMerge_Log_Merged_Text_PASS", targetProject.Descriptor.ProjectName));
        }
        else
        {
            Smithbox.Log(typeof(TextMerge),
                LOC.Get("TEXT_TextMerge_Log_Merged_Text_FAIL", targetProject.Descriptor.ProjectName));
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
