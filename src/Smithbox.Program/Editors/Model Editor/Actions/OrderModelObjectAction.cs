using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Moves a model object within the objects of its type. Only a single object can be moved at a time.
/// </summary>
public class OrderModelObjectAction : ViewportAction, IModelHistoryItem
{
    private ModelEditorView View;
    private ProjectEntry Project;

    private ModelContainer Container;
    private List<ModelEntity> Selection = new();

    public string Describe()
    {
        return LOC.Get("MODEL_History_Reorder", ModelHistory.NameObjects(Selection.Cast<Entity>().ToList()));
    }

    // The object order before the last execution, or null if it didn't move anything
    private List<Entity> StoredObjectOrder;

    // The FLVER indices rewritten by the last execution
    private IndexChangeRecord IndexChanges;

    private TreeObjectOrderMovementType MoveSelectionDir;

    public OrderModelObjectAction(ModelEditorView view, ProjectEntry project,
       ModelContainer container, List<ModelEntity> objects, TreeObjectOrderMovementType moveDir)
    {
        View = view;
        Project = project;
        Container = container;

        Selection.AddRange(objects);

        MoveSelectionDir = moveDir;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        StoredObjectOrder = null;
        IndexChanges = null;

        if (Container == null || Selection.Count != 1)
            return ActionEvent.NoEvent;

        var curSel = Selection[0];
        var index = Container.Objects.IndexOf(curSel);

        // The model root and offset node aren't part of the objects, so they can't be moved
        if (index == -1 || curSel.WrappedObject == null)
            return ActionEvent.NoEvent;

        var classType = curSel.WrappedObject.GetType();

        bool IsSameType(int i) => Container.Objects[i].WrappedObject?.GetType() == classType;

        // The run of objects of the same type that the selection belongs to
        var indexStart = index;
        while (indexStart > 0 && IsSameType(indexStart - 1))
        {
            indexStart--;
        }

        var indexEnd = index;
        while (indexEnd < Container.Objects.Count - 1 && IsSameType(indexEnd + 1))
        {
            indexEnd++;
        }

        var targetIndex = MoveSelectionDir switch
        {
            TreeObjectOrderMovementType.Up => Math.Max(indexStart, index - 1),
            TreeObjectOrderMovementType.Down => Math.Min(indexEnd, index + 1),
            TreeObjectOrderMovementType.Top => indexStart,
            TreeObjectOrderMovementType.Bottom => indexEnd,
            _ => index
        };

        if (targetIndex == index)
            return ActionEvent.NoEvent;

        StoredObjectOrder = new List<Entity>(Container.Objects);

        var materialsBefore = ModelIndexRemapper.GetMaterials(Container);
        var nodesBefore = ModelIndexRemapper.GetNodes(Container);

        Container.Objects.RemoveAt(index);
        Container.Objects.Insert(targetIndex, curSel);

        // Meshes, dummies and nodes refer to materials and nodes by position, so follow the moved entry
        IndexChanges = ModelIndexRemapper.Remap(Container, materialsBefore, nodesBefore);

        Container.RefreshTypeLists();
        Container.HasUnsavedChanges = true;

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        if (StoredObjectOrder == null)
            return ActionEvent.NoEvent;

        IndexChanges?.Revert();
        IndexChanges = null;

        // Restored into the existing list, as other objects hold on to it
        Container.Objects.Clear();
        Container.Objects.AddRange(StoredObjectOrder);

        StoredObjectOrder = null;

        Container.RefreshTypeLists();

        return ActionEvent.ObjectAddedRemoved;
    }
}
