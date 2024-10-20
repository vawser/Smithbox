using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKLib.hk2018.hkAsyncThreadPool;
using ImGuiNET;
using StudioCore.Core.Project;
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

        var currentInfo = editor.Selection.SelectedContainer;
        var currentCategory = currentInfo.ContainerDisplayCategory;

        // <language X> -> Add Unique Entries from Primary

        foreach (TextContainerCategory category in Enum.GetValues(typeof(TextContainerCategory)))
        {
            if (TextBank.FmgBank.Any(e => e.Value.ContainerDisplayCategory == category) && editor.FileView.AllowedCategory(category))
            {
                var targetContainer = TextBank.FmgBank
                    .Where(e => e.Value.ContainerDisplayCategory == editor.Selection.SelectedContainer.ContainerDisplayCategory)
                    .Where(e => e.Value.ContainerDisplayCategory != currentCategory)
                    .FirstOrDefault();

                if (targetContainer.Value != null)
                {
                    var targetInfo = targetContainer.Value;

                    var displayName = targetInfo.Filename;

                    if (CFG.Current.TextEditor_DisplayCommunityContainerName)
                    {
                        // To get nice DS2 names, apply the FMG display name stuff on the container level
                        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                        {
                            displayName = TextUtils.GetFmgDisplayName(targetInfo, -1, targetInfo.Filename);
                        }
                        else
                        {
                            displayName = targetInfo.GetContainerDisplayName();
                        }
                    }

                    if (ImGui.Selectable($"{category.GetDisplayName()}: {displayName}##{targetInfo.Filename}"))
                    {
                        SyncLanguage(currentInfo, targetInfo);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sync currently selected category into chosen category
    /// </summary>
    public static void SyncLanguage(TextContainerWrapper sourceContainer, TextContainerWrapper targetContainer)
    {

    }
}
