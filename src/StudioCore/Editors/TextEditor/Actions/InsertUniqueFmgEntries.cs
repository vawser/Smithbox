using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Actions;

public class InsertUniqueFmgEntries : EditorAction
{
    private FmgInfo TargetFmgInfo;
    private FMG OriginalFmg;
    private FmgWrapper Wrapper;

    public InsertUniqueFmgEntries(FmgInfo targetFmgInfo, FmgWrapper wrapper)
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

            if (!TargetFmgInfo.File.Entries.Where(e => e.ID == entry.ID).Any())
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