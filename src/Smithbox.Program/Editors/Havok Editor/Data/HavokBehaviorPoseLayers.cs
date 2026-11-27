#nullable enable
using Havok;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// One clip that a behavior graph plays alongside another: a layer of a layer generator, or a child of a blender, on the
/// way from the root to the clip, with what those nodes and their variables give it.
/// </summary>
/// <param name="AnimationName">The clip's animation, e.g. a000_098000.</param>
/// <param name="Label">The nodes from the path's branch point down to the clip, for display.</param>
/// <param name="Weight">The product of the layer and blend weights down to the clip.</param>
/// <param name="BlendDivisor">
/// For a blender child: the weight of the blender's child holding the playing clip, which a normal (not additive) clip
/// shares the blend with. 0 for a layer, which crossfades by its weight alone.
/// </param>
/// <param name="BoneWeights">The bone masks down to the clip, multiplied (indexed by skeleton bone); null for none.</param>
/// <param name="Underlay">Comes from a layer before the one playing the clip: shows only on bones the clip doesn't track.</param>
/// <param name="Looping">The clip generator loops (MODE_LOOPING); otherwise it plays once and holds.</param>
public sealed record HavokBehaviorLayer(string AnimationName, string Label, float Weight, float BlendDivisor,
    float[]? BoneWeights, bool Underlay, bool Looping)
{
    /// <summary>The weight to play the clip at, given whether its file is additive</summary>
    public float EffectiveWeight(bool additive)
    {
        return additive || BlendDivisor <= 0 ? Weight : Weight / (BlendDivisor + Weight);
    }
}

/// <summary>
/// The layers a behavior graph plays with one animation, and the variables that set them.
/// </summary>
/// <param name="Path">The named nodes from the root to the clip.</param>
/// <param name="Layers">Underlays from the outermost in, then overlays from the innermost out: the order they apply in.</param>
/// <param name="Variables">Each variable a weight, selector or start state read, with the value it had.</param>
public sealed record HavokBehaviorLayerPlan(string Path, IReadOnlyList<HavokBehaviorLayer> Layers,
    IReadOnlyList<(string Name, float Value)> Variables);

/// <summary>
/// Works out, from a Havok behavior file, which clips play alongside an animation when the graph plays it: every layer
/// of a layer generator and every child of a blender that sits beside the path from the root down to the clip, with its
/// weight, bone mask and the selectors and state machines below it resolved by their variables (initial values unless
/// overridden). Nothing is game-specific: whatever graph the file holds decides.
///
/// Approximations: layers off by default wait for an event and are left out; a blender child's normal clip is shared
/// with the path's child alone (not every sibling); the path's own bone masks (half-body blends) are not applied; the
/// clip is matched by animation name, first in walk order. Read by field name, so every SDK version works alike.
/// </summary>
public sealed class HavokBehaviorPoseGraph
{
    private const int MaxDepth = 96;

    private static readonly string[] ChildFields = { "m_child", "m_generator", "m_generators", "m_layers", "m_children", "m_states" };

    private readonly object? _root;
    private readonly IList? _variableNames;
    private readonly IList? _variableInfos;
    private readonly IList? _initialValues;
    private readonly HashSet<string> _animations = new(StringComparer.OrdinalIgnoreCase);

    public string FileName { get; }

    public HavokBehaviorPoseGraph(HavokDocument document, string fileName)
    {
        FileName = fileName;

        foreach (var obj in document.Objects)
        {
            switch (obj.GetType().Name)
            {
                case "hkbBehaviorGraph":
                    _root ??= Get(obj, "m_rootGenerator");
                    break;
                case "hkbBehaviorGraphStringData":
                    _variableNames ??= Get(obj, "m_variableNames") as IList;
                    break;
                case "hkbBehaviorGraphData":
                    _variableInfos ??= Get(obj, "m_variableInfos") as IList;
                    _initialValues ??= Get(Get(obj, "m_variableInitialValues"), "m_wordVariableValues") as IList;
                    break;
                case "hkbClipGenerator":
                    if (Get(obj, "m_animationName") is string name)
                        _animations.Add(name);
                    break;
            }
        }
    }

