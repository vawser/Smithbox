#nullable enable
using System;
using System.Numerics;

namespace StudioCore.Renderer;

/// <summary>
///     Plays animations on one instance of a model. Models share their static bone buffer between
///     instances, so an animated instance gets a bone range of its own.
///     <para>
///         The range holds a skinning matrix per bone followed by a model space matrix per bone, as
///         weighted vertices are stored in the bind pose and rigid ones in the space of their bone.
///     </para>
/// </summary>
public sealed class ModelAnimator : IDisposable
{
    private const int MatrixSize = 64;
    private const int SkinVertexSize = 16;

    private readonly ModelRig _rig;
    private readonly ModelPose _pose;
    private readonly int[] _meshSkinStart;

    private GPUBufferAllocator.GPUBufferHandle? _boneBuffer;
    private GPUBufferAllocator.GPUBufferHandle? _skinBuffer;

    private bool _poseDirty = true;

    public AnimationPackage Package { get; }
    public AnimationEntry? CurrentAnimation { get; private set; }
    public HavokClip? CurrentClip { get; private set; }

    public float Time { get; private set; }
    public float Speed = 1;
    public bool Loop = true;
    public bool Playing { get; private set; }

    // These go through instance data rather than the bone buffer, so they have to be told about each pose
    public bool HasBoneFollowingMeshes => Array.Exists(_rig.Meshes, m => m.Mode == MeshSkinMode.RigidMesh);

    public int NodeCount => _rig.NodeNames.Length;
    public float Duration => CurrentClip?.Duration ?? 0;
    public float Frame => CurrentClip is { FrameDuration: > 0 } ? Time / CurrentClip.FrameDuration : 0;

    public ModelAnimator(AnimationPackage package, ModelRig rig)
    {
        Package = package;
        _rig = rig;
        _pose = new ModelPose(rig, package.Skeleton!, package.RetargetSkeleton);

        _meshSkinStart = new int[rig.Meshes.Length];
        Array.Fill(_meshSkinStart, -1);

        _boneBuffer = SceneRenderer.BoneBufferAllocator.Allocate((uint)(NodeCount * 2 * MatrixSize), MatrixSize);
        UploadSkin();
    }

    private void UploadSkin()
    {
        var total = 0;
        foreach (MeshRig mesh in _rig.Meshes)
        {
            total += mesh.Vertices?.Length ?? 0;
        }

        if (total == 0)
        {
            return;
        }

        var skin = new SkinVertex[total];
        var offset = 0;
        for (var i = 0; i < _rig.Meshes.Length; i++)
        {
            SkinVertex[]? vertices = _rig.Meshes[i].Vertices;
            if (vertices == null)
            {
                continue;
            }

            _meshSkinStart[i] = offset;
            vertices.CopyTo(skin, offset);
            offset += vertices.Length;
        }

        _skinBuffer = SceneRenderer.BoneBufferAllocator.Allocate((uint)(total * SkinVertexSize), MatrixSize);
        _skinBuffer.FillBuffer(skin);
    }

    /// <summary>
    ///     Starts an animation from the beginning. Returns false if the clip could not be read.
    /// </summary>
    public bool Play(AnimationEntry entry)
    {
        HavokClip? clip = Package.GetClip(entry);
        if (clip == null)
        {
            return false;
        }

        CurrentAnimation = entry;
        CurrentClip = clip;
        Time = 0;
        Playing = true;
        _poseDirty = true;
        return true;
    }

    public void SetPlaying(bool playing)
    {
        Playing = playing && CurrentClip != null;
    }

    public void Seek(float time)
    {
        Time = Math.Clamp(time, 0, Duration);
        _poseDirty = true;
    }

    /// <summary>
    ///     Advances playback, and returns true if the pose has changed.
    /// </summary>
    public bool Update(float deltaTime)
    {
        if (Playing && CurrentClip != null)
        {
            Advance(deltaTime * Speed);
        }

        if (!_poseDirty)
        {
            return false;
        }

        _poseDirty = false;
        ApplyPose();
        return true;
    }

    /// <summary>
    ///     Takes on the pose of another animator of the same package, for models built from several FLVERs.
    /// </summary>
    public void Follow(ModelAnimator leader)
    {
        CurrentAnimation = leader.CurrentAnimation;
        CurrentClip = leader.CurrentClip;
        Time = leader.Time;

        _poseDirty = false;
        ApplyPose();
    }

