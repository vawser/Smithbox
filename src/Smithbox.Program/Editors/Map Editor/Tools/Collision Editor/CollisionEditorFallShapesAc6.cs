using Havok.hk2019;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// AC6's fall protection (f) bodies (Havok 2019), as the game's files hold them (CollisionSurvey AC6 all 0 ac6fall):
/// one physics system whose walls are, per collision filter, a single body with an hknpCompoundShape. Its instances
/// are each one wall run's extern mesh (a strip of bottom and top vertices, 10 high in the game), placed with an
/// identity transform, under a compact simd tree of the instances. The extern meshes are laid out as ER's
/// (CollisionEditorFallShapes.MakeShape), but mutable.
/// </summary>
public static class CollisionEditorFallShapesAc6
{
    /// <summary>The wall height of every wall in AC6's f files.</summary>
    public const float GameWallHeight = 10f;

    // The w slots of an instance transform and of the tree's points hold small integers above 0.5f's bits: the
    // transform's first column 64, its translation and the point the instance's index
    private static readonly int HalfBits = BitConverter.SingleToInt32Bits(0.5f);

    private static float HalfPlus(int n) => BitConverter.Int32BitsToSingle(HalfBits + n);

    /// <summary>
    /// The physics system of an f file, whose bodies are the walls: the first of its physics scene. Null if the
    /// document isn't a Havok 2019 physics scene with one.
    /// </summary>
    public static hknpPhysicsSystemData GetSystem(Havok.HavokDocument document)
    {
        return (document?.Root as hkRootLevelContainer)?.m_namedVariants
            .Select(v => v.m_variant)
            .OfType<hknpPhysicsSceneData>()
            .FirstOrDefault()?.m_systemDatas.FirstOrDefault();
    }

    /// <summary>Whether a document can be the base of a new f file: a physics scene with a single system.</summary>
    public static bool IsTemplate(Havok.HavokDocument document)
    {
        var scene = (document?.Root as hkRootLevelContainer)?.m_namedVariants
            .Select(v => v.m_variant)
            .OfType<hknpPhysicsSceneData>()
            .FirstOrDefault();

        return scene != null && scene.m_systemDatas.Count == 1 && scene.m_systemDatas[0] != null;
    }

    /// <summary>
    /// The body of the walls with a collision filter: an hknpCompoundShape of every wall run, as the game's, at the
    /// origin, unnamed, with no motion properties (0xFFFF).
    /// </summary>
    public static hknpPhysicsSystemData.bodyCinfoWithAttachment MakeBody(List<CollisionEditorFallShapes.WallBody> walls, uint collisionFilter)
    {
        return new hknpPhysicsSystemData.bodyCinfoWithAttachment
        {
            m_shape = MakeCompound(walls),
            m_flags = 0,
            m_collisionCntrl = 0,
            m_collisionFilterInfo = collisionFilter,
            m_materialId = 0,
            m_qualityId = 0,
            m_name = null,
            m_userData = 0,
            m_motionType = 0,
            m_position = Vector4.Zero,
            m_orientation = Quaternion.Identity,
            m_linearVelocity = Vector4.Zero,
            m_angularVelocity = Vector4.Zero,
            m_mass = -1f,
            m_massDistribution = null,
            m_dragProperties = null,
            m_motionPropertiesId = CollisionEditorFallShapes.NoMotionProperties,
            m_desiredBodyId = new hknpBodyId { m_serialAndIndex = 0x00FFFFFF },
            m_motionId = 0x7FFFFFFF,
            m_collisionLookAheadDistance = 0f,
            m_localFrame = null,
            m_activationPriority = 0,
            m_attachedBody = -1
        };
    }

    /// <summary>
    /// A compound of the wall runs as the game's: one instance per run with an identity transform, key bits of the
    /// instance count's bit length, the estimated key count the sum of each instance's key space, the aabb of the
    /// instances (its bounding radius half its diagonal), and a compact simd tree whose points are each instance's
    /// min + max, with its index in w.
    /// </summary>
    public static hknpCompoundShape MakeCompound(List<CollisionEditorFallShapes.WallBody> walls)
    {
        var instances = new hkFreeListArrayHknpShapeInstance { m_firstFree = -1, m_numAllocated = (uint)walls.Count };
        var boxes = new (Vector3 Min, Vector3 Max)[walls.Count];
        var estimated = 0;

        for (var i = 0; i < walls.Count; i++)
        {
            var shape = MakeShape(walls[i].Vertices, walls[i].Indices);
            estimated += 1 << shape.m_numShapeKeyBits;

            var transform = Matrix4x4.Identity;
            transform.M14 = HalfPlus(64);
            transform.M44 = HalfPlus(i);

            instances.m_elements.Add(new hkFreeListArrayElementHknpShapeInstance
            {
                m_transform = transform,
                m_scale = Vector4.One,
                m_shape = shape,
                m_shapeTag = 0xFFFF,
                m_destructionTag = 0xFFFF,
                m_isEmpty = 0,
                m_nextEmptyElement = 0
            });

            boxes[i] = GetBox(walls[i].Vertices);
        }

        var min = boxes.Length == 0 ? Vector3.Zero : boxes.Aggregate(new Vector3(float.MaxValue), (a, b) => Vector3.Min(a, b.Min));
        var max = boxes.Length == 0 ? Vector3.Zero : boxes.Aggregate(new Vector3(float.MinValue), (a, b) => Vector3.Max(a, b.Max));

        var tree = new hknpCompoundShapeSimdTree { m_isCompact = true };
        AddNodes(tree, CollisionEditorFallShapes.BuildTreeLayout(boxes));

        for (var i = 0; i < boxes.Length; i++)
            tree.m_points.Add(new Vector4(boxes[i].Min + boxes[i].Max, HalfPlus(i)));

        return new hknpCompoundShape
        {
            m_flags = hknpShape.FlagsEnum.IS_COMPOSITE_SHAPE,
            m_type = hknpShapeType.Enum.COMPOUND,
            m_dispatchType = hknpCollisionDispatchType.Enum.COMPOSITE,
            m_numShapeKeyBits = (byte)BitLength((uint)walls.Count),
            m_convexRadius = 0f,
            m_userData = 0,
            m_properties = null,
            m_shapeTagCodecInfo = 0xFFFFFFFF,
            m_instances = instances,
            m_aabb = new hkAabb { m_min = new Vector4(min, 0f), m_max = new Vector4(max, 0f) },
            m_boundingRadius = (max - min).Length() / 2f,
            m_isMutable = true,
            m_estimatedNumShapeKeys = estimated,
            m_boundingVolumeData = new hknpCompoundShapeData
            {
                m_type = Havok.hk2019.hknpCompoundShapeBoundingVolumeType.Enum.SIMD_TREE,
                m_simdTree = tree
            }
        };
    }

