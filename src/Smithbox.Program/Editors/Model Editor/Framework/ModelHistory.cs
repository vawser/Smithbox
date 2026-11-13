using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// A Model Editor action that can say what it does, for the History tool
/// </summary>
public interface IModelHistoryItem
{
    string Describe();
}

/// <summary>
/// A model switched away from in a view while it had undo history. Its objects (which the history refers to) stay
/// alive, with their drawables out of the scene, until the model is loaded in that view again, unloaded, or dropped
/// to keep within <c>ModelEditor_History_Parked_Limit</c>. See <see cref="ModelEditorView.LoadModel"/>.
/// </summary>
public sealed class ParkedModel
{
    public ModelEditorView View;
    public ModelContainer Container;
    public ViewportActionHistory History;
    public List<Entity> Selection;

    /// <summary>When it was parked, so the oldest is dropped first</summary>
    public long ParkedAt;
}

/// <summary>
/// Names undo steps for the History tool. Plan: Documentation/Model Editor Second Review.md, R5.
/// </summary>
public static class ModelHistory
{
    // Properties that only place an object, so a step changing just these is a transform
    private static readonly HashSet<string> TransformProperties = new()
    {
        nameof(FLVER.Node.Translation), nameof(FLVER.Node.Rotation), nameof(FLVER.Node.Scale),
        nameof(FLVER.Dummy.Position), nameof(FLVER.Dummy.Forward), nameof(FLVER.Dummy.Upward)
    };

    public static string Describe(ViewportAction action)
    {
        return action switch
        {
            null => "",
            ViewportCompoundAction { Label: not null } labelled => labelled.Label,
            IModelHistoryItem item => item.Describe(),
            ViewportCompoundAction compound => DescribeCompound(compound),
            _ => LOC.Get("MODEL_History_Step_Other")
        };
    }

    private static string DescribeCompound(ViewportCompoundAction compound)
    {
        var children = compound.Children.Where(e => e != null).ToList();

        if (children.Count == 0)
            return LOC.Get("MODEL_History_Step_Other");

        if (children.Count == 1)
            return Describe(children[0]);

        // A gizmo move of several objects: one transform per object
        if (children.All(IsTransform))
        {
            var entities = children.SelectMany(GetEntities).Distinct().ToList();

            return LOC.Get("MODEL_History_Transform", NameObjects(entities));
        }

        // Field edits over several objects
        if (children.All(e => e is ModelPropertyChangeAction))
        {
            var props = children.Cast<ModelPropertyChangeAction>().ToList();
            var fields = props.SelectMany(e => e.PropertyNames).Distinct().ToList();
            var entities = props.SelectMany(e => e.Entities).Distinct().ToList();

            var fieldText = fields.Count <= 3 ? string.Join(", ", fields) : LOC.Get("MODEL_History_Fields", fields.Count);

            return LOC.Get("MODEL_History_Set_Field", fieldText, NameObjects(entities));
        }

        return LOC.Get("MODEL_History_Compound", Describe(children[0]), children.Count - 1);
    }

    private static bool IsTransform(ViewportAction action)
    {
        return action switch
        {
            MeshTransformAction => true,
            ModelPropertyChangeAction prop => prop.PropertyNames.Count > 0 && prop.PropertyNames.All(TransformProperties.Contains),
            ViewportCompoundAction { Label: null } compound => compound.Children.Count > 0 && compound.Children.All(IsTransform),
            _ => false
        };
    }

    private static IEnumerable<Entity> GetEntities(ViewportAction action)
    {
        return action switch
        {
            MeshTransformAction mesh => new Entity[] { mesh.Target },
            ModelPropertyChangeAction prop => prop.Entities,
            ViewportCompoundAction compound => compound.Children.SelectMany(GetEntities),
            _ => Enumerable.Empty<Entity>()
        };
    }

    /// <summary>A step changing one property: "Set Translation on R_Hand"</summary>
    public static string DescribePropertyChange(IReadOnlyCollection<string> fields, IReadOnlyCollection<Entity> entities)
    {
        if (fields.Count > 0 && fields.All(TransformProperties.Contains))
            return LOC.Get("MODEL_History_Transform", NameObjects(entities));

        var fieldText = fields.Count switch
        {
            0 => LOC.Get("MODEL_History_Field_Unnamed"),
            <= 3 => string.Join(", ", fields),
            _ => LOC.Get("MODEL_History_Fields", fields.Count)
        };

        return LOC.Get("MODEL_History_Set_Field", fieldText, NameObjects(entities));
    }

    /// <summary>One object by its name, or else how many there are</summary>
    public static string NameObjects(IReadOnlyCollection<Entity> entities)
    {
        if (entities == null || entities.Count == 0)
            return LOC.Get("MODEL_History_Objects", 0);

        if (entities.Count == 1)
            return NameObject(entities.First());

        return LOC.Get("MODEL_History_Objects", entities.Count);
    }

    public static string NameObjects(IEnumerable<Entity> entities)
    {
        return NameObjects(entities?.ToList());
    }

    /// <summary>
    /// A bone or material by its name; a dummy by its reference ID; a mesh by its position (as the Contents list
    /// shows it, which may have moved since)
    /// </summary>
    public static string NameObject(Entity entity)
    {
        switch (entity?.WrappedObject)
        {
            case FLVER.Node node:
                return string.IsNullOrEmpty(node.Name) ? LOC.Get("MODEL_History_Unnamed") : node.Name;

            case FLVER2.Material material:
                return string.IsNullOrEmpty(material.Name) ? LOC.Get("MODEL_History_Unnamed") : material.Name;

            case FLVER.Dummy dummy:
                return LOC.Get("MODEL_History_Dummy", dummy.ReferenceID);

            case FLVER2.Mesh:
                var index = (entity.Container as ModelContainer)?.Meshes.IndexOf(entity) ?? -1;
                return index >= 0 ? LOC.Get("MODEL_History_Mesh", index) : LOC.Get("MODEL_History_Mesh_Removed");

            case null:
                return LOC.Get("MODEL_History_Unnamed");

            default:
                return entity.WrappedObject.GetType().Name;
        }
    }

    /// <summary>Undoes the steps after the given one, and then it</summary>
    public static void UndoTo(ViewportActionManager manager, ViewportAction target)
    {
        if (!manager.GetUndoActions().Contains(target))
            return;

        while (manager.PeekUndoAction() is { } action)
        {
            manager.UndoAction();

            if (action == target)
                break;
        }
    }

    /// <summary>Redoes the steps up to and including the given one</summary>
    public static void RedoTo(ViewportActionManager manager, ViewportAction target)
    {
        if (!manager.GetRedoActions().Contains(target))
            return;

        while (manager.PeekRedoAction() is { } action)
        {
            manager.RedoAction();

            if (action == target)
                break;
        }
    }
}
