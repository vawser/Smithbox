using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A Map Editor action that can say what it does, for the History tool. Where it happened (the maps) comes from
/// the action's TouchedScopes, so Describe only says what.
/// </summary>
public interface IMapHistoryItem
{
    string Describe();
}

/// <summary>
/// Names undo steps and walks the history of a view: undo to a step, redo to a step, and how unloading a map
/// shortens it. Plan: Documentation/Map Editor Review - Third Pass.md, R5.
/// </summary>
public static class MapHistory
{
    // Properties that only place an object, so a step changing just these is a transform
    private static readonly HashSet<string> TransformProperties = new()
    {
        "Position", "Rotation", "Scale",
        "PositionX", "PositionY", "PositionZ", "RotationX", "RotationY", "RotationZ"
    };

    /// <summary>
    /// What a step does, e.g. "Delete 3 objects" or "Rename c1000_9000 to c1000_9001"
    /// </summary>
    public static string Describe(ViewportAction action)
    {
        switch (action)
        {
            case null:
                return "";

            case IMapHistoryItem item:
                return item.Describe();

            case ViewportCompoundAction compound:
                return DescribeCompound(compound);

            default:
                return LOC.Get("MAP_History_Step_Other");
        }
    }

    private static string DescribeCompound(ViewportCompoundAction compound)
    {
        if (compound.Label != null)
            return compound.Label;

        // Follow-ups (such as keeping world positions after a move) are part of the step before them
        var children = compound.Children
            .Where(e => e != null && e is not DeferredAction)
            .ToList();

        if (children.Count == 0)
            return LOC.Get("MAP_History_Step_Other");

        if (children.Count == 1)
            return Describe(children[0]);

        // A gizmo move, rotate or scale: one transform change per object
        var entities = new List<Entity>();

        if (children.All(e => IsTransformOf(e, entities)))
            return LOC.Get("MAP_History_Transform", NameObjects(entities));

        // Field edits over several objects, as a mass edit or a property action makes
        if (children.All(e => e is PropChangeAction or PropMultChangeAction))
        {
            var fields = children.OfType<PropChangeAction>().SelectMany(e => e.PropertyNames)
                .Concat(children.OfType<PropMultChangeAction>().Select(e => e.PropertyName))
                .Distinct()
                .ToList();

            var edited = children.OfType<PropChangeAction>().Select(e => e.TargetEntity)
                .Concat(children.OfType<PropMultChangeAction>().SelectMany(e => e.Entities))
                .Where(e => e != null)
                .Distinct()
                .ToList();

            var fieldText = fields.Count <= 3 ? string.Join(", ", fields) : LOC.Get("MAP_History_Fields", fields.Count);

            return LOC.Get("MAP_History_Set_Field", fieldText, NameObjects(edited));
        }

        var first = Describe(children[0]);

        return LOC.Get("MAP_History_Compound", first, children.Count - 1);
    }

    /// <summary>
    /// Whether the action only changes where one object is (a PropChangeAction of transform fields, or a compound
    /// of those for one DS2 param row), adding that object to the list
    /// </summary>
    private static bool IsTransformOf(ViewportAction action, List<Entity> entities)
    {
        if (action is PropChangeAction prop)
        {
            if (prop.TargetEntity == null || prop.PropertyNames.Count == 0 || !prop.PropertyNames.All(TransformProperties.Contains))
                return false;

            if (!entities.Contains(prop.TargetEntity))
                entities.Add(prop.TargetEntity);

            return true;
        }

        if (action is ViewportCompoundAction compound && compound.Label == null && compound.Children.Count > 0)
        {
            return compound.Children.All(e => IsTransformOf(e, entities));
        }

        return false;
    }

    /// <summary>
    /// One object by its name, or else how many there are
    /// </summary>
    public static string NameObjects(IReadOnlyCollection<Entity> entities)
    {
        if (entities == null || entities.Count == 0)
            return LOC.Get("MAP_History_Objects", 0);

        if (entities.Count == 1)
            return NameObject(entities.First());

        return LOC.Get("MAP_History_Objects", entities.Count);
    }

    public static string NameObject(Entity entity)
    {
        var name = entity?.Name;

        return string.IsNullOrEmpty(name) ? LOC.Get("MAP_History_Unnamed") : name;
    }

    /// <summary>
    /// Undoes the steps after the given one, and then it, so the history is back to before it was made
    /// </summary>
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

    /// <summary>
    /// Redoes the steps up to and including the given one
    /// </summary>
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
