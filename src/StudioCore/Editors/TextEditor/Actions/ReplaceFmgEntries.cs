using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Actions;

public class ReplaceFmgEntries : EditorAction
{
    private TextFmgWrapper TargetFmgInfo;
    private FMG OriginalFmg;
    private StoredFmgWrapper Wrapper;

    public ReplaceFmgEntries(TextFmgWrapper targetFmgInfo, StoredFmgWrapper wrapper)
    {
        TargetFmgInfo = targetFmgInfo;
        OriginalFmg = targetFmgInfo.File.Clone();
        Wrapper = wrapper;
    }

    public override ActionEvent Execute()
    {
        foreach (var entry in Wrapper.Fmg.Entries)
        {
            // Assign parent
            entry.Parent = TargetFmgInfo.File;

            var uniqueId = true;

            foreach(var tEntry in TargetFmgInfo.File.Entries)
            {
                if(tEntry.ID == entry.ID)
                {
                    uniqueId = false;
                    tEntry.Text = entry.Text;
                }
            }

            if(uniqueId)
            {
                TargetFmgInfo.File.Entries.Add(entry);
            }
        }

        TargetFmgInfo.File.Entries.Sort();

        Smithbox.EditorHandler.TextEditor.Selection.SelectFmg(TargetFmgInfo, true);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        TargetFmgInfo.File.Entries = OriginalFmg.Entries;
        TargetFmgInfo.File.Entries.Sort();

        Smithbox.EditorHandler.TextEditor.Selection.SelectFmg(TargetFmgInfo, true);

        return ActionEvent.NoEvent;
    }
}