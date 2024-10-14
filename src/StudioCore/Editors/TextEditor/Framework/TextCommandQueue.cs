using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.ParticleEditor.ParticleBank;

namespace StudioCore.Editors.TextEditor;

public class TextCommandQueue
{
    private TextEditorScreen Screen;

    public TextCommandQueue(TextEditorScreen screen)
    {
        Screen = screen;
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

                foreach (var (path, info) in TextBank.FmgBank)
                {
                    if(info.Category.ToString() == category)
                    {
                        if(info.Name == containerName)
                        {
                            Screen.Selection.FocusFileSelection = true;
                            Screen.Selection.SelectFileContainer(info, index);

                            foreach (var fmg in info.FmgInfos)
                            {
                                if(fmg.Name == fmgName)
                                {
                                    Screen.Selection.FocusFmgSelection = true;
                                    Screen.Selection.SelectFmg(fmg, false);

                                    var entryIndex = 0;

                                    foreach (var entry in fmg.File.Entries)
                                    {
                                        if($"{entry.ID}" == fmgEntryId)
                                        {
                                            Screen.Selection.FocusFmgEntrySelection = true;
                                            Screen.Selection.SelectFmgEntry(entryIndex, entry, false);
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