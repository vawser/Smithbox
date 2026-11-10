using SoulsFormats;
using StudioCore.Editors.Common;
using System;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// Adds a TPF (as a binder file) to a container, or removes it (T3: add empty / duplicate / paste / delete TPF).
/// </summary>
public class TpfInsertRemoveAction : EditorAction
{
    private readonly TexEditorView View;
    private readonly BinderContents Container;
    private readonly BinderFile File;
    private readonly TPF Tpf;
    private readonly int Index;
    private readonly bool Insert;

    public TpfInsertRemoveAction(TexEditorView view, BinderContents container, BinderFile file, TPF tpf, int index, bool insert)
    {
        View = view;
        Container = container;
        File = file;
        Tpf = tpf;
        Index = index;
        Insert = insert;
    }

    public override ActionEvent Execute()
    {
        Apply(Insert);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Apply(!Insert);
        return ActionEvent.NoEvent;
    }

    private void Apply(bool insert)
    {
        var files = Container.Binder.Files;

        if (insert)
        {
            files.Insert(Math.Clamp(Index, 0, files.Count), File);
            Container.Files[File] = Tpf;
            Container.MarkModified(File);
            Container.StructureModified = true;
            View.OnTpfListChanged(Container, File);
        }
        else
        {
            files.Remove(File);
            Container.Files.Remove(File);
            Container.ModifiedFiles.Remove(File);
            Container.StructureModified = true;
            View.OnTpfListChanged(Container, null);
        }
    }
}

/// <summary>
/// Renames a TPF's binder file (its path inside the container).
/// </summary>
public class TpfRenameAction : EditorAction
{
    private readonly TexEditorView View;
    private readonly BinderContents Container;
    private readonly BinderFile File;
    private readonly string OldName;
    private readonly string NewName;

    public TpfRenameAction(TexEditorView view, BinderContents container, BinderFile file, string newName)
    {
        View = view;
        Container = container;
        File = file;
        OldName = file.Name;
        NewName = newName;
    }

    public override ActionEvent Execute()
    {
        File.Name = NewName;
        Container.StructureModified = true;
        View.OnTpfListChanged(Container, File);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        File.Name = OldName;
        Container.StructureModified = true;
        View.OnTpfListChanged(Container, File);
        return ActionEvent.NoEvent;
    }
}
