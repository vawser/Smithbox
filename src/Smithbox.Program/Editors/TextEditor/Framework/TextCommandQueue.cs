using SoulsFormats;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextCommandQueue
{
    private TextEditorScreen Editor;

    public TextCommandQueue(TextEditorScreen screen)
    {
        Editor = screen;
    }

    public void Parse(string[] initcmd)
    {
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

                foreach (var (fileEntry, info) in Editor.Project.TextData.PrimaryBank.Entries)
                {
                    bool found = false;
                    FMG.Entry? entry = null;
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
                                Editor.Selection.FocusFmgSelection = true;
                                Editor.Selection.SelectFmg(fmg);
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

                    if (found)
                    {
                        Editor.Selection.FocusFileSelection = true;
                        Editor.Selection.SelectFileContainer(fileEntry, info, fileIndex);
                        Editor.Selection.FocusFmgEntrySelection = true;
                        Editor.Selection.SelectFmgEntry(index, entry);
                        break;
                    }
                    fileIndex++;
                }
            }
        }
    }
}