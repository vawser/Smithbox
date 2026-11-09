using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>The line layers of the debug shapes; each is drawn in its own colour.</summary>
public enum FxDebugLayer
{
    Shapes,
    Selected,
    AxisX,
    AxisY,
    AxisZ,
    Directions,
    Proxies,
    Particles,
    Lights,
}

/// <summary>
/// Builds the debug wireframes of an effect (FX Editor Plan, phase V1): each node's origin as a small set of axes,
/// and each emitter shape at its node's transform, in world space with the effect at the origin.
///
/// This is a static picture of the effect in one state, at time 0:
/// - Each node uses the config its StateConfigMap (action 199) gives the preview state. A node with no config for
///   that state draws nothing of its own, but its children still draw.
/// - Node transforms come from the NodeTransform slot (StaticNodeTransform/RandomNodeTransform: offset and
///   rotation; variance is left out). The schema says rotations apply Z, then X, then Y, and the offset after them.
/// - Emitter sizes are properties; their value at time 0 is used (a constant, or the first keyframe).
/// - NodeMovement (velocity, acceleration) and Proxy targets are not followed. The runtime (V2) moves nodes.
/// </summary>
public static class FxDebugShapes
{
    public class Result
    {
        public Dictionary<FxDebugLayer, List<(Vector3 A, Vector3 B)>> Lines = new();

        public Vector3 Min = new(float.MaxValue);
        public Vector3 Max = new(float.MinValue);

        public int Nodes;
        public int Shapes;

        public bool HasBounds => Min.X <= Max.X;

        public void Add(FxDebugLayer layer, Vector3 a, Vector3 b)
        {
            if (!Lines.TryGetValue(layer, out var list))
                Lines[layer] = list = new();
            list.Add((a, b));
            Min = Vector3.Min(Min, Vector3.Min(a, b));
            Max = Vector3.Max(Max, Vector3.Max(a, b));
        }
    }

    public static Result Build(FxData data, FxEntry entry, object selectedItem, int state, bool shapes, bool axes)
    {
        var result = new Result();
        if (entry?.Fxr == null)
            return result;

        BuildNode(data, entry, entry.Fxr.Root, Matrix4x4.Identity, selectedItem, state, shapes, axes, result);
        return result;
    }

    private static void BuildNode(FxData data, FxEntry entry, FxrNode node, Matrix4x4 parent, object selectedItem,
        int state, bool shapes, bool axes, Result result)
    {
        result.Nodes++;

        var config = ConfigFor(node, state);
        var actions = config != null ? node.Actions.Concat(config.Actions).ToList() : node.Actions;

        // The node's transform: its own local transform, then the parent's
        var local = Matrix4x4.Identity;
        foreach (var action in actions)
        {
            var view = data.View(entry, action);
            if (view.IsTyped && view.Definition.Slot == "NodeTransform")
                local = NodeTransform(view);
        }
        var world = local * parent;

        bool selected = IsSelected(node, config, selectedItem);
        var shapeLayer = selected ? FxDebugLayer.Selected : FxDebugLayer.Shapes;

        // The root sits at the effect's origin, which the grid already marks
        if (axes && node.Type != 2000)
            AddAxes(result, world, 0.15f);

        // Proxy nodes start another effect here
        if (node.Type == 2001)
            AddCross(result, selected ? FxDebugLayer.Selected : FxDebugLayer.Proxies, world, 0.25f);

        if (shapes && config != null)
        {
            foreach (var action in actions)
            {
                var view = data.View(entry, action);
                if (!view.IsTyped || view.Definition.Slot != "EmitterShape")
                    continue;

                if (AddEmitterShape(result, shapeLayer, view, world))
                    result.Shapes++;
            }
        }

        foreach (var child in node.Children)
            BuildNode(data, entry, child, world, selectedItem, state, shapes, axes, result);
    }

    /// <summary>The config a node uses in a state, by the runtime's rule (<see cref="FxEffectDef.ConfigIndexFor"/>).</summary>
    public static FxrConfig ConfigFor(FxrNode node, int state)
    {
        int index = FxEffectDef.ConfigIndexFor(FxEffectDef.StateConfigMap(node), node.Configs.Count, state);
        return index >= 0 ? node.Configs[index] : null;
    }

    private static bool IsSelected(FxrNode node, FxrConfig config, object item)
    {
        if (item == null)
            return false;
        if (ReferenceEquals(item, node))
            return true;
        if (node.Configs.Any(c => ReferenceEquals(c, item)))
            return true;
        if (item is FxrAction action)
            return node.Actions.Contains(action) || node.Configs.Any(c => c.Actions.Contains(action));
        return false;
    }

