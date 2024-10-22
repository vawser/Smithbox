using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKLib.hk2018.hkAsyncThreadPool;
using ImGuiNET;
using Microsoft.Extensions.DependencyModel;
using Octokit;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Interface;
using StudioCore.Utilities;

namespace StudioCore.Editors.TextEditor.Utils;

public static class LanguageSync
{
    /// <summary>
    /// Options to sync to
    /// </summary>
    public static void DisplaySyncOptions()
    {
        var editor = Smithbox.EditorHandler.TextEditor;

        var currentContainerWrapper = editor.Selection.SelectedContainerWrapper;

        if (ImGui.BeginMenu("Sync With"))
        {
            foreach(var entry in TextBank.FmgBank)
            {
                var container = entry.Value;

                var proceed = false;

                if(container.Filename == currentContainerWrapper.Filename &&
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
                    var displayName = container.Filename;

                    if (CFG.Current.TextEditor_DisplayCommunityContainerName)
                    {
                        // To get nice DS2 names, apply the FMG display name stuff on the container level
                        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                        {
                            displayName = TextUtils.GetFmgDisplayName(container, -1, container.Filename);
                        }
                        else
                        {
                            displayName = container.GetContainerDisplayName();
                        }
                    }

                    if (ImGui.Selectable($"{container.ContainerDisplayCategory.GetDisplayName()}: {displayName}##{container.Filename}"))
                    {
                        SyncLanguage(currentContainerWrapper, container);
                    }
                }
            }

            ImGui.EndMenu();
        }
        UIHelper.ShowHoverTooltip("Sync all unique changes from another category into this category.");
    }

    /// <summary>
    /// Sync currently selected category into chosen category
    /// </summary>
    private static void SyncLanguage(TextContainerWrapper targetContainer, TextContainerWrapper sourceContainer)
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

    private static void ProcessFmg(TextFmgWrapper targetWrapper, TextFmgWrapper sourceWrapper)
    {
        var editor = Smithbox.EditorHandler.TextEditor;

        // Add new
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

            // TODO: add modified detection based on source entry difference state
            /*
            else if (targetWrapper.File.Entries.Any(e => e.ID == srcEntry.ID))
            {
                var targetEntry = targetWrapper.File.Entries.Where(e => e.ID == srcEntry.ID).FirstOrDefault();
                if(targetEntry != null)
                {
                    var newText = targetEntry.Text;

                    var newEntry = new FMG.Entry(targetWrapper.File, srcEntry.ID, newText);
                    targetWrapper.File.Entries.Add(newEntry);
                }
            }
            */
        }

        targetWrapper.File.Entries.Sort();
    }
}
