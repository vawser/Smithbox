#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Utilities.Fbx;

/// <summary>A Properties70 value: its numbers (most properties) or its string</summary>
public sealed class FbxPropertyValue
{
    public string Type = "";
    public double[] Numbers = Array.Empty<double>();
    public string? Text;

    public double Number(int i = 0, double fallback = 0) => i < Numbers.Length ? Numbers[i] : fallback;

    public Vector3 Vector(Vector3 fallback) => Numbers.Length >= 3
        ? new Vector3((float)Numbers[0], (float)Numbers[1], (float)Numbers[2])
        : fallback;
}

/// <summary>
///     One animation curve: keys at FBX times with constant, linear or cubic (Hermite, unweighted) interpolation
///     between them, held before the first and after the last.
/// </summary>
public sealed class FbxCurve
{
    private const int InterpolationConstant = 0x2;
    private const int InterpolationLinear = 0x4;
    private const int InterpolationCubic = 0x8;
    private const int ConstantNext = 0x100;

    public long[] Times = Array.Empty<long>();
    public float[] Values = Array.Empty<float>();
    public double Default;

    /// <summary>Per key: the KeyAttrFlags of its attribute, its right slope and the next key's left slope (per second)</summary>
    public int[] Flags = Array.Empty<int>();
    public float[] RightSlope = Array.Empty<float>();
    public float[] NextLeftSlope = Array.Empty<float>();

    public int KeyCount => Times.Length;

    public double Evaluate(long time)
    {
        var n = Times.Length;
        if (n == 0)
            return Default;
        if (time <= Times[0])
            return Values[0];
        if (time >= Times[n - 1])
            return Values[n - 1];

        var i = Array.BinarySearch(Times, time);
        if (i >= 0)
            return Values[i];
        i = ~i - 1;

        var flags = i < Flags.Length ? Flags[i] : InterpolationLinear;
        double v0 = Values[i], v1 = Values[i + 1];
        if ((flags & InterpolationConstant) != 0)
            return (flags & ConstantNext) != 0 ? v1 : v0;

        var span = (double)(Times[i + 1] - Times[i]);
        var t = (time - Times[i]) / span;
        if ((flags & InterpolationCubic) == 0)
            return v0 + (v1 - v0) * t;

        // Hermite with the slopes in value per second
        var seconds = span / FbxAnimationExport.TicksPerSecond;
        var m0 = (i < RightSlope.Length ? RightSlope[i] : 0) * seconds;
        var m1 = (i < NextLeftSlope.Length ? NextLeftSlope[i] : 0) * seconds;
        var t2 = t * t;
        var t3 = t2 * t;
        return (2 * t3 - 3 * t2 + 1) * v0 + (t3 - 2 * t2 + t) * m0 + (-2 * t3 + 3 * t2) * v1 + (t3 - t2) * m1;
    }
}

/// <summary>An AnimationCurveNode: up to three curves (d|X, d|Y, d|Z) driving one property of one model</summary>
public sealed class FbxCurveNode
{
    public long Id;
    public string Property = "";
    public FbxModel? Model;
    public readonly FbxCurve?[] Curves = new FbxCurve?[3];
    public readonly double[] Defaults = new double[3];
    public readonly bool[] HasDefault = new bool[3];
    public readonly List<long> Layers = new();
}

/// <summary>An AnimationStack (a take): its time span and its layers, base layer first</summary>
public sealed class FbxAnimationStack
{
    public long Id;
    public string Name = "";
    public long Start;
    public long Stop;
    public readonly List<long> Layers = new();

    /// <summary>The curve nodes of the base layer, by model and property</summary>
    public readonly Dictionary<(long Model, string Property), FbxCurveNode> Base = new();

    public int CurveNodesOnOtherLayers;
}

/// <summary>A Model node (LimbNode, Null, Mesh...) with its properties (its own over the template's)</summary>
public sealed class FbxModel
{
    public long Id;
    public string Name = "";
    public string Type = "";
    public FbxModel? Parent;
    public readonly List<FbxModel> Children = new();
    public readonly Dictionary<string, FbxPropertyValue> Properties = new();

