#nullable enable
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// What a player-like actor (c0000) wears: the parts the Character Preview tool chose, as the Time Act and Model
/// Editors dress c0000. The cut's clip poses c0000's own skeleton; the parts follow it as they follow a TAE
/// animation (<see cref="ModelAnimator.Follow" />):
/// <list type="bullet">
/// <item>Skinned parts (body, armour) each get an animator of their own that takes on the actor's pose node by node.</item>
/// <item>Held parts (weapons) move as a whole with the node they attach to, by the offset
/// <see cref="CharacterComposition.GetAnimatedParts" /> works out.</item>
/// </list>
/// The parts mesh and its rigs come from the model pool, so the cuts of a cutscene share them; a new appearance
/// makes a new mesh and the old one is let go.
/// </summary>
public sealed class CutscenePlayerParts : IDisposable
{
    /// <summary>The prefix of the parts' keys in the model pool</summary>
    public const string PoolPrefix = "parts|";

    private readonly CutsceneModelPool? Pool;
    private readonly IReadOnlyList<AnimatedPart> Parts;
    private readonly Task<ModelRig?>[] Rigs;

    public MeshRenderableProxy? Mesh { get; private set; }

    // Made once the actor's animator and the parts' rigs are ready
    private List<(MeshRenderableProxy Mesh, ModelAnimator Animator)>? Followers;
    private List<(MeshRenderableProxy Mesh, int Node, Matrix4x4 Offset)>? Attachments;

    private static int Serial;

    public int PartCount => Parts.Count;

    /// <summary>A key that is the same for two appearances that look the same</summary>
    public static string KeyOf(CharacterAppearance appearance) =>
        PoolPrefix + string.Join(",", appearance.Parts.Select(p => $"{p.Name}@{p.AttachNode}#{p.EquipParamId}")) +
        "|" + string.Join(",", appearance.Masks ?? Array.Empty<int>());

    public CutscenePlayerParts(ProjectEntry project, RenderScene scene, CutsceneModelPool? pool, CharacterAppearance appearance)
    {
        Pool = pool;

        (MeshRenderableProxy?, string?) Create()
        {
            var uid = $"CutscenePlayer_{System.Threading.Interlocked.Increment(ref Serial)}";
            var mesh = CharacterComposition.CreateMesh(project, scene, appearance, uid, LOC.Get("CUTSCENE_Load_Job_Parts"));
            if (mesh != null)
                mesh.Visible = false;
            return (mesh, uid);
        }

        Mesh = pool != null ? pool.TakeOrNull(KeyOf(appearance), Create) : Create().Item1;
        Parts = Mesh != null ? CharacterComposition.GetAnimatedParts(project, appearance, Mesh) : Array.Empty<AnimatedPart>();

        // Shared textures may have come in with the parts' own
        ResourceManager.SchedulePostTextureRefresh();

        // The skinned parts' rigs, read on the side (armour binders can carry a lot of textures to skip)
        var game = project.Descriptor.ProjectType;
        var fs = project.VFS.FS;
        Rigs = Parts.Select(part =>
        {
            if (part.AttachNode != null)
                return Task.FromResult<ModelRig?>(null);

            Task<ModelRig?> Read() => Task.Run(() =>
            {
                try
                {
                    return fs.ReadFile(part.ContainerPath) is { } data ? ModelRig.Read(game, data, part.ModelName) : null;
                }
                catch (Exception e)
                {
                    Smithbox.LogError(typeof(CutscenePlayerParts), LOC.Get("CUTSCENE_Player_Part_Rig_Failed", part.ModelName), e);
                    return null;
                }
            });

            return pool != null ? pool.Rig($"partrig|{part.ContainerPath}|{part.ModelName}", Read) : Read();
        }).ToArray();
    }

    /// <summary>
    /// Poses the parts after the actor: <paramref name="leader" /> is its animator (null until it is made), with the
    /// rig it animates. Call every frame after the actor's own pose.
    /// </summary>
    public void Update(ModelAnimator? leader, ModelRig? leaderRig, Matrix4x4 cutRoot, bool visible)
    {
        if (Mesh == null)
            return;

        visible &= leader != null;
        if (Mesh.Visible != visible)
            Mesh.Visible = visible;

        if (!visible || leader == null || leaderRig == null)
            return;

        if (Mesh.World != cutRoot)
            Mesh.World = cutRoot;

        if (Followers == null && Rigs.All(r => r.IsCompleted))
            Build(leader, leaderRig);

        if (Followers == null)
            return;

        foreach (var (mesh, animator) in Followers)
        {
            animator.Follow();
            if (mesh.Animator != animator || animator.HasBoneFollowingMeshes)
                mesh.Animator = animator;
        }

        foreach (var (mesh, node, offset) in Attachments!)
            mesh.AttachTransform = offset * leader.GetNodeWorld(node);
    }

    private void Build(ModelAnimator leader, ModelRig leaderRig)
    {
        Followers = new();
        Attachments = new();

        for (var i = 0; i < Parts.Count; i++)
        {
            var part = Parts[i];

            if (part.AttachNode != null)
            {
                var node = leaderRig.FindNode(part.AttachNode);
                if (node >= 0)
                    Attachments.Add((part.Mesh, node, part.AttachOffset ?? Matrix4x4.Identity));
                continue;
            }

            // A part whose bones can't be read stays in its rest pose
            if (Rigs[i].IsCompletedSuccessfully && Rigs[i].Result is { } rig)
                Followers.Add((part.Mesh, new ModelAnimator(rig, leader)));
        }
    }

    /// <summary>The actor's animator is about to go: the parts stop following it, and are made again for the next</summary>
    public void ReleaseLeader()
    {
        if (Followers != null)
        {
            foreach (var (mesh, animator) in Followers)
            {
                mesh.Animator = null;
                animator.Dispose();
            }
        }

        Followers = null;
        Attachments = null;
    }

    /// <summary>
    /// Hands the mesh back to the pool (an appearance no longer chosen is let go there, by
    /// <see cref="CutsceneModelPool.DiscardUnused" />), or lets it go when there is no pool.
    /// </summary>
    public void Dispose()
    {
        ReleaseLeader();

        if (Mesh == null)
            return;

        if (Pool?.Return(Mesh) != true)
        {
            Mesh.Animator = null;
            Mesh.Dispose();
        }

        Mesh = null;
    }
}
