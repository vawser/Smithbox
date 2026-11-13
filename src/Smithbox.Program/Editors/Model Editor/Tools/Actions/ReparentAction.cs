using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Builds the actions that move bones and dummies under another bone, for drag and drop in the node hierarchy.
/// <para>
/// The object keeps its place in the model: its transform is worked out again relative to its new parent. Bind
/// poses (and so the skinned meshes drawn from them) are unchanged. Editing a parent index in the properties keeps the
/// local values instead, as that edits the field itself.
/// </para>
/// </summary>
public static class ReparentAction
{
    /// <summary>
    /// Puts a bone under another bone, or at the top of the skeleton for -1. Returns null if the new parent is the
    /// bone itself or one below it.
    /// </summary>
    public static ViewportAction ForNode(ModelEditorView view, ModelContainer container, ModelEntity entity, int newParent)
    {
        if (entity.WrappedObject is not FLVER.Node node)
            return null;

        if (newParent >= 0 && ModelReferencePicker.GetSelfAndDescendants(container, node).Contains(newParent))
            return null;

        if (node.ParentIndex == newParent)
            return null;

        var nodes = ModelIndexRemapper.GetNodes(container);

        // The local transform that keeps the bone where it is under the new parent
        var world = Utils.GetBoneWorldMatrix(node, nodes);
        var parentWorld = newParent >= 0 && newParent < nodes.Count
            ? Utils.GetBoneWorldMatrix(nodes[newParent], nodes)
            : Matrix4x4.Identity;

        var actions = new List<ViewportAction>
        {
            Change(view, entity, nameof(FLVER.Node.ParentIndex), (short)newParent)
        };

        if (Matrix4x4.Invert(parentWorld, out var toParent) &&
            Matrix4x4.Decompose(world * toParent, out var scale, out var rotation, out var translation))
        {
            actions.Add(Change(view, entity, nameof(FLVER.Node.Translation), translation));
            actions.Add(Change(view, entity, nameof(FLVER.Node.Rotation),
                EulerUtils.MatrixToEulerXZY(Matrix4x4.CreateFromQuaternion(rotation))));
            actions.Add(Change(view, entity, nameof(FLVER.Node.Scale), scale));
        }

        return new ViewportCompoundAction(actions)
        {
            Label = LOC.Get("MODEL_History_Reparent", ModelHistory.NameObject(entity), NameParent(container, newParent))
        };
    }

    /// <summary>
    /// Puts a dummy in another bone's space (its parent bone), keeping where it is and which way it faces
    /// </summary>
    public static ViewportAction ForDummyParent(ModelEditorView view, ModelContainer container, ModelEntity entity, int newParent)
    {
        if (entity.WrappedObject is not FLVER.Dummy dummy || dummy.ParentBoneIndex == newParent)
            return null;

        var nodes = ModelIndexRemapper.GetNodes(container);

        Matrix4x4 World(int index) => index >= 0 && index < nodes.Count
            ? Utils.GetBoneWorldMatrix(nodes[index], nodes)
            : Matrix4x4.Identity;

        var actions = new List<ViewportAction>
        {
            Change(view, entity, nameof(FLVER.Dummy.ParentBoneIndex), (short)newParent)
        };

        if (Matrix4x4.Invert(World(newParent), out var toNewParent))
        {
            var move = World(dummy.ParentBoneIndex) * toNewParent;

            actions.Add(Change(view, entity, nameof(FLVER.Dummy.Position), Vector3.Transform(dummy.Position, move)));
            actions.Add(Change(view, entity, nameof(FLVER.Dummy.Forward), Vector3.TransformNormal(dummy.Forward, move)));
            actions.Add(Change(view, entity, nameof(FLVER.Dummy.Upward), Vector3.TransformNormal(dummy.Upward, move)));
        }

        return new ViewportCompoundAction(actions)
        {
            Label = LOC.Get("MODEL_History_Reparent", ModelHistory.NameObject(entity), NameParent(container, newParent))
        };
    }

    // The new parent bone for a history step's name
    private static string NameParent(ModelContainer container, int index)
    {
        return index >= 0 && index < container.Nodes.Count
            ? ModelHistory.NameObject(container.Nodes[index])
            : LOC.Get("MODEL_History_Top_Level");
    }

    /// <summary>
    /// Makes a dummy follow another bone (its attach bone). Where it sits doesn't change.
    /// </summary>
    public static ViewportAction ForDummyAttach(ModelEditorView view, ModelEntity entity, int bone)
    {
        if (entity.WrappedObject is not FLVER.Dummy dummy || dummy.AttachBoneIndex == bone)
            return null;

        return Change(view, entity, nameof(FLVER.Dummy.AttachBoneIndex), (short)bone);
    }

    private static ViewportAction Change(ModelEditorView view, ModelEntity entity, string property, object value)
    {
        PropertyInfo prop = entity.WrappedObject.GetType().GetProperty(property);

        return new ModelPropertyChangeAction(view, prop, new HashSet<Entity> { entity }, value,
            ownerObject: entity.WrappedObject);
    }
}