    public FbxPropertyValue? Property(string name) => Properties.GetValueOrDefault(name);

    public Vector3 Vector(string name, Vector3 fallback) => Property(name)?.Vector(fallback) ?? fallback;

    public double Number(string name, double fallback) => Property(name)?.Number(0, fallback) ?? fallback;

    public override string ToString() => $"{Name} ({Type})";
}

/// <summary>
///     The scene meaning of an FBX 7.x document: models with their parents and properties, animation stacks with their
///     curves, axis and unit settings, the frame rate and bind pose. <see cref="Local" /> evaluates a model's full FBX
///     transform at a time; <see cref="World" /> up its parents. Matrices are System.Numerics (row vectors).
///     See Claude/Documentation/Animation Editor Analysis.md §14.3.
/// </summary>
public sealed class FbxScene
{
    public int Version;
    public string Creator = "";
    public string Comment = "";

    // GlobalSettings
    public int UpAxis = 1, UpAxisSign = 1, FrontAxis = 2, FrontAxisSign = 1, CoordAxis = 0, CoordAxisSign = 1;

    /// <summary>Centimetres per file unit (FBX's default unit is the centimetre)</summary>
    public double UnitScaleFactor = 1;

    public double FrameRate = 30;

    public readonly Dictionary<long, FbxModel> Models = new();
    public readonly List<FbxAnimationStack> Stacks = new();

    /// <summary>World matrices of the bind pose (Pose BindPose, then Cluster TransformLink), by model id</summary>
    public readonly Dictionary<long, Matrix4x4> BindPose = new();

    /// <summary>Models with InheritType other than RSrs (1), which are evaluated as RSrs</summary>
    public int OtherInheritTypes;

    public IEnumerable<FbxModel> Roots => Models.Values.Where(m => m.Parent == null);

    // ------------------------------------------------------------------ reading

