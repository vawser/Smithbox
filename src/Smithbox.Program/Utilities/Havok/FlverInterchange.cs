#nullable enable
using SoulsFormats;
using StudioCore.Renderer;
using StudioCore.Utilities.Interchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Utilities;

/// <summary>
///     A character's FLVER as skinned interchange meshes on its Havok skeleton (Animation Editor Analysis.md §14, F5).
///     Skinning comes from <see cref="ModelRig.FromFlver(FLVER2)" />, the same the viewport plays: weighted vertices in
///     model space, and rigid ones (a mesh on one node, or a vertex on the node its normal W names) moved from that
///     node's space into model space with weight 1 on it. FLVER nodes are matched to skeleton bones by name; a node the
///     skeleton doesn't have gives its weight to its nearest ancestor that it has (else the root).
/// </summary>
public static class FlverInterchange
{
    public sealed class Report
    {
        public int Meshes;
        public int Vertices;
        public int Triangles;

        /// <summary>FLVER nodes with weight that the skeleton doesn't have (their weight went to an ancestor)</summary>
        public HashSet<string> NodesNotInSkeleton = new();

        /// <summary>The largest gap between a FLVER node's bind position and its bone's rest position (model space, m)</summary>
        public float WorstBindGap;

        public string WorstBindBone = "";

        /// <summary>Bones whose bind position is more than 1 mm from their rest position</summary>
        public int BonesOffRest;
    }

    public static List<InterchangeMesh> Meshes(FLVER2 flver, string modelName, HavokSkeletonData skeleton, Report report)
    {
        var rig = ModelRig.FromFlver(flver);
        var nodes = rig.NodeNames.Length;

        // FLVER node → skeleton joint: by name, else the nearest ancestor by name, else the root
        var direct = rig.NodeNames.Select(n => Array.IndexOf(skeleton.Names, n)).ToArray();
        var joint = new int[nodes];
        for (var i = 0; i < nodes; i++)
        {
            var j = -1;
            for (int n = i, guard = 0; n >= 0 && n < nodes && guard < nodes; n = rig.NodeParents[n], guard++)
            {
                if (direct[n] >= 0)
                {
                    j = direct[n];
                    break;
                }
            }

            joint[i] = Math.Max(j, 0);
        }

        // Each joint's bind: its FLVER node's reference world; how far that is from the Havok rest pose
        var bind = new Matrix4x4?[skeleton.BoneCount];
        var rest = EditableClip.ModelSpace(skeleton, b => skeleton.ReferencePose[b]);
        for (var i = 0; i < nodes; i++)
        {
            if (direct[i] < 0 || bind[direct[i]] != null)
                continue;
            bind[direct[i]] = rig.NodeReferenceWorld[i];
            var gap = (rig.NodeReferenceWorld[i].Translation - rest[direct[i]].Position).Length();
            if (gap > 1e-3f)
                report.BonesOffRest++;
            if (gap > report.WorstBindGap)
            {
                report.WorstBindGap = gap;
                report.WorstBindBone = rig.NodeNames[i];
            }
        }

        var result = new List<InterchangeMesh>();
        for (var m = 0; m < flver.Meshes.Count; m++)
        {
            var mesh = flver.Meshes[m];
            var skin = rig.Meshes[m];
            if (mesh.Vertices == null || mesh.Vertices.Count == 0)
                continue;

            var faceSet = mesh.FaceSets.Find(fs => fs.Flags == FLVER2.FaceSet.FSFlags.None) ??
                          (mesh.FaceSets.Count > 0 ? mesh.FaceSets[0] : null);
            if (faceSet == null)
                continue;

            var count = mesh.Vertices.Count;
            var triangles = faceSet.Triangulate(count < ushort.MaxValue);
            var valid = new List<int>(triangles.Count);
            for (var t = 0; t + 2 < triangles.Count; t += 3)
            {
                int a = triangles[t], b = triangles[t + 1], c = triangles[t + 2];
                if (a < 0 || b < 0 || c < 0 || a >= count || b >= count || c >= count || a == b || b == c || a == c)
                    continue;
                valid.Add(a);
                valid.Add(b);
                valid.Add(c);
            }

            if (valid.Count == 0)
                continue;

            var hasUVs = mesh.Vertices[0].UVs is { Count: > 0 };
            var im = new InterchangeMesh
            {
                Name = $"{modelName}_{m:D2}",
                Material = mesh.MaterialIndex >= 0 && mesh.MaterialIndex < flver.Materials.Count
                    ? flver.Materials[mesh.MaterialIndex].Name
                    : "",
                Positions = new Vector3[count],
                Normals = new Vector3[count],
                UVs = hasUVs ? new Vector2[count] : null,
                Triangles = valid.ToArray(),
                Joints = new int[count * 4],
                Weights = new float[count * 4],
                JointBind = bind,
            };

            for (var v = 0; v < count; v++)
            {
                var vert = mesh.Vertices[v];
                var normal = vert.Normal == Vector3.Zero ? Vector3.UnitY : Vector3.Normalize(vert.Normal);

                // The node a rigid vertex is stored on
                var rigidNode = skin.Mode switch
                {
                    MeshSkinMode.RigidMesh => skin.NodeIndex,
                    MeshSkinMode.RigidNormalW => vert.NormalW,
                    _ => -1,
                };

                if (skin.Mode != MeshSkinMode.Skinned)
                {
                    var world = rigidNode >= 0 && rigidNode < nodes ? rig.NodeReferenceWorld[rigidNode] : Matrix4x4.Identity;
                    im.Positions[v] = Vector3.Transform(vert.Position, world);
                    im.Normals[v] = Vector3.Normalize(Vector3.TransformNormal(normal, world));
                    im.Joints[v * 4] = rigidNode >= 0 && rigidNode < nodes ? joint[rigidNode] : 0;
                    im.Weights[v * 4] = 1;
                    if (rigidNode >= 0 && rigidNode < nodes && direct[rigidNode] < 0)
                        report.NodesNotInSkeleton.Add(rig.NodeNames[rigidNode]);
                }
                else
                {
                    im.Positions[v] = vert.Position;
                    im.Normals[v] = normal;

                    var sv = skin.Vertices![v];
                    var total = 0f;
                    for (var k = 0; k < 4; k++)
                    {
                        var w = (sv.Weights >> (k * 8)) & 0xFF;
                        var node = (int)(((k < 2 ? sv.Indices01 : sv.Indices23) >> (k % 2 * 16)) & 0xFFFF);
                        im.Joints[v * 4 + k] = node < nodes ? joint[node] : 0;
                        im.Weights[v * 4 + k] = w / 255f;
                        total += w / 255f;
                        if (w > 0 && node < nodes && direct[node] < 0)
                            report.NodesNotInSkeleton.Add(rig.NodeNames[node]);
                    }

                    // Normalise (packed weights round to 1/255); a vertex without weight goes to the root
                    if (total > 0)
                    {
                        for (var k = 0; k < 4; k++)
                            im.Weights[v * 4 + k] /= total;
                    }
                    else
                    {
                        im.Joints[v * 4] = 0;
                        im.Weights[v * 4] = 1;
                    }
                }

                if (im.UVs != null)
                {
                    var uv = vert.UVs[0];
                    im.UVs[v] = new Vector2(uv.X, 1 - uv.Y);
                }
            }

            report.Meshes++;
            report.Vertices += count;
            report.Triangles += valid.Count / 3;
            result.Add(im);
        }

        return result;
    }
}
