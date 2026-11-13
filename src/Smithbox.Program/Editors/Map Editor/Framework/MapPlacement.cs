using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Keeps objects where they are in the world when they move into another map's hierarchy. A map object's position
/// is local to its parents: the map root holds the tile's offset in ER and NR (MapConnections_ER/NR), and the map
/// offset node the MSB's MapOffset in the games that have one. So an object duplicated, moved or pasted into another
/// map keeps its local coordinates and jumps by the difference (256 m between ER tiles) unless it is re-localised.
/// </summary>
public static class MapPlacement
{
    /// <summary>
    /// The parent an object goes under when it is put in a map, as MapContainer.LoadMSB places it: MSB parts, regions
    /// and events under the map root; anything else (DS2 generator locations, and lights' BTL parents, which callers
    /// pick themselves) under the map offset node, which holds the MSB's MapOffset.
    /// </summary>
    public static Entity GetDefaultParent(MapContainer map, Entity ent)
    {
        if (ent?.WrappedObject is IMsbPart or IMsbRegion or IMsbEvent)
            return map.RootObject;

        return map.MapOffsetNode ?? map.RootObject;
    }

    /// <summary>
    /// The world matrix of the entity's parents, built as Entity.GetWorldMatrix builds it (what the viewport draws)
    /// </summary>
    public static Matrix4x4 GetParentWorld(Entity ent)
    {
        var m = Matrix4x4.Identity;

        for (var p = ent?.Parent; p != null; p = p.Parent)
        {
            if (p.HasTransform)
            {
                m *= p.GetLocalTransform().WorldMatrix;
            }
        }

        return m;
    }

    /// <summary>
    /// The action that moves each entity, now under its new parents, back to where it was in the world under its
    /// old ones. Null if none of them changed parent frames. Built after the entities have been placed (as the step
    /// of a DeferredAction), and undone with the placement.
    /// </summary>
    public static ViewportAction BuildKeepWorldAction(IEnumerable<(Entity Entity, Matrix4x4 OldParentWorld)> moves)
    {
        var actions = new List<ViewportAction>();

        foreach (var (ent, oldParentWorld) in moves)
        {
            if (ent == null || !ent.HasTransform || ent.WrappedObject == null)
                continue;

            var newParentWorld = GetParentWorld(ent);

            if (NearlyEqual(oldParentWorld, newParentWorld))
                continue;

            if (!Matrix4x4.Invert(newParentWorld, out var newParentInverse))
                continue;

            var local = ent.GetLocalTransform();

            // The object's own position and rotation; its scale (and a region's shape size) stays as it is
            var localRigid = Matrix4x4.CreateFromQuaternion(local.Rotation) * Matrix4x4.CreateTranslation(local.Position);
            var newLocal = localRigid * oldParentWorld * newParentInverse;

            if (!Matrix4x4.Decompose(newLocal, out _, out var rotation, out var position))
                continue;

            var action = SameRotation(oldParentWorld, newParentWorld)
                ? BuildPositionAction(ent, position)
                : null;

            // A rotated frame (a DS3 map offset turns on Y), or an object placed by something other than its Position
            // property: the whole transform is written
            action ??= ent.GetUpdateTransformAction(new Transform(position, rotation, Vector3.One));

            if (action != null)
            {
                actions.Add(action);
            }
        }

        return actions.Count > 0 ? new ViewportCompoundAction(actions) : null;
    }

    /// <summary>
    /// Writes only the Position property, so the rotation values aren't rewritten through a quaternion (which can turn
    /// 90 into 89.99999). Null where an object isn't placed by a Vector3 Position (DS2 param rows, map limit edges).
    /// </summary>
    private static ViewportAction BuildPositionAction(Entity ent, Vector3 position)
    {
        if (ent is MsbEntity { Type: MsbEntityType.MapLimitEdge })
            return null;

        var prop = ent.WrappedObject.GetType().GetProperty("Position");

        if (prop == null || prop.PropertyType != typeof(Vector3) || !prop.CanWrite)
            return null;

        var action = new PropChangeAction(ent, ent.WrappedObject);
        action.AddPropertyChange(prop, position);
        action.SetPostExecutionAction(_ => ent.UpdateRenderModel());

        return action;
    }

    private static bool SameRotation(Matrix4x4 a, Matrix4x4 b)
    {
        if (!Matrix4x4.Decompose(a, out _, out var ra, out _) || !Matrix4x4.Decompose(b, out _, out var rb, out _))
            return false;

        // q and -q are the same rotation
        return MathF.Abs(Quaternion.Dot(ra, rb)) > 0.99999f;
    }

    private static bool NearlyEqual(Matrix4x4 a, Matrix4x4 b)
    {
        const float epsilon = 0.0001f;

        return MathF.Abs(a.M11 - b.M11) < epsilon && MathF.Abs(a.M12 - b.M12) < epsilon && MathF.Abs(a.M13 - b.M13) < epsilon &&
               MathF.Abs(a.M21 - b.M21) < epsilon && MathF.Abs(a.M22 - b.M22) < epsilon && MathF.Abs(a.M23 - b.M23) < epsilon &&
               MathF.Abs(a.M31 - b.M31) < epsilon && MathF.Abs(a.M32 - b.M32) < epsilon && MathF.Abs(a.M33 - b.M33) < epsilon &&
               MathF.Abs(a.M41 - b.M41) < epsilon && MathF.Abs(a.M42 - b.M42) < epsilon && MathF.Abs(a.M43 - b.M43) < epsilon;
    }
}
