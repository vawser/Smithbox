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

    public void Parse(string[] initcmd)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        // text / select / category / container name / fmg name / fmg entry id
        if (initcmd != null && initcmd[0] == "select")
        {
            if (initcmd.Length > 4)
            {
                var category = initcmd[1];
                var containerName = initcmd[2];
                var fmgName = initcmd[3];
                var fmgEntryId = initcmd[4];

                var fileIndex = 0;

                TextFmgWrapper targetFmg = null;

                foreach (var (fileEntry, info) in Editor.Project.Handler.TextData.PrimaryBank.Containers)
                {
                    bool found = false;
                    FMG.Entry entry = null;
                    int index = -1;

                    if (info.ContainerDisplayCategory.ToString() == category &&
                    (
                        info.FileEntry.Filename == $"{containerName}_dlc02" ||
                        info.FileEntry.Filename == $"{containerName}_dlc01" ||
                        info.FileEntry.Filename == containerName)
                    )
                    {
                        foreach (var fmg in info.FmgWrappers)
                        {
                            if (fmg.Name == fmgName)
                            {
                                targetFmg = fmg;

                                entry = fmg.File.Entries.FirstOrDefault(e => e.ID.ToString() == fmgEntryId);
                                if (entry != null)
                                {
                                    index = fmg.File.Entries.IndexOf(entry);
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (found && activeView != null)
                    {
                        activeView.Selection.SelectFileContainer(fileEntry, info, fileIndex);
                        activeView.Selection.FocusFileSelection = true;

                        activeView.Selection.SelectFmg(targetFmg);
                        activeView.Selection.FocusFmgSelection = true;

                        activeView.Selection.SelectFmgEntry(index, entry);
                        activeView.Selection.FocusFmgEntrySelection = true;

                        break;
                    }
                    fileIndex++;
                }
            }
        }
    }
}