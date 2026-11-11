using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Shared work of the actions that add, remove and rename material files: marking the binder changed, refreshing
/// the bank's name lookups, and keeping every view's selection valid
/// </summary>
public abstract class MatFileAction : EditorAction
{
    protected readonly MaterialEditorScreen Editor;
    protected readonly MaterialBank Bank;
    protected readonly IMaterialBinder Binder;

    protected MatFileAction(MaterialEditorScreen editor, MaterialBank bank, IMaterialBinder binder)
    {
        Editor = editor;
        Bank = bank;
        Binder = binder;
    }

    protected void Finish(bool undo)
    {
        if (undo)
            Binder.MarkStructureUndone();
        else
            Binder.MarkStructureChanged();

        Bank.RebuildLookups();

        foreach (var view in Editor.ViewHandler.MaterialViews)
        {
            view?.Selection.Revalidate();
        }
    }

    /// <summary>
    /// A MATBIN's SourcePath follows its file name in vanilla (…\matxml\c4800_Body.matxml for c4800_Body.matbin).
    /// When it does, a new name carries over, with the key that follows SourcePath.
    /// </summary>
    protected static void FollowName(MaterialFileWrapper file, string oldShortName, string newShortName)
    {
        if (file is not MATBINFileWrapper { MATBIN: var matbin })
            return;

        var source = matbin.SourcePath ?? "";

        if (!string.Equals(MaterialBank.ShortNameOf(source), oldShortName, StringComparison.OrdinalIgnoreCase))
            return;

        matbin.SourcePath = MaterialBank.WithShortName(source, newShortName);
        MaterialUtils.ApplyMatbinKey(matbin);
    }
}

/// <summary>
/// Adds a copy of a material file to its binder, under a new short name, right after the original
/// </summary>
public class MatDuplicateFile : MatFileAction
{
    private readonly MaterialFileWrapper Copy;
    private readonly int Index;

    public MaterialFileWrapper NewFile => Copy;

    public MatDuplicateFile(MaterialEditorScreen editor, MaterialBank bank, IMaterialBinder binder,
        MaterialFileWrapper source, string newShortName) : base(editor, bank, binder)
    {
        var newKey = MaterialBank.WithShortName(source.Key, newShortName);

        Copy = binder.CreateCopy(source, newKey);
        FollowName(Copy, MaterialBank.ShortNameOf(source.Key), newShortName);
        Copy.MarkChanged();

        Index = binder.FileOrder.ToList().IndexOf(source.Key) + 1;
    }

    public override ActionEvent Execute()
    {
        Binder.InsertFile(Index, Copy);
        Finish(false);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Binder.RemoveFile(Copy.Key);
        Finish(true);
        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Adds a new material file, made elsewhere (an import), to a binder at a place in its order
/// </summary>
public class MatInsertFile : MatFileAction
{
    private readonly MaterialFileWrapper File;
    private readonly int Index;

    public MatInsertFile(MaterialEditorScreen editor, MaterialBank bank, IMaterialBinder binder, MaterialFileWrapper file, int index)
        : base(editor, bank, binder)
    {
        File = file;
        Index = index;
        File.MarkChanged();
    }

    public override ActionEvent Execute()
    {
        Binder.InsertFile(Index, File);
        MaterialPreview.Queue(File.Key);
        Finish(false);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Binder.RemoveFile(File.Key);
        MaterialPreview.Queue(File.Key);
        Finish(true);
        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Removes a material file from its binder. Undo puts the same file back where it was.
/// </summary>
public class MatDeleteFile : MatFileAction
{
    private readonly MaterialFileWrapper File;
    private int Index = -1;

    public MatDeleteFile(MaterialEditorScreen editor, MaterialBank bank, IMaterialBinder binder, MaterialFileWrapper file)
        : base(editor, bank, binder)
    {
        File = file;
    }

    public override ActionEvent Execute()
    {
        Index = Binder.RemoveFile(File.Key);
        MaterialPreview.Queue(File.Key);
        Finish(false);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Binder.InsertFile(Index, File);
        MaterialPreview.Queue(File.Key);
        Finish(true);
        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Gives a material file a new short name, in the same folder. A MATBIN's SourcePath and key follow when they
/// matched the old name.
/// </summary>
public class MatRenameFile : MatFileAction
{
    private readonly MaterialFileWrapper File;
    private readonly string OldKey;
    private readonly string NewKey;
    private readonly string NewShortName;

    private readonly string OldSourcePath;
    private readonly uint OldMatbinKey;

    public MatRenameFile(MaterialEditorScreen editor, MaterialBank bank, IMaterialBinder binder,
        MaterialFileWrapper file, string newShortName) : base(editor, bank, binder)
    {
        File = file;
        OldKey = file.Key;
        NewKey = MaterialBank.WithShortName(file.Key, newShortName);
        NewShortName = newShortName;

        if (file is MATBINFileWrapper { MATBIN: var matbin })
        {
            OldSourcePath = matbin.SourcePath;
            OldMatbinKey = matbin.Key;
        }
    }

    public override ActionEvent Execute()
    {
        Binder.RenameFile(OldKey, NewKey);
        MaterialPreview.Queue(OldKey);
        FollowName(File, MaterialBank.ShortNameOf(OldKey), NewShortName);
        File.MarkChanged();
        Finish(false);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Binder.RenameFile(NewKey, OldKey);
        MaterialPreview.Queue(NewKey);

        if (File is MATBINFileWrapper { MATBIN: var matbin })
        {
            matbin.SourcePath = OldSourcePath;
            matbin.Key = OldMatbinKey;
        }

        File.MarkUndone();
        Finish(true);
        return ActionEvent.NoEvent;
    }
}