    public static FbxScene Read(FbxDocument doc)
    {
        if (doc.Version < 7000)
            throw new InvalidDataException(
                $"FBX {doc.Version / 1000}.{doc.Version / 100 % 10} files aren't supported: save the file as FBX 7 (2011 or later)");

        var scene = new FbxScene { Version = doc.Version };
        scene.Creator = doc.Find("Creator")?.Prop(0)?.AsString() ??
                        doc.Find("FBXHeaderExtension")?.Child("Creator")?.Prop(0)?.AsString() ?? "";
        scene.Comment = doc.Find("FBXHeaderExtension")?.Child("SceneInfo")?.Child("MetaData")?.Child("Comment")
            ?.Prop(0)?.AsString() ?? "";

        scene.ReadGlobalSettings(doc.Find("GlobalSettings"));

        var templates = ReadTemplates(doc.Find("Definitions"));
        var objects = doc.Find("Objects") ?? throw new InvalidDataException("FBX file has no Objects");

        var curveNodes = new Dictionary<long, FbxCurveNode>();
        var curves = new Dictionary<long, FbxCurve>();
        var layers = new HashSet<long>();
        var clusters = new Dictionary<long, Matrix4x4>();

        foreach (var o in objects.Children)
        {
            if (o.Prop(0) is not { IsNumber: true } idProp)
                continue;
            var id = idProp.AsLong();

            switch (o.Name)
            {
                case "Model":
                {
                    var model = new FbxModel
                    {
                        Id = id,
                        Name = o.Prop(1)?.AsObjectName().Name ?? "",
                        Type = o.Prop(2)?.AsString() ?? "",
                    };
                    if (templates.TryGetValue(("Model", "FbxNode"), out var template))
                    {
                        foreach (var (k, v) in template)
                            model.Properties[k] = v;
                    }

                    foreach (var (k, v) in ReadProperties(o))
                        model.Properties[k] = v;
                    scene.Models[id] = model;
                    break;
                }
                case "AnimationStack":
                {
                    var props = ReadProperties(o);
                    var stack = new FbxAnimationStack { Id = id, Name = o.Prop(1)?.AsObjectName().Name ?? "" };
                    stack.Start = (long)(props.GetValueOrDefault("LocalStart")?.Number() ?? 0);
                    stack.Stop = (long)(props.GetValueOrDefault("LocalStop")?.Number() ?? 0);
                    scene.Stacks.Add(stack);
                    break;
                }
                case "AnimationLayer":
                    layers.Add(id);
                    break;
                case "AnimationCurveNode":
                {
                    var node = new FbxCurveNode { Id = id };
                    foreach (var (k, v) in ReadProperties(o))
                    {
                        var axis = k switch { "d|X" => 0, "d|Y" => 1, "d|Z" => 2, _ => -1 };
                        if (axis >= 0 && v.Numbers.Length > 0)
                        {
                            node.Defaults[axis] = v.Numbers[0];
                            node.HasDefault[axis] = true;
                        }
                    }

                    curveNodes[id] = node;
                    break;
                }
                case "AnimationCurve":
                    curves[id] = ReadCurve(o);
                    break;
                case "Pose" when o.Child("Type")?.Prop(0)?.AsString() == "BindPose":
                    foreach (var pn in o.ChildrenNamed("PoseNode"))
                    {
                        if (pn.Child("Node")?.Prop(0) is { } node && pn.Child("Matrix") is { } m)
                            scene.BindPose[node.AsLong()] = ToMatrix(m.Doubles());
                    }

                    break;
                case "Deformer" when o.Prop(2)?.AsString() == "Cluster" && o.Child("TransformLink") is { } link:
                    clusters[id] = ToMatrix(link.Doubles());
                    break;
            }
        }

        // Connections
        foreach (var c in doc.Find("Connections")?.ChildrenNamed("C") ?? Enumerable.Empty<FbxNode>())
        {
            var kind = c.Prop(0)?.AsString();
            if (c.Prop(1) is not { IsNumber: true } childProp || c.Prop(2) is not { IsNumber: true } parentProp)
                continue;
            var child = childProp.AsLong();
            var parent = parentProp.AsLong();
            var property = c.Prop(3)?.AsString() ?? "";

            if (scene.Models.TryGetValue(child, out var model))
            {
                if (kind == "OO" && scene.Models.TryGetValue(parent, out var p) && model.Parent == null && p != model)
                {
                    model.Parent = p;
                    p.Children.Add(model);
                }
            }
            else if (curves.TryGetValue(child, out var curve) && curveNodes.TryGetValue(parent, out var cn))
            {
                var axis = property switch { "d|X" => 0, "d|Y" => 1, "d|Z" => 2, _ => -1 };
                if (axis >= 0)
                    cn.Curves[axis] = curve;
            }
            else if (curveNodes.TryGetValue(child, out var node))
            {
                if (kind == "OP" && scene.Models.TryGetValue(parent, out var target))
                {
                    node.Model = target;
                    node.Property = property;
                }
                else if (layers.Contains(parent))
                {
                    node.Layers.Add(parent);
                }
            }
            else if (layers.Contains(child) && scene.Stacks.Find(s => s.Id == parent) is { } stack)
            {
                stack.Layers.Add(child);
            }
        }

        // Bones linked to skin clusters (the bone is the child of its cluster): the cluster's TransformLink is the
        // bone's bind pose when the file has no BindPose entry for it
        foreach (var c in doc.Find("Connections")?.ChildrenNamed("C") ?? Enumerable.Empty<FbxNode>())
        {
            if (c.Prop(1)?.IsNumber == true && c.Prop(2)?.IsNumber == true &&
                scene.Models.ContainsKey(c.Prop(1)!.AsLong()) && clusters.TryGetValue(c.Prop(2)!.AsLong(), out var link))
                scene.BindPose.TryAdd(c.Prop(1)!.AsLong(), link);
        }

        // The base layer's curve nodes per stack
        foreach (var stack in scene.Stacks)
        {
            if (stack.Layers.Count == 0)
                continue;
            var baseLayer = stack.Layers[0];
            foreach (var node in curveNodes.Values)
            {
                if (node.Model == null)
                    continue;
                if (node.Layers.Contains(baseLayer))
                    stack.Base[(node.Model.Id, node.Property)] = node;
                else if (node.Layers.Any(stack.Layers.Contains))
                    stack.CurveNodesOnOtherLayers++;
            }

            // A stack without a time span: its keys' span
            if (stack.Stop <= stack.Start)
            {
                var times = stack.Base.Values.SelectMany(n => n.Curves).Where(c => c is { KeyCount: > 0 })
                    .SelectMany(c => new[] { c!.Times[0], c.Times[^1] }).ToList();
                if (times.Count > 0)
                {
                    stack.Start = times.Min();
                    stack.Stop = times.Max();
                }
            }
        }

        scene.OtherInheritTypes = scene.Models.Values.Count(m => (int)m.Number("InheritType", 1) != 1);
        return scene;
    }

