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
    private FmgInfo TargetFmgInfo;
    private FMG OriginalFmg;
    private FmgWrapper Wrapper;

    public ReplaceFmgEntries(FmgInfo targetFmgInfo, FmgWrapper wrapper)
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
            TargetFmgInfo.File.Entries.Add(entry);
        }

        TargetFmgInfo.File.Entries.Sort();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        TargetFmgInfo.File.Entries = OriginalFmg.Entries;
        TargetFmgInfo.File.Entries.Sort();

        return ActionEvent.NoEvent;
    }
}