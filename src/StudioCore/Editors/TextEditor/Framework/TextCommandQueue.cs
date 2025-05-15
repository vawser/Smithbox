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

                var index = 0;

                foreach (var (fileEntry, info) in Editor.Project.TextData.PrimaryBank.Entries)
                {
                    if(info.ContainerDisplayCategory.ToString() == category)
                    {
                        if(info.FileEntry.Filename == containerName)
                        {
                            Editor.Selection.FocusFileSelection = true;
                            Editor.Selection.SelectFileContainer(fileEntry, info, index);

                            foreach (var fmg in info.FmgWrappers)
                            {
                                if(fmg.Name == fmgName)
                                {
                                    Editor.Selection.FocusFmgSelection = true;
                                    Editor.Selection.SelectFmg(fmg);

                                    var entryIndex = 0;

                                    foreach (var entry in fmg.File.Entries)
                                    {
                                        if($"{entry.ID}" == fmgEntryId)
                                        {
                                            Editor.Selection.FocusFmgEntrySelection = true;
                                            Editor.Selection.SelectFmgEntry(entryIndex, entry);
                                        }

                                        entryIndex++;
                                    }
                                }
                            }
                        }
                    }

                    index++;
                }
            }
        }
    }
}