    private void ReadGlobalSettings(FbxNode? settings)
    {
        if (settings == null)
            return;

        var props = ReadProperties(settings);
        int I(string name, int fallback) => (int)(props.GetValueOrDefault(name)?.Number(0, fallback) ?? fallback);

        UpAxis = I("UpAxis", 1);
        UpAxisSign = I("UpAxisSign", 1);
        FrontAxis = I("FrontAxis", 2);
        FrontAxisSign = I("FrontAxisSign", 1);
        CoordAxis = I("CoordAxis", 0);
        CoordAxisSign = I("CoordAxisSign", 1);
        UnitScaleFactor = props.GetValueOrDefault("UnitScaleFactor")?.Number(0, 1) ?? 1;
        if (UnitScaleFactor <= 0)
            UnitScaleFactor = 1;

        var custom = props.GetValueOrDefault("CustomFrameRate")?.Number(0, 0) ?? 0;
        FrameRate = I("TimeMode", 0) switch
        {
            1 => 120,
            2 => 100,
            3 => 60,
            4 => 50,
            5 => 48,
            6 or 7 => 30,
            8 or 9 => 29.97,
            10 => 25,
            11 or 13 => 24,
            12 => 1000,
            15 => 96,
            16 => 72,
            17 => 59.94,
            _ => custom > 0 ? custom : 30,
        };
    }

    private static Dictionary<(string Type, string Template), Dictionary<string, FbxPropertyValue>> ReadTemplates(
        FbxNode? definitions)
    {
        var result = new Dictionary<(string, string), Dictionary<string, FbxPropertyValue>>();
        foreach (var type in definitions?.ChildrenNamed("ObjectType") ?? Enumerable.Empty<FbxNode>())
        {
            foreach (var template in type.ChildrenNamed("PropertyTemplate"))
                result[(type.Prop(0)?.AsString() ?? "", template.Prop(0)?.AsString() ?? "")] = ReadProperties(template);
        }

        return result;
    }

    /// <summary>A node's Properties70 (7.x) or Properties60 (P: name, type, label, flags, values...)</summary>
    public static Dictionary<string, FbxPropertyValue> ReadProperties(FbxNode node)
    {
        var result = new Dictionary<string, FbxPropertyValue>();
        var props = node.Child("Properties70");
        if (props == null)
            return result;

        foreach (var p in props.ChildrenNamed("P"))
        {
            if (p.Properties.Count < 4)
                continue;
            var values = p.Properties.Skip(4).ToList();
            result[p.Properties[0].AsString()] = new FbxPropertyValue
            {
                Type = p.Properties[1].AsString(),
                Numbers = values.Where(v => v.IsNumber).Select(v => v.AsDouble()).ToArray(),
                Text = values.FirstOrDefault(v => v.Type == FbxPropertyType.String)?.AsString(),
            };
        }

        return result;
    }

