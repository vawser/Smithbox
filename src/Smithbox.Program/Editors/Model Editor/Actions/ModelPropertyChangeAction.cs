using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.ModelEditor;

public class ModelPropertyChangeAction : ViewportAction, IModelHistoryItem
{
    /// <summary>The fields this step changes, for the History tool. A nested list element has no field name.</summary>
    public IReadOnlyList<string> PropertyNames =>
        Changes.Where(c => c.Property != null).Select(c => c.Property.Name).Distinct().ToList();

    /// <summary>The objects this step changes</summary>
    public IReadOnlyList<Entity> Entities =>
        Changes.Select(c => c.Entity).Where(e => e != null).Distinct().ToList();

    public string Describe()
    {
        return ModelHistory.DescribePropertyChange(PropertyNames, Entities);
    }

    private ModelEditorView View;

    private readonly HashSet<Entity> ChangedEnts = new();
    private readonly List<PropertyChange> Changes = new();

    public bool UpdateRenderModel = false;
    public bool ClearName { get; set; }

    // The node links rewritten and the entities moved when a parent index changed, undone in reverse
    private IndexChangeRecord HierarchyChanges;

    /// <param name="ownerObject">The object holding the property, when known. Searching for it by index from the
    /// entity only keeps the innermost index, so it can find the wrong object in deeply nested data.</param>
    public ModelPropertyChangeAction(ModelEditorView view, PropertyInfo prop, HashSet<Entity> changedEnts, object newval,
        int index = -1, int classIndex = -1, bool clearName = true, object ownerObject = null)
    {
        View = view;

        ClearName = clearName;
        ChangedEnts = changedEnts;

        // The owner drawn in the property view is exact, but only applies to the single entity it was drawn from
        var useOwner = ownerObject != null && changedEnts.Count == 1 &&
            prop.DeclaringType != null && prop.DeclaringType.IsInstanceOfType(ownerObject);

        foreach (Entity o in changedEnts)
        {
            var propObj = useOwner
                ? ownerObject
                : PropFinderUtil.FindPropertyObject(prop, o.WrappedObject, index, classIndex, false);
            if (propObj != null)
            {
                var change = new PropertyChange
                {
                    Entity = o,
                    ChangedObj = propObj,
                    Property = prop,
                    NewValue = newval,
                    ArrayIndex = index
                };

                var value = change.Property.GetValue(propObj);

                Type valType = null;

                if (value != null)
                {
                    valType = value.GetType();
                }

                if (index != -1 && prop.PropertyType.IsArray)
                {
                    var a = (Array)value;
                    change.OldValue = a.GetValue(index);
                }
                else if (value != null && valType != null && valType.IsGenericType && index != -1 && value is IList list)
                {
                    change.OldValue = list[index];
                }
                else
                {
                    change.OldValue = prop.GetValue(propObj);
                }

                Changes.Add(change);
            }
        }
    }

    /// <summary>
    /// Changes an element of a list nested within another list, which has no property of its own
    /// </summary>
    public ModelPropertyChangeAction(Entity ent, IList list, int index, object oldval, object newval, bool clearName = true)
    {
        ClearName = clearName;
        if (ent != null)
        {
            ChangedEnts = new HashSet<Entity> { ent };
        }

        var change = new PropertyChange
        {
            Entity = ent,
            DirectList = list,
            ArrayIndex = index,
            OldValue = oldval,
            NewValue = newval
        };

        Changes.Add(change);
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (PropertyChange change in Changes)
        {
            Apply(change, change.NewValue);
        }

        HierarchyChanges = new IndexChangeRecord();

        foreach (PropertyChange change in Changes)
        {
            ApplyParentChange(change, HierarchyChanges);
        }

        RefreshEntities();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        HierarchyChanges?.Revert();
        HierarchyChanges = null;

        foreach (PropertyChange change in Changes)
        {
            Apply(change, change.OldValue);
        }

        RefreshEntities();

        return ActionEvent.NoEvent;
    }

