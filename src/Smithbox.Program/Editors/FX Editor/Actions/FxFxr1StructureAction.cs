using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.FxEditor.Model;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// A structure edit of an FXR1 / FXR v2 effect (DS1, DS1R, BB; FFX Older Games Plan §21.8, F7b), as
/// <see cref="FxDlseStructureAction"/> is for DLSE: whole-file snapshots before and after; the first run edits in place,
/// undo and redo put a freshly read copy in the entry (the model is rebuilt, the selection cleared).
/// </summary>
public class FxFxr1StructureAction : EditorAction
{
    private readonly FxEditorView Editor;
    private readonly FxEntry Entry;
    private readonly Action<FXR1> Apply;
    private readonly byte[] Before;
    private byte[] After;

    public FxFxr1StructureAction(FxEditorView editor, FxEntry entry, Action<FXR1> apply)
    {
        Editor = editor;
        Entry = entry;
        Apply = apply;
        Before = Fxr1Structure.Snapshot(entry.Fxr1);
    }

    public override ActionEvent Execute()
    {
        if (After == null)
        {
            Apply(Entry.Fxr1);
            After = Fxr1Structure.Snapshot(Entry.Fxr1);
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
        Entry.Fxr1 = FXR1.Read(bytes);
        Entry.ModelCache = null;
        Editor.Selection.ClearItem();
    }
}
