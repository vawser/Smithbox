using SoulsFormats;
using StudioCore.Application;
using System;
using System.Linq;

namespace StudioCore.Editors.TextEditor;

public class TextCommandQueue
{
    private TextEditorScreen Editor;
    private ProjectEntry Project;

    public bool DoFocus = false;

    public TextCommandQueue(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] commands)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        // text / select / category / container name / fmg name / fmg entry id
        if (commands != null && commands.Length > 0 && commands[0] == "select")
        {
            if (commands.Length > 4 && activeView != null)
            {
                var category = commands[1];
                var containerName = commands[2];
                var fmgName = commands[3];
                var fmgEntryId = commands[4];

                // Try the containers in the order the game uses them: DLC2, then DLC1, then base.
                // The bank is a ConcurrentDictionary with no defined order, so taking the first
                // match could jump to an obsolete container (e.g. ER 'item' instead of 'item_dlc02').
                string[] candidateNames =
                {
                    $"{containerName}_dlc02",
                    $"{containerName}_dlc2",
                    $"{containerName}_dlc01",
                    $"{containerName}_dlc1",
                    containerName
                };

                var containers = Editor.Project.Handler.TextData.PrimaryBank.Containers;

                foreach (var candidate in candidateNames)
                {
                    foreach (var (fileEntry, info) in containers)
                    {
                        if (info.ContainerDisplayCategory.ToString() != category)
                            continue;

                        if (info.FileEntry.Filename != candidate)
                            continue;

                        if (info.FmgWrappers == null)
                            continue;

                        foreach (var fmg in info.FmgWrappers)
                        {
                            if (fmg.Name != fmgName)
                                continue;

                            var entry = fmg.File.Entries.FirstOrDefault(e => e.ID.ToString() == fmgEntryId);

                            if (entry == null)
                                continue;

                            var index = fmg.File.Entries.IndexOf(entry);

                            // The container list highlights by file entry, so the index argument
                            // is only informational here.
                            activeView.Selection.SelectFileContainer(fileEntry, info, -1);
                            activeView.Selection.FocusFileSelection = true;

                            activeView.Selection.SelectFmg(fmg);
                            activeView.Selection.FocusFmgSelection = true;

                            activeView.Selection.SelectFmgEntry(index, entry);
                            activeView.TextEntryList.FocusSelection = true;

                            return;
                        }
                    }
                }
            }
        }
    }
}