    private static void Apply(PropertyChange change, object value)
    {
        if (change.DirectList != null)
        {
            change.DirectList[change.ArrayIndex] = value;
            return;
        }

        var current = change.Property.GetValue(change.ChangedObj);

        if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
        {
            ((Array)current).SetValue(value, change.ArrayIndex);
        }
        else if (current != null && current.GetType().IsGenericType && change.ArrayIndex != -1 && current is IList list)
        {
            list[change.ArrayIndex] = value;
        }
        else
        {
            change.Property.SetValue(change.ChangedObj, value);
        }
    }

    /// <summary>
    /// A dummy's parent bone and a node's parent decide where they sit in the tree and which bone they move with,
    /// so the entity is moved under its new parent. A node is also moved between the sibling chains of the FLVER.
    /// </summary>
    private static void ApplyParentChange(PropertyChange change, IndexChangeRecord record)
    {
        var entity = change.Entity;

        if (entity?.Container is not ModelContainer container || change.ChangedObj != entity.WrappedObject)
            return;

        if (Equals(change.OldValue, change.NewValue))
            return;

        int newParent;

        if (entity.WrappedObject is FLVER.Dummy dummy && change.Property?.Name is
            nameof(FLVER.Dummy.ParentBoneIndex) or nameof(FLVER.Dummy.AttachBoneIndex))
        {
            var newBone = Convert.ToInt32(change.NewValue);
            var nodes = ModelIndexRemapper.GetNodes(container);

            // In the games every bone a dummy names is marked as a dummy owner
            if (newBone >= 0 && newBone < nodes.Count)
            {
                ModelIndexRemapper.MarkDummyOwner(nodes[newBone], record);
            }

            // ... and a dummy follows its attach bone exactly when it has one
            if (change.Property.Name == nameof(FLVER.Dummy.AttachBoneIndex))
            {
                var oldFollow = dummy.FollowAttachBone;
                dummy.FollowAttachBone = newBone != -1;
                record.Record(() => dummy.FollowAttachBone = oldFollow);

                // The attach bone doesn't decide where the dummy sits in the tree
                return;
            }

            newParent = newBone;
        }
        else if (entity.WrappedObject is FLVER.Node node && change.Property?.Name == nameof(FLVER.Node.ParentIndex))
        {
            var nodes = ModelIndexRemapper.GetNodes(container);
            var index = container.Nodes.IndexOf(entity);

            newParent = Convert.ToInt32(change.NewValue);

            // A parent that would loop the tree, or doesn't exist, is refused: the value goes back, so the saved
            // model never holds a loop. Undo puts back the old value anyway.
            if (!ModelIndexRemapper.RelinkNode(nodes, index, Convert.ToInt32(change.OldValue), newParent, record))
            {
                node.ParentIndex = (short)Convert.ToInt32(change.OldValue);
                Smithbox.LogError(typeof(ModelPropertyChangeAction), LOC.Get("MODEL_Properties_Parent_Refused", node.Name, newParent));
                return;
            }
        }
        else
        {
            return;
        }

        var newParentEntity = newParent >= 0 && newParent < container.Nodes.Count
            ? container.Nodes[newParent]
            : container.RootObject;

        var oldParentEntity = entity.Parent;

        if (oldParentEntity == newParentEntity)
            return;

        var oldChildIndex = oldParentEntity?.ChildIndex(entity) ?? -1;

        oldParentEntity?.RemoveChild(entity);
        newParentEntity.AddChild(entity);

        record.Record(() =>
        {
            newParentEntity.RemoveChild(entity);

            if (oldParentEntity != null)
            {
                oldParentEntity.AddChild(entity, oldChildIndex);
            }
        });
    }

    private void RefreshEntities()
    {
        if (ChangedEnts == null)
            return;

        foreach (Entity e in ChangedEnts)
        {
            // Bones and dummies place their markers (and those of their children) from their properties
            if (UpdateRenderModel || e.HasTransform)
            {
                e.UpdateRenderModel();
            }

            // Clear name cache, forcing it to update.
            if (ClearName)
                e.Name = null;
        }
    }

    private class PropertyChange
    {
        public Entity Entity;
        public int ArrayIndex;
        public object ChangedObj;
        public object NewValue;
        public object OldValue;
        public PropertyInfo Property;
        public IList DirectList;
    }
}
