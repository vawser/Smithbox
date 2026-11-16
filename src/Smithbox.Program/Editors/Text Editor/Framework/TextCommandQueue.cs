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

                // The named container first: every sender passes the filename of the container the
                // entry was found in. Trying the DLC names first jumped a result in e.g. ER 'item'
                // (listed when Hide Unused is off) to 'item_dlc02' when that has the same ID.
                // The DLC names, in the order the game uses them, are only a fallback for when the
                // entry isn't in the named container.
                string[] candidateNames =
                {
                    containerName,
                    $"{containerName}_dlc02",
                    $"{containerName}_dlc2",
                    $"{containerName}_dlc01",
                    $"{containerName}_dlc1"
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

                            // Single selection (not SelectFmgEntry, which reads Shift/Ctrl and could
                            // range-select from a search result), shown even if the filter hid it
                            activeView.Selection.RevealEntry(index, entry);

                            return;
                        }
                    }
                }
            }
        }
    }
}
