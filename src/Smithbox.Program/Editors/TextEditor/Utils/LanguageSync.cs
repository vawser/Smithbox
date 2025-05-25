using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System.Linq;

namespace StudioCore.Editors.TextEditor.Utils;

public class LanguageSync
{
    public TextEditorScreen Editor;
    public ProjectEntry Project;

    public LanguageSync(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Options to sync to
    /// </summary>
    public void DisplaySyncOptions()
    {
        var currentContainerWrapper = Editor.Selection.SelectedContainerWrapper;

        if (ImGui.BeginMenu("Sync With"))
        {
            foreach(var entry in Editor.Project.TextData.PrimaryBank.Entries)
            {
                var container = entry.Value;

                var proceed = false;

                if(container.FileEntry.Filename == currentContainerWrapper.FileEntry.Filename &&
                   container.ContainerDisplayCategory != currentContainerWrapper.ContainerDisplayCategory)
                {
                    proceed = true;
                }

                if (proceed)
                {
                    if (CFG.Current.TextEditor_LanguageSync_PrimaryOnly)
                    {
                        proceed = false;

                        if (container.ContainerDisplayCategory == CFG.Current.TextEditor_PrimaryCategory)
                        {
                            proceed = true;
                        }
                    }
                }

                // Not current selection, but the same file in a different category
                if (proceed)
                {
                    var displayName = container.FileEntry.Filename;

                    if (CFG.Current.TextEditor_DisplayCommunityContainerName)
                    {
                        // To get nice DS2 names, apply the FMG display name stuff on the container level
                        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                        {
                            displayName = TextUtils.GetFmgDisplayName(Project, container, -1, container.FileEntry.Filename);
                        }
                        else
                        {
                            displayName = container.GetContainerDisplayName();
                        }
                    }

                    if (ImGui.Selectable($"{container.ContainerDisplayCategory.GetDisplayName()}: {displayName}##{container.FileEntry.Filename}"))
                    {
                        SyncLanguage(currentContainerWrapper, container);
                    }
                }
            }

            ImGui.EndMenu();
        }
        UIHelper.Tooltip("Sync all unique changes from another category into this category.");
    }

    /// <summary>
    /// Sync currently selected category into chosen category
    /// </summary>
    private void SyncLanguage(TextContainerWrapper targetContainer, TextContainerWrapper sourceContainer)
    {
        foreach(var targetWrapper in targetContainer.FmgWrappers)
        {
            foreach (var sourceWrapper in sourceContainer.FmgWrappers)
            {
                if(targetWrapper.ID == sourceWrapper.ID)
                {
                    ProcessFmg(targetWrapper, sourceWrapper);
                }
            }
        }
    }

    private void ProcessFmg(TextFmgWrapper targetWrapper, TextFmgWrapper sourceWrapper)
    {
        foreach (var srcEntry in sourceWrapper.File.Entries)
        {
            // Unique to source
            if(!targetWrapper.File.Entries.Any(e => e.ID == srcEntry.ID))
            {
                var newText = srcEntry.Text;

                if(CFG.Current.TextEditor_LanguageSync_AddPrefix)
                {
                    newText = $"{CFG.Current.TextEditor_LanguageSync_Prefix}{newText}";
                }

                var newEntry = new FMG.Entry(targetWrapper.File, srcEntry.ID, newText);
                targetWrapper.File.Entries.Add(newEntry);
            }
            else if (targetWrapper.File.Entries.Any(e => e.ID == srcEntry.ID))
            {
                var targetEntry = targetWrapper.File.Entries.Where(e => e.ID == srcEntry.ID).FirstOrDefault();

                if(targetEntry != null)
                {
                    if (Editor.DifferenceManager.IsDifferentToVanilla(srcEntry))
                    {
                        var newText = targetEntry.Text;

                        if (newText == null)
                            newText = "";

                        var newEntry = new FMG.Entry(targetWrapper.File, srcEntry.ID, newText);
                        targetWrapper.File.Entries.Add(newEntry);
                    }
                }
            }
        }

        targetWrapper.File.Entries.Sort();
    }
}
