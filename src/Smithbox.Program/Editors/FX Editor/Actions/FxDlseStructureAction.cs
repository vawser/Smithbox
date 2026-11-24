using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.FxEditor.Model;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// A structure edit of a DLSE effect (FX.md, FFX §21, F7a): a state, exit, action or call-list entry added,
/// duplicated, deleted or moved. The whole effect is snapshotted to bytes before and after. The first execute edits
/// the loaded file in place; redo and undo put a freshly read copy in the entry, so the model is rebuilt
/// (<see cref="FxData.Effect"/> is keyed on the file object) and the selection, which held the old objects, is cleared.
/// </summary>
public class FxDlseStructureAction : EditorAction
{
    private readonly FxEditorView Editor;
    private readonly FxEntry Entry;
    private readonly Action<FFXDLSE> Apply;
    private readonly byte[] Before;
    private byte[] After;

    public FxDlseStructureAction(FxEditorView editor, FxEntry entry, Action<FFXDLSE> apply)
    {
        Editor = editor;
        Entry = entry;
        Apply = apply;
        Before = FfxStructure.Snapshot(entry.Ffx);
    }

    public override ActionEvent Execute()
    {
        if (After == null)
        {
            Apply(Entry.Ffx);
            After = FfxStructure.Snapshot(Entry.Ffx);
            // the edit was in place: the adapters' lists are rebuilt from the file
            Entry.ModelCache = null;
        }
        else
        {
            Put(After);
        }
        Editor.Data.Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Put(Before);
        Editor.Data.Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }

    private void Put(byte[] bytes)
    {
        Entry.Ffx = FFXDLSE.Read(bytes);
        Entry.ModelCache = null;
        Editor.Selection.ClearItem();
    }
}