    /// <summary>
    /// An extern mesh of the triangles as the game's f files hold them: vertices with w 1, triangles without a
    /// material, a shape key bit more than the triangle count's bit length, a compact simd tree, and mutable.
    /// </summary>
    public static hknpExternMeshShape MakeShape(List<Vector3> vertices, List<int> indices)
    {
        var geometry = new hkGeometry();
        geometry.m_vertices.AddRange(vertices.Select(v => new Vector4(v, 1f)));

        for (var i = 0; i + 2 < indices.Count; i += 3)
        {
            geometry.m_triangles.Add(new hkGeometry.Triangle
            {
                m_a = indices[i],
                m_b = indices[i + 1],
                m_c = indices[i + 2],
                m_material = -1
            });
        }

        var tree = new hkcdSimdTree { m_isCompact = true };
        AddNodes(tree, CollisionEditorFallShapes.BuildTreeLayout(CollisionEditorFallShapes.GetTriangleBoxes(vertices, indices)));

        return new hknpExternMeshShape
        {
            m_geometry = new hknpDefaultExternMeshShapeGeometry
            {
                m_geometry = geometry,
                m_useTriangleMaterialAsShapeTag = false
            },
            m_boundingVolumeData = new hknpExternMeshShapeData { m_simdTree = tree },
            m_flags = hknpShape.FlagsEnum.IS_COMPOSITE_SHAPE,
            m_type = hknpShapeType.Enum.EXTERN_MESH,
            m_dispatchType = hknpCollisionDispatchType.Enum.COMPOSITE,
            m_numShapeKeyBits = (byte)(BitLength((uint)geometry.m_triangles.Count) + 1),
            m_convexRadius = 0f,
            m_userData = 0,
            m_properties = null,
            m_shapeTagCodecInfo = 0xFFFFFFFF,
            m_isMutable = true
        };
    }

    private static int BitLength(uint n) => 32 - System.Numerics.BitOperations.LeadingZeroCount(n);

    private static (Vector3 Min, Vector3 Max) GetBox(List<Vector3> vertices)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var v in vertices)
        {
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }

        return (min, max);
    }

    private static void AddNodes(hkcdSimdTree tree, List<CollisionEditorFallShapes.TreeNode> layout)
    {
        foreach (var source in layout)
        {
            var node = new hkcdSimdTree.Node
            {
                m_isLeaf = source.Leaf,
                m_lx = new Vector4(float.MaxValue),
                m_hx = new Vector4(-float.MaxValue),
                m_ly = new Vector4(float.MaxValue),
                m_hy = new Vector4(-float.MaxValue),
                m_lz = new Vector4(float.MaxValue),
                m_hz = new Vector4(-float.MaxValue)
            };

            for (var lane = 0; lane < 4; lane++)
            {
                node.m_data[lane] = source.Data[lane];

                if (source.Lanes[lane] is not { } box)
                    continue;

                node.m_lx = WithLane(node.m_lx, lane, box.Min.X);
                node.m_hx = WithLane(node.m_hx, lane, box.Max.X);
                node.m_ly = WithLane(node.m_ly, lane, box.Min.Y);
                node.m_hy = WithLane(node.m_hy, lane, box.Max.Y);
                node.m_lz = WithLane(node.m_lz, lane, box.Min.Z);
                node.m_hz = WithLane(node.m_hz, lane, box.Max.Z);
            }

            tree.m_nodes.Add(node);
        }
    }

    private static Vector4 WithLane(Vector4 v, int lane, float value)
    {
        switch (lane)
        {
            case 0: v.X = value; break;
            case 1: v.Y = value; break;
            case 2: v.Z = value; break;
            default: v.W = value; break;
        }

        return v;
    }
}
