#nullable enable
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// Poses a skeleton from its clip without a model: for the camera, actors without a model and locators. Bone worlds
/// are made as ModelPose makes them (rotation and translation down the hierarchy, scale apart from it), in the space
/// of the cut.
/// </summary>
public static class CutsceneSkeletonPose
{
    /// <summary>The bone's matrix in the space of the cut, at a frame of the clip.</summary>
    public static Matrix4x4 BoneWorld(HavokSkeletonData skeleton, HavokClip? clip, int bone, float frame)
    {
        Matrix4x4 rotationTranslation = Matrix4x4.Identity;
        Vector3 scale = Vector3.One;

        // Walk from the bone up to its root, then apply them root first
        Span<int> chain = stackalloc int[Math.Min(skeleton.BoneCount, 256)];
        var count = 0;
        for (var b = bone; b >= 0 && b < skeleton.BoneCount && count < chain.Length; b = skeleton.Parents[b])
        {
            chain[count++] = b;
        }

        for (var i = count - 1; i >= 0; i--)
        {
            NewBlendableTransform local = Local(skeleton, clip, chain[i], frame);
            rotationTranslation = local.GetMatrix() * rotationTranslation;
            scale *= local.Scale;
        }

        return Matrix4x4.CreateScale(scale) * rotationTranslation;
    }

    private static NewBlendableTransform Local(HavokSkeletonData skeleton, HavokClip? clip, int bone, float frame)
    {
        var track = clip != null && bone < clip.BoneToTrack.Length ? clip.BoneToTrack[bone] : -1;
        if (track >= 0)
        {
            return clip!.Sample(track, Math.Clamp(frame, 0, Math.Max(0, clip.FrameCount - 1)));
        }

        return skeleton.ReferencePose[bone].ToBlendable();
    }

    /// <summary>
    /// A copy of the skeleton with the actor's name taken off its bones (c4750_0000_L_Foot becomes L_Foot), so the
    /// FLVER's nodes find them by name. The root, named as the actor, keeps its name.
    /// </summary>
    public static HavokSkeletonData WithoutActorPrefix(HavokSkeletonData skeleton)
    {
        var prefix = skeleton.Name + "_";
        var names = new string[skeleton.BoneCount];
        for (var i = 0; i < names.Length; i++)
        {
            var name = skeleton.Names[i];
            names[i] = name.StartsWith(prefix, StringComparison.Ordinal) ? name[prefix.Length..] : name;
        }

        return new HavokSkeletonData
        {
            Name = skeleton.Name,
            Names = names,
            Parents = skeleton.Parents,
            ReferencePose = skeleton.ReferencePose,
            FloatSlots = skeleton.FloatSlots,
            ReferenceFloats = skeleton.ReferenceFloats
        };
    }
}
