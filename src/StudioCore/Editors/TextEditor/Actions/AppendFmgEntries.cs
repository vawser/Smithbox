using Org.BouncyCastle.Crypto;
using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Actions;

public class AppendFmgEntries : EditorAction
{
    private TextFmgWrapper TargetFmgInfo;
    private FMG OriginalFmg;
    private StoredFmgWrapper Wrapper;

    public AppendFmgEntries(TextFmgWrapper targetFmgInfo, StoredFmgWrapper wrapper)
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
            else
            {
                // Replace existing entry if it matches wrapper entry
                var existingEntry = TargetFmgInfo.File.Entries.Where(e => e.ID == entry.ID).FirstOrDefault();
                existingEntry = entry;
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