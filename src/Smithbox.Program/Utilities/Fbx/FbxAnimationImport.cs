#nullable enable
using StudioCore.Utilities.Interchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Utilities.Fbx;

public sealed class FbxImportSettings
{
    public InterchangeHandedness Handedness = InterchangeHandedness.FlipZ;

    /// <summary>The take to read (an AnimationStack's name); null or not found: the first</summary>
    public string? Take;

    /// <summary>The frame rate the take is sampled at (the clip's)</summary>
    public double FrameRate = 30;

    /// <summary>
    ///     The node read as root motion (by normalised name; joints are taken relative to it): Smithbox's RootMotion by
    ///     default, or another (Blender's "root"). Null or not found: none.
    /// </summary>
    public string? RootMotionNode = FbxAnimationExport.RootMotionNodeName;
}

/// <summary>
///     A take sampled at whole frames as world matrices in game space (row vectors), by normalised joint name
///     (<see cref="FbxAnimationImport.NormaliseName" />). With a RootMotion node, joints are relative to it (character
///     space) and <see cref="RootMotion" /> holds the node's own world matrix per frame.
/// </summary>
public sealed class FbxImportedTake
{
    public string Take = "";
    public double SourceFrameRate;
    public double FrameRate;
    public int FrameCount;
    public readonly Dictionary<string, Matrix4x4[]> World = new(StringComparer.Ordinal);

    /// <summary>Joints the take's base layer animates (others only hold their file pose)</summary>
    public readonly HashSet<string> Animated = new(StringComparer.Ordinal);

    public Matrix4x4[]? RootMotion;
    public string Comment = "";
    public readonly List<string> Notes = new();

    /// <summary>
    ///     The file's skeleton, for retargeting: its joints (LimbNode / Root models, Blender's leaf bones left out),
    ///     parents first, by normalised name; each joint's parent joint (the nearest joint above it, non-joint nodes
    ///     skipped; null for a root); and its rest pose in game space (the bind pose when every joint has one, else the
    ///     models' own values), relative to the RootMotion node like <see cref="World" />.
    /// </summary>
    public readonly List<string> Joints = new();

    public readonly Dictionary<string, string?> JointParent = new(StringComparer.Ordinal);
    public readonly Dictionary<string, Matrix4x4> Rest = new(StringComparer.Ordinal);
    public bool RestFromBindPose;

    /// <summary>Float tracks read from the Smithbox_Floats node, by slot name: a value per frame</summary>
    public readonly Dictionary<string, float[]> Floats = new(StringComparer.Ordinal);

    /// <summary>The node read as root motion, or null</summary>
    public string? RootMotionNode;

    /// <summary>Whether the take animates the root motion node (a take of a several-take file may not)</summary>
    public bool RootMotionAnimated;

    /// <summary>The root motion node's matrices when this take animates it, else null</summary>
    public Matrix4x4[]? AnimatedRootMotion => RootMotionAnimated ? RootMotion : null;

    /// <summary>A joint's world matrices by its name (normalised here), or null when the file has no such joint</summary>
    public Matrix4x4[]? WorldOf(string name) => World.GetValueOrDefault(FbxAnimationImport.NormaliseName(name));

    public bool IsAnimated(string name) => Animated.Contains(FbxAnimationImport.NormaliseName(name));

    public IEnumerable<string> JointNames => World.Keys;
}

/// <summary>
///     Reads a take of an FBX scene for import (Animation Editor Analysis.md §14.4): every model's world transform per
///     frame, turned into game space (the file's axes and units to Y up in metres, then the handedness mirror).
///     Matching to a skeleton is by name, done by the caller.
/// </summary>
public static class FbxAnimationImport
{
    public static IReadOnlyList<string> Takes(FbxScene scene) => scene.Stacks.Select(s => s.Name).ToList();

    private static readonly Regex BlenderSuffix = new(@"\.\d{3}$", RegexOptions.Compiled);

