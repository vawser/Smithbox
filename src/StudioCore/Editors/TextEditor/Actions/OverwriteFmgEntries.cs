using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Actions;

public class OverwriteFmgEntries : EditorAction
{
    private TextFmgWrapper TargetFmgInfo;
    private FMG OriginalFmg;
    private StoredFmgWrapper Wrapper;

    public OverwriteFmgEntries(TextFmgWrapper targetFmgInfo, StoredFmgWrapper wrapper)
    {
        TargetFmgInfo = targetFmgInfo;
        OriginalFmg = targetFmgInfo.File.Clone();
        Wrapper = wrapper;
    }

    public override ActionEvent Execute()
    {
        TargetFmgInfo.File.Entries.Clear();

        foreach (var entry in Wrapper.Fmg.Entries)
        {
            // Assign parent
            entry.Parent = TargetFmgInfo.File;

            TargetFmgInfo.File.Entries.Add(entry);
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