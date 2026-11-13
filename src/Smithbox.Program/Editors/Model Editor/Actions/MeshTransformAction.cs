using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Moves, turns or scales a mesh. A FLVER mesh has no transform of its own, so the move is written into its
/// vertices: positions, normals, tangents and bitangents, plus the mesh's and the model's bounding boxes.
/// <para>
/// The move is given in model space, the space the viewport draws in. Skinned (dynamic) meshes keep their vertices
/// in model space. Rigid meshes keep theirs in the space of a bone, which the game (and the viewport) place the vertex
/// with: there the move is carried into the bone's space first.
/// </para>
/// </summary>
public class MeshTransformAction : ViewportAction, IModelHistoryItem
{
    private readonly ModelContainer Container;
    private readonly ModelEntity Entity;
    private readonly Matrix4x4 Move;

    /// <summary>The mesh this step moves</summary>
    public ModelEntity Target => Entity;

    public string Describe()
    {
        return LOC.Get("MODEL_History_Transform", ModelHistory.NameObject(Entity));
    }

    private MeshState Before;
    private MeshState After;

    public MeshTransformAction(ModelContainer container, ModelEntity entity, Matrix4x4 move)
    {
        Container = container;
        Entity = entity;
        Move = move;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (Entity.WrappedObject is not FLVER2.Mesh mesh || Container.Flver == null)
            return ActionEvent.NoEvent;

        if (After == null)
        {
            Before = MeshState.Capture(Container.Flver, mesh);

            Apply(Container.Flver, mesh, Move, Container.Project.Descriptor.ProjectType);

            After = MeshState.Capture(Container.Flver, mesh);
        }
        else
        {
            After.Restore(Container.Flver, mesh);
        }

        // The drawable still holds the geometry it was built with, so it is moved with the data until the next
        // preview of the edited model replaces it
        Entity.DrawableOffset *= Move;
        Entity.InvalidateMeshPivot();
        Entity.UpdateRenderModel();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (Entity.WrappedObject is not FLVER2.Mesh mesh || Container.Flver == null || Before == null)
            return ActionEvent.NoEvent;

        Before.Restore(Container.Flver, mesh);

        if (Matrix4x4.Invert(Move, out var inverse))
        {
            Entity.DrawableOffset *= inverse;
        }

        Entity.InvalidateMeshPivot();
        Entity.UpdateRenderModel();

        return ActionEvent.NoEvent;
    }

    #region Transform
    /// <summary>
    /// The middle of the box around the mesh's vertices where they are drawn, in model space, or null for a mesh
    /// without vertices
    /// </summary>
    public static Vector3? GetDrawnCenter(FLVER2 flver, FLVER2.Mesh mesh, ProjectType projectType)
    {
        if (mesh.Vertices.Count == 0)
            return null;

        var boneWorlds = flver.Nodes.Select(n => Utils.GetBoneWorldMatrix(n, flver.Nodes)).ToArray();
        var placement = new VertexPlacement(flver, mesh, projectType);

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var vertex in mesh.Vertices)
        {
            var node = placement.GetNode(vertex);

            var drawn = node >= 0 && node < boneWorlds.Length
                ? Vector3.Transform(vertex.Position, boneWorlds[node])
                : vertex.Position;

            min = Vector3.Min(min, drawn);
            max = Vector3.Max(max, drawn);
        }