    /// <summary>
    ///     A joint name as matched: without a namespace (<c>mixamorig:</c>, <c>Armature|</c>) or Blender's
    ///     <c>.001</c> suffix.
    /// </summary>
    public static string NormaliseName(string name)
    {
        var i = Math.Max(name.LastIndexOf(':'), name.LastIndexOf('|'));
        if (i >= 0 && i < name.Length - 1)
            name = name[(i + 1)..];
        return BlenderSuffix.Replace(name, "");
    }

    /// <summary>Whether a joint is one of the leaf bones Blender adds on export ("Add Leaf Bones")</summary>
    public static bool IsLeafBone(string name) => name.EndsWith("_end", StringComparison.OrdinalIgnoreCase);

    public static FbxImportedTake Sample(FbxScene scene, FbxImportSettings settings)
    {
        var stack = scene.Stacks.FirstOrDefault(s => s.Name == settings.Take) ?? scene.Stacks.FirstOrDefault();
        var take = new FbxImportedTake
        {
            Take = stack?.Name ?? "",
            SourceFrameRate = scene.FrameRate,
            FrameRate = settings.FrameRate,
            Comment = scene.Comment,
        };

        if (stack == null)
            take.Notes.Add("the file has no animation: its pose is read as one frame");
        else if (stack.CurveNodesOnOtherLayers > 0)
            take.Notes.Add($"{stack.CurveNodesOnOtherLayers} curve nodes on layers above the base layer ignored");
        if (scene.OtherInheritTypes > 0)
            take.Notes.Add($"{scene.OtherInheritTypes} models with an inherit type other than RSrs read as RSrs");

        // Frames at the clip's rate over the take's span
        var frameTicks = FbxAnimationExport.FrameTicks(settings.FrameRate);
        var start = stack?.Start ?? 0;
        var stop = stack?.Stop ?? 0;
        take.FrameCount = Math.Max(1, (int)Math.Round((double)(stop - start) / frameTicks) + 1);
        if (Math.Abs(scene.FrameRate - settings.FrameRate) > 0.01)
            take.Notes.Add($"resampled from {scene.FrameRate:0.##} to {settings.FrameRate:0.##} fps");

        // File space → game space: B = canonical (axes, metres) · mirror; a transform L becomes B⁻¹ · L · B
        var mirror = settings.Handedness == InterchangeHandedness.FlipX
            ? Matrix4x4.CreateScale(-1, 1, 1)
            : Matrix4x4.CreateScale(1, 1, -1);
        var basis = scene.ToCanonical() * mirror;
        Matrix4x4.Invert(basis, out var basisInverse);
        Matrix4x4 ToGame(Matrix4x4 m) => basisInverse * m * basis;

        var root = string.IsNullOrEmpty(settings.RootMotionNode)
            ? null
            : scene.Models.Values.FirstOrDefault(m => NormaliseName(m.Name) == settings.RootMotionNode);
        var floats = scene.Models.Values.FirstOrDefault(m => NormaliseName(m.Name) == FbxAnimationExport.FloatsNodeName);
        var models = scene.Models.Values.Where(m => m != root && m != floats).ToList();
        var byName = new Dictionary<string, FbxModel>(StringComparer.Ordinal);
        var duplicates = 0;
        foreach (var m in models)
        {
            if (!byName.TryAdd(NormaliseName(m.Name), m))
                duplicates++;
        }

        if (duplicates > 0)
            take.Notes.Add($"{duplicates} models share a name with another; the first is used");

        foreach (var name in byName.Keys)
            take.World[name] = new Matrix4x4[take.FrameCount];
        if (root != null)
        {
            take.RootMotion = new Matrix4x4[take.FrameCount];
            take.RootMotionNode = NormaliseName(root.Name);
            take.RootMotionAnimated = stack != null && stack.Base.Keys.Any(k => k.Model == root.Id && k.Property.StartsWith("Lcl "));
        }

        if (stack != null)
        {
            foreach (var (model, property) in stack.Base.Keys)
            {
                if (scene.Models.TryGetValue(model, out var m) && m != root && property.StartsWith("Lcl "))
                    take.Animated.Add(NormaliseName(m.Name));
            }
        }

        // The file's skeleton: joints parents first, their parent joints and rest pose
        static bool IsJoint(FbxModel m) => (m.Type == "LimbNode" || m.Type == "Root") && !IsLeafBone(m.Name);
        var jointModels = byName.Where(p => IsJoint(p.Value)).ToDictionary(p => p.Value.Id, p => p.Key);
        FbxModel? ParentJoint(FbxModel m)
        {
            for (var p = m.Parent; p != null; p = p.Parent)
            {
                if (jointModels.ContainsKey(p.Id))
                    return p;
            }

            return null;
        }

        void AddJoint(FbxModel m)
        {
            var name = jointModels[m.Id];
            if (take.JointParent.ContainsKey(name))
                return;
            var parent = ParentJoint(m);
            if (parent != null)
                AddJoint(parent);
            take.JointParent[name] = parent != null ? jointModels[parent.Id] : null;
            take.Joints.Add(name);
        }

        foreach (var id in jointModels.Keys)
            AddJoint(scene.Models[id]);

        take.RestFromBindPose = jointModels.Count > 0 && jointModels.Keys.All(scene.BindPose.ContainsKey);
        var restCache = new Dictionary<long, Matrix4x4>();
        Matrix4x4 RestWorld(FbxModel m) => take.RestFromBindPose && scene.BindPose.TryGetValue(m.Id, out var bind)
            ? bind
            : scene.World(m, null, 0, restCache);
        var rootRest = root != null ? RestWorld(root) : Matrix4x4.Identity;
        Matrix4x4.Invert(rootRest, out var rootRestInverse);
        foreach (var (id, name) in jointModels)
            take.Rest[name] = ToGame(RestWorld(scene.Models[id]) * rootRestInverse);

        for (var f = 0; f < take.FrameCount; f++)
        {
            var time = start + f * frameTicks;
            var cache = new Dictionary<long, Matrix4x4>();
            var rootWorld = root != null ? scene.World(root, stack, time, cache) : Matrix4x4.Identity;
            Matrix4x4.Invert(rootWorld, out var rootInverse);

            foreach (var (name, model) in byName)
            {
                var world = scene.World(model, stack, time, cache);
                take.World[name][f] = ToGame(root != null ? world * rootInverse : world);
            }

            if (take.RootMotion != null)
                take.RootMotion[f] = ToGame(rootWorld);
        }

        // Float tracks: the floats node's animated properties (and its other number properties, held)
        if (floats != null)
        {
            foreach (var (name, value) in floats.Properties)
            {
                var animated = stack != null && stack.Base.ContainsKey((floats.Id, name));
                if (!animated && value.Type != "Number")
                    continue;
                var samples = new float[take.FrameCount];
                for (var f = 0; f < take.FrameCount; f++)
                    samples[f] = scene.Animated(floats, name, new Vector3((float)value.Number()), stack, start + f * frameTicks).X;
                take.Floats[name] = samples;
            }
        }

        return take;
    }

    /// <summary>
    ///     Models that can be read as root motion: Smithbox's RootMotion node first, then the nodes above the joints
    ///     (Blender's armature, a "root" null) and joints named like a root, by normalised name
    /// </summary>
    public static List<string> RootMotionCandidates(FbxScene scene)
    {
        static bool IsJoint(FbxModel m) => m.Type is "LimbNode" or "Root";
        var result = new List<string>();
        void Add(string name)
        {
            if (!result.Contains(name))
                result.Add(name);
        }

        if (scene.Models.Values.Any(m => NormaliseName(m.Name) == FbxAnimationExport.RootMotionNodeName))
            Add(FbxAnimationExport.RootMotionNodeName);
        foreach (var m in scene.Models.Values)
        {
            var name = NormaliseName(m.Name);
            if (name == FbxAnimationExport.FloatsNodeName)
                continue;
            if (!IsJoint(m) && m.Children.Any(c => IsJoint(c) || c.Children.Count > 0))
                Add(name);
            else if (IsJoint(m) && name.Contains("root", StringComparison.OrdinalIgnoreCase))
                Add(name);
        }

        return result;
    }
}