    private static FbxCurve ReadCurve(FbxNode o)
    {
        var curve = new FbxCurve
        {
            Times = o.Child("KeyTime")?.Longs() ?? Array.Empty<long>(),
            Values = (o.Child("KeyValueFloat") ?? o.Child("KeyValueDouble"))?.Doubles().Select(v => (float)v).ToArray()
                     ?? Array.Empty<float>(),
            Default = o.Child("Default")?.Prop(0)?.AsDouble() ?? 0,
        };
        if (curve.Values.Length != curve.Times.Length)
        {
            var n = Math.Min(curve.Values.Length, curve.Times.Length);
            curve.Times = curve.Times[..n];
            curve.Values = curve.Values[..n];
        }

        // Per-key attributes: each attribute (flags + 4 data floats) covers RefCount keys in a row
        var flags = o.Child("KeyAttrFlags")?.Longs() ?? Array.Empty<long>();
        var data = o.Child("KeyAttrDataFloat")?.Doubles() ?? Array.Empty<double>();
        var refs = o.Child("KeyAttrRefCount")?.Longs() ?? Array.Empty<long>();
        var count = curve.Times.Length;
        curve.Flags = new int[count];
        curve.RightSlope = new float[count];
        curve.NextLeftSlope = new float[count];
        var key = 0;
        for (var a = 0; a < flags.Length && key < count; a++)
        {
            var span = a < refs.Length ? refs[a] : count - key;
            for (var k = 0; k < span && key < count; k++, key++)
            {
                curve.Flags[key] = (int)flags[a];
                curve.RightSlope[key] = 4 * a < data.Length ? (float)data[4 * a] : 0;
                curve.NextLeftSlope[key] = 4 * a + 1 < data.Length ? (float)data[4 * a + 1] : 0;
            }
        }

        for (; key < count; key++)
            curve.Flags[key] = 0x4;
        return curve;
    }

    /// <summary>16 doubles in FbxAMatrix memory order (the System.Numerics field order)</summary>
    private static Matrix4x4 ToMatrix(double[] a) => a.Length < 16
        ? Matrix4x4.Identity
        : new Matrix4x4((float)a[0], (float)a[1], (float)a[2], (float)a[3], (float)a[4], (float)a[5], (float)a[6],
            (float)a[7], (float)a[8], (float)a[9], (float)a[10], (float)a[11], (float)a[12], (float)a[13],
            (float)a[14], (float)a[15]);

    // ------------------------------------------------------------------ evaluation

    /// <summary>A property at a time: the stack's curves where they exist, else the curve node's default, else the model's value</summary>
    public Vector3 Animated(FbxModel model, string property, Vector3 value, FbxAnimationStack? stack, long time)
    {
        if (stack == null || !stack.Base.TryGetValue((model.Id, property), out var node))
            return value;

        float Axis(int i, float fallback)
        {
            if (node.Curves[i] is { KeyCount: > 0 } c)
                return (float)c.Evaluate(time);
            return node.HasDefault[i] ? (float)node.Defaults[i] : fallback;
        }

        return new Vector3(Axis(0, value.X), Axis(1, value.Y), Axis(2, value.Z));
    }