    /// <summary>
    /// StaticNodeTransform/RandomNodeTransform: rotation (degrees, Z then X then Y), then the offset, which the
    /// rotation doesn't affect.
    /// </summary>
    private static Matrix4x4 NodeTransform(FxrActionView view)
    {
        var offset = Vec3(view, "offset");
        var rotation = Vec3(view, "rotation") * (MathF.PI / 180f);

        return Matrix4x4.CreateRotationZ(rotation.Z)
            * Matrix4x4.CreateRotationX(rotation.X)
            * Matrix4x4.CreateRotationY(rotation.Y)
            * Matrix4x4.CreateTranslation(offset);
    }

    private static bool AddEmitterShape(Result r, FxDebugLayer layer, FxrActionView view, Matrix4x4 world)
    {
        var name = view.Definition.Name;
        switch (name)
        {
            case "PointEmitterShape":
                AddCross(r, layer, world, 0.1f);
                AddDirection(r, view, world, Vector3.Zero);
                return true;

            case "DiskEmitterShape":
            {
                float radius = Scalar(view, "radius");
                AddCircle(r, layer, world, Vector3.Zero, Vector3.UnitX, Vector3.UnitY, radius);
                AddDirection(r, view, world, Vector3.Zero);
                return true;
            }

            case "RectangleEmitterShape":
            {
                float hx = Scalar(view, "sizeX") / 2, hy = Scalar(view, "sizeY") / 2;
                var c = new[] { new Vector3(-hx, -hy, 0), new Vector3(hx, -hy, 0), new Vector3(hx, hy, 0), new Vector3(-hx, hy, 0) };
                for (int i = 0; i < 4; i++)
                    AddLine(r, layer, world, c[i], c[(i + 1) % 4]);
                AddDirection(r, view, world, Vector3.Zero);
                return true;
            }

            case "SphereEmitterShape":
            {
                float radius = Scalar(view, "radius");
                AddCircle(r, layer, world, Vector3.Zero, Vector3.UnitX, Vector3.UnitY, radius);
                AddCircle(r, layer, world, Vector3.Zero, Vector3.UnitY, Vector3.UnitZ, radius);
                AddCircle(r, layer, world, Vector3.Zero, Vector3.UnitX, Vector3.UnitZ, radius);
                return true;
            }

            case "BoxEmitterShape":
            {
                var h = new Vector3(Scalar(view, "sizeX"), Scalar(view, "sizeY"), Scalar(view, "sizeZ")) / 2;
                AddBox(r, layer, world, -h, h);
                AddDirection(r, view, world, Vector3.Zero);
                return true;
            }

            case "CylinderEmitterShape":
            {
                float radius = Scalar(view, "radius");
                float half = Scalar(view, "height") / 2;
                bool yAxis = Bool(view, "yAxis");

                // Along Z, or Y when yAxis is set
                var axis = yAxis ? Vector3.UnitY : Vector3.UnitZ;
                var u = Vector3.UnitX;
                var v = yAxis ? Vector3.UnitZ : Vector3.UnitY;

                AddCircle(r, layer, world, axis * half, u, v, radius);
                AddCircle(r, layer, world, -axis * half, u, v, radius);
                foreach (var side in new[] { u, -u, v, -v })
                    AddLine(r, layer, world, side * radius + axis * half, side * radius - axis * half);
                AddDirection(r, view, world, Vector3.Zero);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// An arrow for the shape's InitialDirection: Up/Down/North are global, the Local ones follow the node. Emitter
    /// (0) is the shape's own: local north for points, disks and rectangles; boxes and cylinders emit outwards, which
    /// has no single direction, so they get none.
    /// </summary>
    private static void AddDirection(Result r, FxrActionView view, Matrix4x4 world, Vector3 localOrigin)
    {
        if (!view.Names.Contains("direction"))
            return;

        int mode = Int(view, "direction");
        var shape = view.Definition.Name;

        Vector3? local = mode switch
        {
            0 when shape is "PointEmitterShape" or "DiskEmitterShape" or "RectangleEmitterShape" => Vector3.UnitZ,
            4 => Vector3.UnitY,
            5 => -Vector3.UnitY,
            6 => Vector3.UnitZ,
            _ => null,
        };
        Vector3? global = mode switch
        {
            1 => Vector3.UnitY,
            2 => -Vector3.UnitY,
            3 => Vector3.UnitZ,
            _ => null,
        };

        var origin = Vector3.Transform(localOrigin, world);
        Vector3 dir;
        if (local is { } l)
            dir = Vector3.Normalize(Vector3.TransformNormal(l, world));
        else if (global is { } g)
            dir = g;
        else
            return;

        const float length = 0.5f;
        var tip = origin + dir * length;
        r.Add(FxDebugLayer.Directions, origin, tip);

        // Arrow head
        var side = Vector3.Cross(dir, MathF.Abs(dir.Y) > 0.9f ? Vector3.UnitX : Vector3.UnitY);
        side = Vector3.Normalize(side) * 0.06f;
        var back = tip - dir * 0.12f;
        r.Add(FxDebugLayer.Directions, tip, back + side);
        r.Add(FxDebugLayer.Directions, tip, back - side);
    }

    // ---------------------------------------------------------------- values

    /// <summary>A scalar property's value at time 0: its constant, or its first keyframe.</summary>
    public static float Scalar(FxrActionView view, string name)
    {
        try
        {
            var curve = view.GetCurve(name);
            return curve.Function switch
            {
                FxrPropertyFunction.Zero => 0,
                FxrPropertyFunction.One => 1,
                FxrPropertyFunction.Constant => curve.Constant?.FirstOrDefault() ?? 0,
                FxrPropertyFunction.ComponentHermite =>
                    curve.ComponentKeyframes?.FirstOrDefault()?.FirstOrDefault()?.Value?.FirstOrDefault() ?? 0,
                _ => curve.Keyframes.FirstOrDefault()?.Value?.FirstOrDefault() ?? 0,
            };
        }
        catch
        {
            return 0;
        }
    }

    private static Vector3 Vec3(FxrActionView view, string name)
    {
        try
        {
            var v = view.GetValue(name);
            return v.Length >= 3 ? new Vector3((float)v[0], (float)v[1], (float)v[2]) : Vector3.Zero;
        }
        catch
        {
            return Vector3.Zero;
        }
    }

    private static int Int(FxrActionView view, string name)
    {
        try { return (int)view.GetValue(name)[0]; }
        catch { return 0; }
    }

    private static bool Bool(FxrActionView view, string name) => Int(view, name) != 0;

    // ---------------------------------------------------------------- lines

    private static void AddLine(Result r, FxDebugLayer layer, Matrix4x4 world, Vector3 a, Vector3 b)
    {
        r.Add(layer, Vector3.Transform(a, world), Vector3.Transform(b, world));
    }

    private static void AddCircle(Result r, FxDebugLayer layer, Matrix4x4 world, Vector3 center, Vector3 u, Vector3 v,
        float radius, int segments = 32)
    {
        if (radius <= 0)
        {
            // A zero-sized shape still shows where it is
            AddCross(r, layer, Matrix4x4.CreateTranslation(center) * world, 0.05f);
            return;
        }

        var prev = center + u * radius;
        for (int i = 1; i <= segments; i++)
        {
            float a = i * MathF.Tau / segments;
            var next = center + (u * MathF.Cos(a) + v * MathF.Sin(a)) * radius;
            AddLine(r, layer, world, prev, next);
            prev = next;
        }
    }

    private static void AddBox(Result r, FxDebugLayer layer, Matrix4x4 world, Vector3 min, Vector3 max)
    {
        Vector3 C(int i) => new((i & 1) != 0 ? max.X : min.X, (i & 2) != 0 ? max.Y : min.Y, (i & 4) != 0 ? max.Z : min.Z);
        int[,] edges = { { 0, 1 }, { 2, 3 }, { 4, 5 }, { 6, 7 }, { 0, 2 }, { 1, 3 }, { 4, 6 }, { 5, 7 }, { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 } };
        for (int e = 0; e < 12; e++)
            AddLine(r, layer, world, C(edges[e, 0]), C(edges[e, 1]));
    }

    private static void AddCross(Result r, FxDebugLayer layer, Matrix4x4 world, float size)
    {
        AddLine(r, layer, world, -Vector3.UnitX * size, Vector3.UnitX * size);
        AddLine(r, layer, world, -Vector3.UnitY * size, Vector3.UnitY * size);
        AddLine(r, layer, world, -Vector3.UnitZ * size, Vector3.UnitZ * size);
    }

    private static void AddAxes(Result r, Matrix4x4 world, float size)
    {
        var o = world.Translation;
        r.Add(FxDebugLayer.AxisX, o, Vector3.Transform(Vector3.UnitX * size, world));
        r.Add(FxDebugLayer.AxisY, o, Vector3.Transform(Vector3.UnitY * size, world));
        r.Add(FxDebugLayer.AxisZ, o, Vector3.Transform(Vector3.UnitZ * size, world));
    }
}