    /// <summary>Whether a clip of the graph plays this animation (by name, e.g. a000_000000)</summary>
    public bool Plays(string animationName) => _animations.Contains(animationName);

    /// <summary>
    /// The layers played with an animation, or null if the graph doesn't reach a clip of it.
    /// </summary>
    /// <param name="overrides">Variable values by name, in place of the file's initial values.</param>
    public HavokBehaviorLayerPlan? Build(string animationName, IReadOnlyDictionary<string, float> overrides)
    {
        if (_root == null || !Plays(animationName))
            return null;

        var path = new List<object>();
        if (!FindPath(_root, animationName, path, new HashSet<object>(ReferenceEqualityComparer.Instance), 0))
            return null;

        var context = new Context(this, overrides, CategoryOf(animationName));
        var underlays = new List<HavokBehaviorLayer>();
        var overlays = new List<HavokBehaviorLayer>();

        // Outermost first for the underlays, innermost first for the overlays: an inner node's result is what the outer
        // one layers over
        for (var i = 0; i < path.Count - 1; i++)
        {
            var node = path[i];
            var type = node.GetType().Name;
            var next = path[i + 1];

            if (type == "hkbLayerGenerator" && Get(node, "m_layers") is IList layers)
            {
                var onPath = IndexOf(layers, next);
                for (var j = 0; j < onPath; j++)
                    context.Collect(layers[j], 1, null, 0, true, Label(layers[j]), underlays, 0);
            }
        }

        for (var i = path.Count - 2; i >= 0; i--)
        {
            var node = path[i];
            var type = node.GetType().Name;
            var next = path[i + 1];

            if (type == "hkbLayerGenerator" && Get(node, "m_layers") is IList layers)
            {
                var onPath = IndexOf(layers, next);
                for (var j = onPath + 1; j < layers.Count; j++)
                    context.Collect(layers[j], 1, null, 0, false, Label(layers[j]), overlays, 0);
            }
            else if (type == "hkbBlenderGenerator" && Get(node, "m_children") is IList children)
            {
                var onPath = IndexOf(children, next);
                var pathWeight = onPath >= 0 ? context.Weight(children[onPath], "weight", "m_weight") : 1;

                for (var j = 0; j < children.Count; j++)
                {
                    if (j != onPath)
                        context.Collect(children[j], 1, null, Math.Max(pathWeight, 0), false, Label(children[j]), overlays, 0);
                }
            }
        }

        var pathLabel = string.Join(" › ", path.Select(n => Get(n, "m_name") as string).Where(n => !string.IsNullOrEmpty(n)));

        return new HavokBehaviorLayerPlan(pathLabel, underlays.Concat(overlays).ToList(),
            context.Read.Select(kv => (kv.Key, kv.Value)).OrderBy(v => v.Key).ToList());
    }