        return (min + max) / 2;
    }

    /// <summary>
    /// Writes a model space move into the vertices of a mesh
    /// </summary>
    public static void Apply(FLVER2 flver, FLVER2.Mesh mesh, Matrix4x4 move, ProjectType projectType)
    {
        var boneWorlds = flver.Nodes.Select(n => Utils.GetBoneWorldMatrix(n, flver.Nodes)).ToArray();

        var placement = new VertexPlacement(flver, mesh, projectType);

        // One vertex transform per bone the vertices are placed by, and one for the vertices in model space
        var transforms = new Dictionary<int, VertexTransform>();

        VertexTransform GetTransform(int node)
        {
            if (!transforms.TryGetValue(node, out var transform))
            {
                transform = node >= 0 && node < boneWorlds.Length
                    ? VertexTransform.InSpaceOf(boneWorlds[node], move)
                    : VertexTransform.InModelSpace(move);

                transforms[node] = transform;
            }

            return transform;
        }

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var vertex in mesh.Vertices)
        {
            var node = placement.GetNode(vertex);
            var transform = GetTransform(node);

            transform.ApplyTo(vertex);

            // The bounds are in model space, where the vertex is drawn
            var drawn = node >= 0 && node < boneWorlds.Length
                ? Vector3.Transform(vertex.Position, boneWorlds[node])
                : vertex.Position;

            min = Vector3.Min(min, drawn);
            max = Vector3.Max(max, drawn);
        }

        if (mesh.Vertices.Count == 0)
            return;

        if (mesh.BoundingBox != null)
        {
            mesh.BoundingBox.Min = min;
            mesh.BoundingBox.Max = max;
        }

        // The model's bounds only grow, as the other meshes may still reach as far as before
        flver.Header.BoundingBoxMin = Vector3.Min(flver.Header.BoundingBoxMin, min);
        flver.Header.BoundingBoxMax = Vector3.Max(flver.Header.BoundingBoxMax, max);
    }

    /// <summary>
    /// The bone each vertex of a mesh is placed by, or -1 for vertices in model space. Follows the viewport's rules,
    /// so the vertices end up where they were seen to be moved to.
    /// </summary>
    private sealed class VertexPlacement
    {
        private readonly FLVER2.Mesh Mesh;

        private readonly bool IsRigid;
        private readonly bool IsDes;

        // DES: the first bone index, through the mesh's bone list, names the bone
        private readonly bool HasBoneIndices;

        // Rigid meshes whose normal W names the bone of each vertex
        private readonly bool UsesNormalW;

        // Before 0x2000E vertex bone indices index the mesh's own bone list
        private readonly bool UsesMeshBoneTable;

        public VertexPlacement(FLVER2 flver, FLVER2.Mesh mesh, ProjectType projectType)
        {
            Mesh = mesh;
            IsRigid = mesh.Dynamic == 0;
            IsDes = projectType is ProjectType.DES;
            UsesMeshBoneTable = flver.Header.Version <= 0x2000D;

            var layout = mesh.VertexBuffers
                .Where(b => b.LayoutIndex >= 0 && b.LayoutIndex < flver.BufferLayouts.Count)
                .SelectMany(b => flver.BufferLayouts[b.LayoutIndex])
                .ToList();

            HasBoneIndices = layout.Any(l => l.Semantic == FLVER.LayoutSemantic.BoneIndices);

            UsesNormalW = layout.Any(l => l.Semantic == FLVER.LayoutSemantic.Normal &&
                (l.Type == FLVER.LayoutType.UByte4 || l.Type == FLVER.LayoutType.Byte4E));
        }

        public int GetNode(FLVER.Vertex vertex)
        {
            if (!IsRigid)
                return -1;

            if (IsDes)
            {
                if (!HasBoneIndices)
                    return Mesh.NodeIndex;

                var local = vertex.BoneIndices[0];

                return local >= 0 && local < Mesh.BoneIndices.Count ? Mesh.BoneIndices[local] : -1;
            }

            if (UsesNormalW)
            {
                var normalW = vertex.NormalW;

                if (UsesMeshBoneTable && normalW >= 0 && normalW < Mesh.BoneIndices.Count)
                    return Mesh.BoneIndices[normalW];

                return normalW;
            }

            return Mesh.NodeIndex;
        }
    }

    /// <summary>
    /// How the positions and directions of vertices in one space change
    /// </summary>
    private sealed class VertexTransform
    {
        private Matrix4x4 Position;
        private Matrix4x4 Normal;

        public static VertexTransform InModelSpace(Matrix4x4 move)
        {
            return Create(move);
        }

        /// <summary>
        /// For vertices placed by a bone: into model space with the bone, moved, and back into the bone's space
        /// </summary>
        public static VertexTransform InSpaceOf(Matrix4x4 boneWorld, Matrix4x4 move)
        {
            if (!Matrix4x4.Invert(boneWorld, out var toBone))
                return Create(move);

            return Create(boneWorld * move * toBone);
        }

        private static VertexTransform Create(Matrix4x4 position)
        {
            // Directions turn with the inverse transpose, so normals stay at right angles to the surface under scaling
            var normal = Matrix4x4.Invert(position, out var inverse)
                ? Matrix4x4.Transpose(inverse)
                : position;

            return new VertexTransform { Position = position, Normal = normal };
        }

        public void ApplyTo(FLVER.Vertex vertex)
        {
            vertex.Position = Vector3.Transform(vertex.Position, Position);
            vertex.Normal = Direction(vertex.Normal, Normal);

            // W holds the handedness of the tangent frame, which a move doesn't change
            for (var t = 0; t < vertex.Tangents.Count; t++)
            {
                var tangent = vertex.Tangents[t];
                vertex.Tangents[t] = new Vector4(Direction(new Vector3(tangent.X, tangent.Y, tangent.Z), Position), tangent.W);
            }

            var bitangent = vertex.Bitangent;
            vertex.Bitangent = new Vector4(Direction(new Vector3(bitangent.X, bitangent.Y, bitangent.Z), Position), bitangent.W);
        }

        private static Vector3 Direction(Vector3 direction, Matrix4x4 matrix)
        {
            if (direction == Vector3.Zero)
                return direction;

            var turned = Vector3.TransformNormal(direction, matrix);

            // Keep the length the direction had (unit for most, but not every format stores unit vectors)
            var length = turned.Length();

            return length > 1e-8f ? turned / length * direction.Length() : direction;
        }
    }
    #endregion

    #region State
    /// <summary>
    /// The vertex data and bounds a move changes, so undo and redo put back exactly what was there
    /// </summary>
    private sealed class MeshState
    {
        private Vector3[] Positions;
        private Vector3[] Normals;
        private Vector4[][] Tangents;
        private Vector4[] Bitangents;

        private Vector3? MeshMin;
        private Vector3? MeshMax;

        private Vector3 ModelMin;
        private Vector3 ModelMax;

        public static MeshState Capture(FLVER2 flver, FLVER2.Mesh mesh)
        {
            var vertices = mesh.Vertices;

            return new MeshState
            {
                Positions = vertices.Select(v => v.Position).ToArray(),
                Normals = vertices.Select(v => v.Normal).ToArray(),
                Tangents = vertices.Select(v => v.Tangents.ToArray()).ToArray(),
                Bitangents = vertices.Select(v => v.Bitangent).ToArray(),
                MeshMin = mesh.BoundingBox?.Min,
                MeshMax = mesh.BoundingBox?.Max,
                ModelMin = flver.Header.BoundingBoxMin,
                ModelMax = flver.Header.BoundingBoxMax
            };
        }

        public void Restore(FLVER2 flver, FLVER2.Mesh mesh)
        {
            var vertices = mesh.Vertices;
            var count = Math.Min(vertices.Count, Positions.Length);

            for (var i = 0; i < count; i++)
            {
                var vertex = vertices[i];

                vertex.Position = Positions[i];
                vertex.Normal = Normals[i];
                vertex.Bitangent = Bitangents[i];

                vertex.Tangents.Clear();
                vertex.Tangents.AddRange(Tangents[i]);
            }

            if (mesh.BoundingBox != null && MeshMin != null && MeshMax != null)
            {
                mesh.BoundingBox.Min = MeshMin.Value;
                mesh.BoundingBox.Max = MeshMax.Value;
            }

            flver.Header.BoundingBoxMin = ModelMin;
            flver.Header.BoundingBoxMax = ModelMax;
        }
    }
    #endregion
}
