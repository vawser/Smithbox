#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Utilities.Interchange;

/// <summary>
///     A local transform in an interchange file's own space (FBX, later glTF): translate · rotate · scale.
///     See Claude/Documentation/Animation Editor Analysis.md §14.3.
/// </summary>
public struct InterchangeTransform
{
    public Vector3 Translation;
    public Quaternion Rotation;
    public Vector3 Scale;

    public InterchangeTransform(Vector3 translation, Quaternion rotation, Vector3 scale)
    {
        Translation = translation;
        Rotation = rotation;
        Scale = scale;
    }

    public static InterchangeTransform Identity => new(Vector3.Zero, Quaternion.Identity, Vector3.One);

    /// <summary>As a matrix for row vectors (System.Numerics): scale, then rotate, then translate.</summary>
    public Matrix4x4 ToMatrix() =>
        Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Translation);
}

/// <summary>One joint (bone) of an <see cref="InterchangeSkeleton" />.</summary>
public sealed class InterchangeJoint
{
    public string Name = "";

    /// <summary>The parent joint's index, -1 for a root</summary>
    public int Parent = -1;

    /// <summary>The rest (bind) pose, local to the parent</summary>
    public InterchangeTransform Rest = InterchangeTransform.Identity;
}

/// <summary>A skeleton as the interchange file holds it: joints in an order where parents come first is not required.</summary>
public sealed class InterchangeSkeleton
{
    public string Name = "";
    public List<InterchangeJoint> Joints = new();

    /// <summary>Every joint's rest pose in model space (row-vector matrices).</summary>
    public Matrix4x4[] RestWorld()
    {
        var world = new Matrix4x4[Joints.Count];
        var done = new bool[Joints.Count];

        void Resolve(int j)
        {
            if (done[j])
                return;
            done[j] = true;
            var local = Joints[j].Rest.ToMatrix();
            var parent = Joints[j].Parent;
            if (parent < 0 || parent >= Joints.Count || parent == j)
            {
                world[j] = local;
                return;
            }

            Resolve(parent);
            world[j] = local * world[parent];
        }

        for (var j = 0; j < Joints.Count; j++)
            Resolve(j);
        return world;
    }
}

/// <summary>
///     A clip sampled at whole frames: per joint, its local transform at each frame (or none, when the joint holds its
///     rest pose), and optional root motion as a transform per frame.
/// </summary>
public sealed class InterchangeClip
{
    public string Name = "";
    public double FrameRate = 30;
    public int FrameCount;

    /// <summary>[joint][frame]: local transforms, or null for a joint the clip doesn't animate</summary>
    public InterchangeTransform[]?[] Tracks = Array.Empty<InterchangeTransform[]?>();

    /// <summary>[frame]: where the root motion has taken the character (model space), or null without root motion</summary>
    public InterchangeTransform[]? RootMotion;

    /// <summary>Written into the file's metadata (FBX: SceneInfo comment), read back on import</summary>
    public string Comment = "";

    /// <summary>Float tracks by slot name: a value per frame</summary>
    public Dictionary<string, float[]> Floats = new(StringComparer.Ordinal);
}

/// <summary>
///     How game space (left-handed, Y up) is turned into an interchange file's right-handed space: by negating one
///     axis. A mirror is its own inverse, so import uses the same one.
/// </summary>
public enum InterchangeHandedness
{
    /// <summary>
    ///     Negate Z. In a Y-up FBX, Blender then places game (x, y, z) at Blender (x, z, y): where the community FLVER
    ///     importers put models.
    /// </summary>
    FlipZ,

    /// <summary>Negate X, as DSAnimStudio's FBX importer expects.</summary>
    FlipX,
}

/// <summary>
///     A skinned triangle mesh in game space, its bind pose (Animation Editor Analysis.md §14, F5): per vertex a position,
///     normal and UV, up to four skeleton joints with weights; triangles as vertex index triples (counter-clockwise as
///     the game draws them).
/// </summary>
public sealed class InterchangeMesh
{
    public string Name = "";
    public string Material = "";
    public Vector3[] Positions = Array.Empty<Vector3>();
    public Vector3[] Normals = Array.Empty<Vector3>();

    /// <summary>One per vertex, V from the bottom (FBX's way), or null</summary>
    public Vector2[]? UVs;

    public int[] Triangles = Array.Empty<int>();

    /// <summary>Four per vertex: joint indices into the skeleton, and their weights (summing to 1)</summary>
    public int[] Joints = Array.Empty<int>();
    public float[] Weights = Array.Empty<float>();

    /// <summary>
    ///     Per skeleton joint: its world matrix when the mesh was bound (the model's own node), or null to use the
    ///     skeleton's rest pose
    /// </summary>
    public Matrix4x4?[] JointBind = Array.Empty<Matrix4x4?>();

    public int VertexCount => Positions.Length;
}

/// <summary>Where a clip's root motion comes from on import</summary>
public enum ImportRootMotion
{
    /// <summary>A node of the file: Smithbox's RootMotion (what it exports) or another, by name (<see cref="ClipImportSource.RootNode" />)</summary>
    FromNode,

    /// <summary>
    ///     Taken out of a bone of the imported clip (<see cref="ClipImportSource.RootBone" />), as the Root Motion tool's
    ///     Add from Bone does: its travel along the ground and its turn
    /// </summary>
    FromBone,

    /// <summary>The clip's own root motion, as it was before the import</summary>
    Keep,

    None,
}

/// <summary>What bones the file doesn't animate (or doesn't have) do on import</summary>
public enum ImportMissingBones
{
    /// <summary>Keep the clip's own motion for them</summary>
    KeepClip,

    /// <summary>Hold the skeleton's reference pose</summary>
    RestPose,
}

/// <summary>
///     The file and settings a clip was imported from, kept with the clip (and in the work files) so it can be imported
///     again after the file changes (Reimport).
/// </summary>
/// <param name="Retarget">The file is another skeleton's, moved onto the character by the retarget map (F3)</param>
/// <param name="RootNode">For <see cref="ImportRootMotion.FromNode" />: the node's name (null: Smithbox's RootMotion)</param>
/// <param name="RootBone">For <see cref="ImportRootMotion.FromBone" />: the character's bone</param>
public sealed record ClipImportSource(string Path, string Take, InterchangeHandedness Handedness,
    ImportRootMotion RootMotion, ImportMissingBones MissingBones, bool Retarget = false, string? RootNode = null,
    string? RootBone = null);

public static class InterchangeConvert
{
    public static Vector3 Mirror(Vector3 v, InterchangeHandedness h) => h switch
    {
        InterchangeHandedness.FlipX => new Vector3(-v.X, v.Y, v.Z),
        _ => new Vector3(v.X, v.Y, -v.Z),
    };

    /// <summary>
    ///     A rotation seen in the mirror: about the mirrored axis by the opposite angle. For a mirror of axis k, the
    ///     quaternion's other two vector parts are negated.
    /// </summary>
    public static Quaternion Mirror(Quaternion q, InterchangeHandedness h) => h switch
    {
        InterchangeHandedness.FlipX => new Quaternion(q.X, -q.Y, -q.Z, q.W),
        _ => new Quaternion(-q.X, -q.Y, q.Z, q.W),
    };

    public static InterchangeTransform Mirror(InterchangeTransform t, InterchangeHandedness h) =>
        new(Mirror(t.Translation, h), Mirror(t.Rotation, h), t.Scale);
}