    private bool FindPath(object node, string animationName, List<object> path, HashSet<object> visited, int depth)
    {
        if (depth > MaxDepth || !visited.Add(node))
            return false;

        path.Add(node);

        if (node.GetType().Name == "hkbClipGenerator" &&
            string.Equals(Get(node, "m_animationName") as string, animationName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var child in Children(node))
        {
            if (FindPath(child, animationName, path, visited, depth + 1))
                return true;
        }

        path.RemoveAt(path.Count - 1);
        return false;
    }

    private static IEnumerable<object> Children(object node)
    {
        foreach (var field in ChildFields)
        {
            var value = Get(node, field);

            if (value is IList list)
            {
                foreach (var item in list)
                {
                    if (item != null)
                        yield return item;
                }
            }
            else if (value != null && value is not string && !value.GetType().IsValueType)
            {
                yield return value;
            }
        }
    }

    // The clip category of an animation name: a000 of a000_000000
    private static string CategoryOf(string animationName)
    {
        var split = animationName.IndexOf('_');
        return split > 0 ? animationName[..split] : "";
    }

    private static int IndexOf(IList list, object item)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], item))
                return i;
        }

        return -1;
    }

    private static string Label(object node)
    {
        var name = Get(node, "m_name") as string;
        if (!string.IsNullOrEmpty(name))
            return name;

        // Layers and blender children have no name: their generator does
        return Get(Get(node, "m_generator"), "m_name") as string ?? node.GetType().Name;
    }

    private static object? Get(object? obj, string field) => HavokBehaviorSymbols.GetValue(obj, field);

    private static float ToFloat(object? value, float fallback)
    {
        if (value == null)
            return fallback;

        try
        {
            return value is Enum ? Convert.ToInt32(value) : Convert.ToSingle(value);
        }
        catch
        {
            return fallback;
        }
    }

    private static float[]? Combine(float[]? mask, object? boneWeightArray)
    {
        if (Get(boneWeightArray, "m_boneWeights") is not IList weights || weights.Count == 0)
            return mask;

        var length = mask == null ? weights.Count : Math.Min(mask.Length, weights.Count);
        var result = new float[length];

        for (var i = 0; i < length; i++)
            result[i] = ToFloat(weights[i], 0) * (mask?[i] ?? 1);

        return result;
    }

    /// <summary>Resolves the subtrees beside the path into clips, reading variables as it goes</summary>
    private sealed class Context
    {
        private readonly HavokBehaviorPoseGraph _graph;
        private readonly IReadOnlyDictionary<string, float> _overrides;
        private readonly string _category;

        public readonly SortedDictionary<string, float> Read = new(StringComparer.OrdinalIgnoreCase);

        public Context(HavokBehaviorPoseGraph graph, IReadOnlyDictionary<string, float> overrides, string category)
        {
            _graph = graph;
            _overrides = overrides;
            _category = category;
        }

        public void Collect(object? node, float weight, float[]? mask, float divisor, bool underlay, string label,
            List<HavokBehaviorLayer> into, int depth)
        {
            if (node == null || weight <= 0 || depth > MaxDepth)
                return;

            switch (node.GetType().Name)
            {
                case "hkbLayer":
                {
                    var blending = Get(node, "m_blendingControlData");

                    // Off by default and waiting for an event that turns it on
                    if (Get(blending, "m_onByDefault") is false && ToFloat(Get(blending, "m_onEventId"), -1) >= 0)
                        return;

                    var w = Bound(node, "blendingControlData/weight") ?? ToFloat(Get(blending, "m_weight"), 1);
                    Collect(Get(node, "m_generator"), weight * w, Combine(mask, Get(node, "m_boneWeights")), divisor,
                        underlay, label, into, depth + 1);
                    break;
                }
                case "hkbBlenderGeneratorChild":
                {
                    var w = Weight(node, "weight", "m_weight");
                    Collect(Get(node, "m_generator"), weight * w, Combine(mask, Get(node, "m_boneWeights")), divisor,
                        underlay, label, into, depth + 1);
                    break;
                }
                case "hkbLayerGenerator":
                    foreach (var layer in (Get(node, "m_layers") as IList)?.Cast<object>() ?? [])
                        Collect(layer, weight, mask, divisor, underlay, $"{label} › {HavokBehaviorPoseGraph.Label(layer)}", into, depth + 1);
                    break;
                case "hkbBlenderGenerator":
                    foreach (var child in (Get(node, "m_children") as IList)?.Cast<object>() ?? [])
                        Collect(child, weight, mask, divisor, underlay, $"{label} › {HavokBehaviorPoseGraph.Label(child)}", into, depth + 1);
                    break;
                case "hkbStateMachine":
                {
                    var start = (int)(Bound(node, "startStateId") ?? ToFloat(Get(node, "m_startStateId"), 0));
                    var states = (Get(node, "m_states") as IList)?.Cast<object>().Where(s => s != null).ToList() ?? new();
                    var state = states.FirstOrDefault(s => (int)ToFloat(Get(s, "m_stateId"), -1) == start) ?? states.FirstOrDefault();
                    Collect(Get(state, "m_generator"), weight, mask, divisor, underlay, label, into, depth + 1);
                    break;
                }
                case "hkbManualSelectorGenerator":
                {
                    var index = (int)(Bound(node, "selectedGeneratorIndex") ?? ToFloat(Get(node, "m_selectedGeneratorIndex"), 0));
                    if (Get(node, "m_generators") is IList generators && index >= 0 && index < generators.Count)
                        Collect(generators[index], weight, mask, divisor, underlay, label, into, depth + 1);
                    break;
                }
                case "CustomManualSelectorGenerator":
                    Collect(PickClip(Get(node, "m_generators") as IList), weight, mask, divisor, underlay, label, into, depth + 1);
                    break;
                case "hkbClipGenerator":
                    if (Get(node, "m_animationName") is string name && !string.IsNullOrEmpty(name))
                    {
                        var looping = (int)ToFloat(Get(node, "m_mode"), 0) == 1;
                        into.Add(new HavokBehaviorLayer(name, label, weight, divisor, mask, underlay, looping));
                    }
                    break;
                default:
                    // Script and modifier generators pass straight through to their child
                    Collect(Get(node, "m_child") ?? Get(node, "m_generator"), weight, mask, divisor, underlay, label, into, depth + 1);
                    break;
            }
        }

        // The clip for the playing animation's category, else the base category's, else the first
        private object? PickClip(IList? generators)
        {
            if (generators == null)
                return null;

            var clips = generators.Cast<object>().Where(g => g != null).ToList();

            bool InCategory(object g, string category) =>
                Get(g, "m_animationName") is string n && n.StartsWith(category + "_", StringComparison.OrdinalIgnoreCase);

            return clips.FirstOrDefault(g => _category != "" && InCategory(g, _category))
                ?? clips.FirstOrDefault(g => InCategory(g, "a000"))
                ?? clips.FirstOrDefault();
        }

        /// <summary>A weight field, or the variable bound to it</summary>
        public float Weight(object? node, string memberPath, string field)
        {
            return Bound(node, memberPath) ?? ToFloat(Get(node, field), 1);
        }

        /// <summary>The value of the variable bound to a member of the node, or null if none is (or it's a character property)</summary>
        private float? Bound(object? node, string memberPath)
        {
            if (Get(Get(node, "m_variableBindingSet"), "m_bindings") is not IList bindings)
                return null;

            foreach (var binding in bindings)
            {
                if (!string.Equals(Get(binding, "m_memberPath") as string, memberPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Binding type 1 reads a character property, which the game sets
                if ((int)ToFloat(Get(binding, "m_bindingType"), 0) != 0)
                    return null;

                return Variable((int)ToFloat(Get(binding, "m_variableIndex"), -1));
            }

            return null;
        }

        private float? Variable(int index)
        {
            var names = _graph._variableNames;
            if (names == null || index < 0 || index >= names.Count || names[index] is not string name)
                return null;

            if (!_overrides.TryGetValue(name, out var value))
                value = Initial(index);

            Read[name] = value;
            return value;
        }

        // The word value as its type stores it: real variables keep float bits, the others an integer
        private float Initial(int index)
        {
            var values = _graph._initialValues;
            if (values == null || index >= values.Count)
                return 0;

            var word = (int)ToFloat(Get(values[index], "m_value"), 0);
            var infos = _graph._variableInfos;
            var type = infos != null && index < infos.Count ? Get(infos[index], "m_type")?.ToString() ?? "" : "";

            return type.Contains("REAL", StringComparison.OrdinalIgnoreCase) ? BitConverter.Int32BitsToSingle(word) : word;
        }
    }
}
