namespace StudioCore.Editors.EsdEditor;

/// <summary>Adds a container made in the editor (a new or duplicated binder, a duplicated loose ESD) to the file list</summary>
public class EsdContainerAddOp : IEsdOp
{
    private readonly EsdBank Bank;
    private readonly EsdContainer Container;

    public EsdContainerAddOp(EsdBank bank, EsdContainer container)
    {
        Bank = bank;
        Container = container;
    }

    public void Do() => Bank.Insert(Container);
    public void Undo() => Bank.Remove(Container);
}

/// <summary>
/// Deletes a container: an unsaved one just leaves the list; a project-only file leaves it and its project file is
/// moved into the backup folder on the next save
/// </summary>
public class EsdContainerRemoveOp : IEsdOp
{
    private readonly EsdBank Bank;
    private readonly EsdContainer Container;
    private int Index = -1;

    public EsdContainerRemoveOp(EsdBank bank, EsdContainer container)
    {
        Bank = bank;
        Container = container;
    }

    public void Do()
    {
        Index = Bank.Remove(Container);
        if (!Container.IsNew)
            Bank.PendingDeletes.Add(Container.Path);
    }

    public void Undo()
    {
        Bank.PendingDeletes.Remove(Container.Path);
        Bank.Insert(Container, Index);
    }
}

/// <summary>Deletes the project copy of a file the game ships too: it reverts to the game's on the next save</summary>
public class EsdContainerRevertOp : IEsdOp
{
    private readonly EsdContainer Container;

    public EsdContainerRevertOp(EsdContainer container)
    {
        Container = container;
    }

    public void Do() => Container.PendingRevert = true;
    public void Undo() => Container.PendingRevert = false;
}