    /// <summary>
    ///     Current model space matrix of a FLVER node, for models attached to it.
    /// </summary>
    public Matrix4x4 GetNodeWorld(int node)
    {
        return _pose.NodeWorld[node];
    }

    /// <summary>
    ///     How far a FLVER node has moved from its rest pose, for anything else that follows the skeleton.
    /// </summary>
    public Matrix4x4 GetNodeDelta(int node)
    {
        return _rig.NodeInverseReferenceWorld[node] * _pose.NodeWorld[node];
    }

    private void Advance(float delta)
    {
        var duration = Duration;
        if (duration <= 0)
        {
            return;
        }

        Time += delta;
        _poseDirty = true;

        if (Loop)
        {
            Time %= duration;
            if (Time < 0)
            {
                Time += duration;
            }
        }
        else if (Time >= duration || Time < 0)
        {
            Time = Math.Clamp(Time, 0, duration);
            Playing = false;
        }
    }

    private void ApplyPose()
    {
        _pose.Apply(CurrentClip, Math.Min(Frame, Math.Max(0, (CurrentClip?.FrameCount ?? 1) - 1)));

        var count = NodeCount;

        // A new array each time, as the upload happens later on the render thread
        var matrices = new Matrix4x4[count * 2];
        for (var i = 0; i < count; i++)
        {
            matrices[i] = _rig.NodeInverseReferenceWorld[i] * _pose.NodeWorld[i];
            matrices[count + i] = _pose.NodeWorld[i];
        }

        _boneBuffer?.FillBuffer(matrices);
    }

    /// <summary>
    ///     Gets what a mesh needs from this animator to draw. A false result leaves the mesh static.
    /// </summary>
    /// <param name="meshIndex">Index of the mesh within the FLVER.</param>
    /// <param name="vertexOffset">Where the mesh starts in its vertex buffer, as gl_VertexIndex includes it.</param>
    /// <param name="boneStart">The first bone matrix to use.</param>
    /// <param name="skinReference">Tells the vertex shader how to fetch its bones.</param>
    /// <param name="transform">Replacement for the object transform of a mesh that follows a single bone.</param>
    public bool TryGetMeshState(int meshIndex, uint vertexOffset, out uint boneStart, out uint skinReference,
        out Matrix4x4? transform)
    {
        boneStart = 0;
        skinReference = 0;
        transform = null;

        if (_boneBuffer == null || meshIndex < 0 || meshIndex >= _rig.Meshes.Length)
        {
            return false;
        }

        MeshRig mesh = _rig.Meshes[meshIndex];
        var firstBone = _boneBuffer.AllocationStart / MatrixSize;

        switch (mesh.Mode)
        {
            case MeshSkinMode.Skinned:
                if (_skinBuffer == null || _meshSkinStart[meshIndex] < 0)
                {
                    return false;
                }

                var skinStart = _skinBuffer.AllocationStart / SkinVertexSize + (uint)_meshSkinStart[meshIndex];

                // The shader adds gl_VertexIndex, so take the buffer position back out of the start.
                // Wrapping is intended: the sum only has to come out right modulo 2^31.
                skinReference = ((skinStart - vertexOffset) & 0x7FFFFFFFu) | 0x80000000u;
                boneStart = firstBone;
                return true;

            case MeshSkinMode.RigidNormalW:
                skinReference = 1;
                boneStart = firstBone + (uint)NodeCount;
                return true;

            case MeshSkinMode.RigidMesh:
                if (mesh.NodeIndex >= 0 && mesh.NodeIndex < NodeCount)
                {
                    transform = _pose.NodeWorld[mesh.NodeIndex];
                }

                return true;
        }

        return false;
    }

    public void Dispose()
    {
        // Deferred, so that nothing drawn this frame can read a range that has been handed out again
        GPUBufferAllocator.GPUBufferHandle? bones = _boneBuffer;
        GPUBufferAllocator.GPUBufferHandle? skin = _skinBuffer;
        _boneBuffer = null;
        _skinBuffer = null;

        SceneRenderer.AddBackgroundUploadTask((_, _) =>
        {
            SceneRenderer.AddBackgroundUploadTask((_, _) =>
            {
                bones?.Dispose();
                skin?.Dispose();
            });
        });
    }
}