    /// <summary>
    ///     A model's local matrix at a time, the full FBX transform
    ///     <c>T · Roff · Rp · Rpre · R · Rpost⁻¹ · Rp⁻¹ · Soff · Sp · S · Sp⁻¹</c> (column vectors; reversed here for
    ///     row vectors). Pre / post rotation and the rotation order only apply with RotationActive, as the FBX SDK and
    ///     Blender read them; pre / post rotations are XYZ.
    /// </summary>
    public Matrix4x4 Local(FbxModel m, FbxAnimationStack? stack, long time)
    {
        var t = Animated(m, "Lcl Translation", m.Vector("Lcl Translation", Vector3.Zero), stack, time);
        var r = Animated(m, "Lcl Rotation", m.Vector("Lcl Rotation", Vector3.Zero), stack, time);
        var s = Animated(m, "Lcl Scaling", m.Vector("Lcl Scaling", Vector3.One), stack, time);

        var active = m.Number("RotationActive", 0) != 0;
        var order = active ? (int)m.Number("RotationOrder", 0) : 0;
        var pre = active ? m.Vector("PreRotation", Vector3.Zero) : Vector3.Zero;
        var post = active ? m.Vector("PostRotation", Vector3.Zero) : Vector3.Zero;
        var rOff = m.Vector("RotationOffset", Vector3.Zero);
        var rPiv = m.Vector("RotationPivot", Vector3.Zero);
        var sOff = m.Vector("ScalingOffset", Vector3.Zero);
        var sPiv = m.Vector("ScalingPivot", Vector3.Zero);

        var rotation = Matrix4x4.CreateFromQuaternion(FbxEuler.FromDegrees(r, order));
        var preM = Matrix4x4.CreateFromQuaternion(FbxEuler.FromDegrees(pre, 0));
        var postInv = Matrix4x4.CreateFromQuaternion(Quaternion.Conjugate(FbxEuler.FromDegrees(post, 0)));

        return Matrix4x4.CreateTranslation(-sPiv) * Matrix4x4.CreateScale(s) * Matrix4x4.CreateTranslation(sPiv) *
               Matrix4x4.CreateTranslation(sOff) * Matrix4x4.CreateTranslation(-rPiv) * postInv * rotation * preM *
               Matrix4x4.CreateTranslation(rPiv) * Matrix4x4.CreateTranslation(rOff) * Matrix4x4.CreateTranslation(t);
    }

    /// <summary>A model's world matrix at a time (its parents' transforms applied, as RSrs inheritance)</summary>
    public Matrix4x4 World(FbxModel m, FbxAnimationStack? stack, long time, Dictionary<long, Matrix4x4>? cache = null)
    {
        if (cache != null && cache.TryGetValue(m.Id, out var known))
            return known;

        var world = Local(m, stack, time);
        if (m.Parent != null)
            world *= World(m.Parent, stack, time, cache);

        if (cache != null)
            cache[m.Id] = world;
        return world;
    }

    /// <summary>
    ///     File space to Y up, Z front, X across, in metres (the space Smithbox writes), as a row-vector matrix:
    ///     each file axis sent to where its role is in that space, scaled by <see cref="UnitScaleFactor" /> / 100.
    /// </summary>
    public Matrix4x4 ToCanonical()
    {
        var m = new Matrix4x4();
        void Row(int axis, int sign, Vector3 to)
        {
            var v = to * (sign < 0 ? -1 : 1);
            switch (axis)
            {
                case 0: m.M11 = v.X; m.M12 = v.Y; m.M13 = v.Z; break;
                case 1: m.M21 = v.X; m.M22 = v.Y; m.M23 = v.Z; break;
                default: m.M31 = v.X; m.M32 = v.Y; m.M33 = v.Z; break;
            }
        }

        // Fall back to identity when the three axes aren't distinct
        if (UpAxis == FrontAxis || UpAxis == CoordAxis || FrontAxis == CoordAxis ||
            new[] { UpAxis, FrontAxis, CoordAxis }.Any(a => a is < 0 or > 2))
            m = Matrix4x4.Identity;
        else
        {
            Row(UpAxis, UpAxisSign, Vector3.UnitY);
            Row(FrontAxis, FrontAxisSign, Vector3.UnitZ);
            Row(CoordAxis, CoordAxisSign, Vector3.UnitX);
            m.M44 = 1;
        }

        return m * Matrix4x4.CreateScale((float)(UnitScaleFactor / 100.0));
    }

    public string AxisSummary() => string.Create(CultureInfo.InvariantCulture,
        $"up {(UpAxisSign < 0 ? "-" : "+")}{"XYZ"[Math.Clamp(UpAxis, 0, 2)]}, front {(FrontAxisSign < 0 ? "-" : "+")}{"XYZ"[Math.Clamp(FrontAxis, 0, 2)]}, unit {UnitScaleFactor:0.###} cm");
}
