using Havok.Builders;
using Havok.Shared;
using StudioCore.Editors.HavokEditor;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The parts of the Collision Editor that depend on the Havok class set: the FromSoftware compressed meshes of
/// hk2014 (DS3), hk2016 (SDT), hk2018 (ER, NR) and hk2019 (AC6). Every one is built by HavokCompressedMeshBuilder,
/// then turned into its own classes: DS3's by HavokCompressedMeshGenerator_hk2014, the others by their
/// CollisionShapeBuilder.
/// </summary>
public static class CollisionEditorShapes
{
    /// <summary>A rebuilt shape, and the shape key (as stored, top-aligned) each input triangle got.</summary>
    public sealed record BuiltShape(object Shape, uint[] StoredKeys);

    /// <summary>
    /// Whether the editor can reshape a body's shape: a FromSoftware compressed mesh (hknp: DS3, BB, SDT, ER, NR,
    /// DB_NT, AC6), or a MOPP over a FromSoftware storage-extended mesh (hkp: DES, DS1, DS1R, DS2S).
    /// </summary>
    public static bool IsSupportedShape(object shape)
    {
        return IsCompressedMeshShape(shape) || HkpMeshShapeRebuild.IsSupported(shape);
    }

    /// <summary>A FromSoftware compressed mesh (hknp), which can also be built from new triangles.</summary>
    public static bool IsCompressedMeshShape(object shape)
    {
        return shape is Havok.hk2014.fsnpCustomParamCompressedMeshShape
            or Havok.hk2016.fsnpCustomParamCompressedMeshShape
            or Havok.hk2018.fsnpCustomParamCompressedMeshShape
            or Havok.hk2019.fsnpCustomParamCompressedMeshShape;
    }

    /// <summary>
    /// A compressed mesh (hknp) whose triangles can be decoded and edited: a FromSoftware one, or a plain Havok one
    /// (some DS3 objects), which has no materials and is rebuilt as a FromSoftware one with material 0.
    /// </summary>
    public static bool IsDecodableMeshShape(object shape)
    {
        return IsCompressedMeshShape(shape)
            || shape is Havok.hk2014.hknpCompressedMeshShape
            or Havok.hk2016.hknpCompressedMeshShape
            or Havok.hk2018.hknpCompressedMeshShape
            or Havok.hk2019.hknpCompressedMeshShape;
    }

    /// <summary>
    /// Whether a body's shape can be replaced by a mesh built from new triangles: any hknp shape (a convex or box shape
    /// becomes a FromSoftware compressed mesh of the same class set, as objects of those games also have), or an older
    /// game's MOPP mesh (hkp), whose storages are the template of the new mesh.
    /// </summary>
    public static bool CanReplaceShape(object shape)
    {
        return HkpMeshShapeRebuild.IsSupported(shape)
            || shape is Havok.hk2014.hknpShape or Havok.hk2016.hknpShape or Havok.hk2018.hknpShape or Havok.hk2019.hknpShape;
    }

    /// <summary>
    /// An older game's (hkp) single rigid body whose shape isn't a mesh (a box, convex hull or a list of them, as most
    /// of their objects have): it has nothing to base a mesh on, so a new one needs a template from another file of
    /// the game (see BuildShapeFromTemplate). Null if the document isn't one.
    /// </summary>
    public static IHavokBody GetBodyNeedingTemplate(Havok.HavokDocument document)
    {
        var bodies = CollisionGeometry.Bodies(document.Objects).ToList();

        return bodies.Count == 1 && HkpMeshShapeRebuild.GetBodyName(bodies[0]) != null && !CanReplaceShape(bodies[0].Shape)
            ? bodies[0]
            : null;
    }

    /// <summary>
    /// Whether a body's shape can be replaced by a mesh, either from its own shape (CanReplaceShape) or, for an older
    /// game's rigid body with none, from another file's mesh (BuildShapeFromTemplate).
    /// </summary>
    public static bool CanReplaceBody(IHavokBody body, out bool needsTemplate)
    {
        needsTemplate = false;

        if (body?.Shape == null)
            return false;

        if (CanReplaceShape(body.Shape))
            return true;

        needsTemplate = HkpMeshShapeRebuild.GetBodyName(body) != null;
        return needsTemplate;
    }

    /// <summary>A body's name, as the file has it (hkp rigid bodies and hknp body infos both name theirs), or null.</summary>
    public static string GetBodyName(IHavokBody body)
    {
        return HkpMeshShapeRebuild.GetBodyName(body) ?? body?.GetType().GetField("m_name")?.GetValue(body) as string;
    }

    /// <summary>
    /// How a body moves, for telling bodies apart: an hkp rigid body's motion type (MOTION_FIXED, MOTION_KEYFRAMED,
    /// MOTION_BOX_INERTIA...), an hknp body's motion number. Null if it says nothing.
    /// </summary>
    public static string GetBodyMotion(IHavokBody body)
    {
        var motion = body?.GetType().GetField("m_motion")?.GetValue(body);
        var type = motion?.GetType().GetField("m_type")?.GetValue(motion);

        if (type != null)
            return type.ToString();

        return body?.GetType().GetField("m_motionId")?.GetValue(body)?.ToString();
    }

    /// <summary>
    /// A new mesh from triangles, built on another file's MOPP mesh of the same class set (its storage, material
    /// parameter and code settings), for a body whose own shape has none. Null if the template isn't one.
    /// </summary>
    public static BuiltShape BuildShapeFromTemplate(object templateShape, List<Vector3> vertices, List<int> indices, List<uint> materials)
    {
        if (!HkpMeshShapeRebuild.IsSupported(templateShape))
            return null;

        var shape = HkpMeshShapeRebuild.BuildNew(templateShape, vertices, indices, materials, out var keys);

        return shape == null ? null : new BuiltShape(shape, keys);
    }

    /// <summary>The body of a document whose shape a new mesh can replace (a single body, see CanReplaceShape), or null.</summary>
    public static IHavokBody GetReplaceableBody(Havok.HavokDocument document)
    {
        var bodies = CollisionGeometry.Bodies(document.Objects).ToList();

        return bodies.Count == 1 && CanReplaceShape(bodies[0].Shape) ? bodies[0] : null;
    }

    /// <summary>
    /// An older game's (hkp) shape with its vertices moved: each triangle (by its shape key, which a rebuild keeps)
    /// gives its storage corners the positions of its welded vertices. Triangles left out of the input (no area left)
    /// are left out of the MOPP. Null if the shape isn't an hkp mesh this can rebuild.
    /// </summary>
    public static BuiltShape BuildHkpShape(object oldShape, List<Vector3> vertices, List<int> indices, List<uint> keys)
    {
        var mesh = HkpMeshShapeRebuild.Read(oldShape);

        if (mesh == null)
            return null;

        var storageVertices = mesh.StorageVertices.Select(v => (Vector3[])v.Clone()).ToList();
        var skip = new HashSet<uint>();

        for (var s = 0; s < mesh.StorageTriangles.Count; s++)
        {
            for (var t = 0; t < mesh.StorageTriangles[s].Count; t++)
                skip.Add(mesh.Key(s, t));
        }

        for (var i = 0; i < keys.Count; i++)
        {
            var (s, t) = mesh.FromKey(keys[i]);

            if (s >= mesh.StorageTriangles.Count || t >= mesh.StorageTriangles[s].Count)
                return null;

            var (a, b, c) = mesh.StorageTriangles[s][t];
            storageVertices[s][a] = vertices[indices[i * 3]];
            storageVertices[s][b] = vertices[indices[i * 3 + 1]];
            storageVertices[s][c] = vertices[indices[i * 3 + 2]];
            skip.Remove(keys[i]);
        }

        var shape = HkpMeshShapeRebuild.Rebuild(oldShape, storageVertices, skip);

        return shape == null ? null : new BuiltShape(shape, keys.ToArray());
    }

    /// <summary>
    /// The collision material of each triangle, by its shape key (without the unused low bits), through the
    /// FromSoftware parameters: each authored triangle's key and its material group. As CollisionSurvey's rebuild
    /// modes read them, which check them against the game's files.
    /// </summary>
    public static Dictionary<uint, uint> GetMaterialsByKey(object shape)
    {
        return shape switch
        {
            Havok.hk2014.fsnpCustomParamCompressedMeshShape s => MapMaterials(s, s.m_triangleIndexToShapeKey,
                s.m_pParam?.m_triangleDataArray.Select(t => t.m_primitiveDataIndex).ToList(),
                s.m_pParam?.m_primitiveDataArray.Select(p => p.m_materialNameData).ToList()),
            Havok.hk2016.fsnpCustomParamCompressedMeshShape s => MapMaterials(s, s.m_triangleIndexToShapeKey,
                s.m_pParam?.m_triangleDataArray.Select(t => t.m_primitiveDataIndex).ToList(),
                s.m_pParam?.m_primitiveDataArray.Select(p => p.m_materialNameData).ToList()),
            Havok.hk2018.fsnpCustomParamCompressedMeshShape s => MapMaterials(s, s.m_triangleIndexToShapeKey,
                s.m_pParam?.m_triangleDataArray.Select(t => t.m_primitiveDataIndex).ToList(),
                s.m_pParam?.m_primitiveDataArray.Select(p => p.m_materialNameData).ToList()),
            Havok.hk2019.fsnpCustomParamCompressedMeshShape s => MapMaterials(s, s.m_triangleIndexToShapeKey,
                s.m_pParam?.m_triangleDataArray.Select(t => t.m_primitiveDataIndex).ToList(),
                s.m_pParam?.m_primitiveDataArray.Select(p => p.m_materialNameData).ToList()),
            _ => new Dictionary<uint, uint>()
        };
    }

    private static Dictionary<uint, uint> MapMaterials(object shape, List<uint> triangleKeys, List<uint> triangleGroups,
        List<uint> groupMaterials)
    {
        var result = new Dictionary<uint, uint>();

        if (triangleGroups == null || groupMaterials == null)
            return result;

        var shift = 32 - ((IHavokCompressedMesh)shape).BitsPerKey;

        for (var i = 0; i < triangleKeys.Count && i < triangleGroups.Count; i++)
        {
            var group = (int)triangleGroups[i];

            if (group < groupMaterials.Count)
                result[triangleKeys[i] >> shift] = groupMaterials[group];
        }

        return result;
    }

    /// <summary>
    /// A new compressed mesh of the triangles, in the class set of the shape it replaces, keeping that shape's convex
    /// radius (and DS3's flags), user data and properties, with the key each triangle got. Null if the shape isn't
    /// supported.
    /// </summary>
    public static BuiltShape BuildShape(object oldShape, List<Vector3> vertices, List<int> indices, List<uint> materials)
    {
        var defaultMaterial = materials.FirstOrDefault();

        // The older games' (hkp) mesh: a storage per material, copied from the old shape's, and a compiled MOPP
        if (HkpMeshShapeRebuild.IsSupported(oldShape))
        {
            var hkp = HkpMeshShapeRebuild.BuildNew(oldShape, vertices, indices, materials, out var hkpKeys);
            return hkp == null ? null : new BuiltShape(hkp, hkpKeys);
        }

        if (!CanReplaceShape(oldShape))
            return null;

        // A mesh keeps the convex radius it had; a convex or box shape's radius is its own rounding, not a mesh's
        var radius = !IsDecodableMeshShape(oldShape)
            ? oldShape is Havok.hk2014.hknpShape convex2014 ? GetVariant2014(convex2014).ConvexRadius : HavokCompressedMeshBuilder.DefaultConvexRadius
            : oldShape switch
        {
            Havok.hk2014.hknpShape s => s.m_convexRadius,
            Havok.hk2016.hknpShape s => s.m_convexRadius,
            Havok.hk2018.hknpShape s => s.m_convexRadius,
            Havok.hk2019.hknpShape s => s.m_convexRadius,
            _ => HavokCompressedMeshBuilder.DefaultConvexRadius
        };

        var build = HavokCompressedMeshBuilder.Build(vertices, indices, materials, defaultMaterial, radius);
        var keys = Enumerable.Range(0, build.TriangleKeys.Length).Select(build.StoredKey).ToArray();

        object shape;

        // By the old shape's class set: a FromSoftware mesh, a plain one, or a convex or box shape it replaces
        switch (oldShape)
        {
            case Havok.hk2014.hknpShape old:
            {
                // A mesh keeps its flags. A convex shape's describe a convex, so the mesh gets DS3's or BB's (see
                // HavokCompressedMeshGenerator_hk2014.Variant), by whether the shape has the SPU flag, as DS3's do
                var built = IsDecodableMeshShape(old)
                    ? HavokCompressedMeshGenerator_hk2014.ToShape(build, old.m_flags)
                    : HavokCompressedMeshGenerator_hk2014.ToShape(build, GetVariant2014(old).Flags);
                built.m_userData = old.m_userData;
                built.m_properties = old.m_properties;
                shape = built;
                break;
            }
            case Havok.hk2016.hknpShape old:
            {
                var built = Havok.hk2016.Builders.CollisionShapeBuilder.ToShape(build);
                built.m_userData = old.m_userData;
                built.m_properties = old.m_properties;
                shape = built;
                break;
            }
            case Havok.hk2018.hknpShape old:
            {
                var built = Havok.hk2018.Builders.CollisionShapeBuilder.ToShape(build);
                built.m_userData = old.m_userData;
                built.m_properties = old.m_properties;
                shape = built;
                break;
            }
            case Havok.hk2019.hknpShape old:
            {
                var built = Havok.hk2019.Builders.CollisionShapeBuilder.ToShape(build);
                built.m_userData = old.m_userData;
                built.m_properties = old.m_properties;
                shape = built;
                break;
            }
            default:
                return null;
        }

        return new BuiltShape(shape, keys);
    }

    /// <summary>The hk2014 mesh settings for a shape's platform: DS3's when it has the SPU flag, else BB's.</summary>
    private static HavokCompressedMeshGenerator_hk2014.Variant GetVariant2014(Havok.hk2014.hknpShape shape)
    {
        return shape.m_flags.HasFlag(Havok.hk2014.hknpShape.FlagsEnum.NO_GET_SHAPE_KEYS_ON_SPU)
            ? HavokCompressedMeshGenerator_hk2014.Variant.DS3
            : HavokCompressedMeshGenerator_hk2014.Variant.BB;
    }

    /// <summary>
    /// Whether a document can be the base of a new collision: a single body, whose shape is a supported compressed
    /// mesh, so it can be replaced by the new mesh.
    /// </summary>
    public static bool IsTemplate(Havok.HavokDocument document)
    {
        var bodies = CollisionGeometry.Bodies(document.Objects).ToList();

        return bodies.Count == 1 && IsSupportedShape(bodies[0].Shape);
    }

    /// <summary>
    /// The body of a document that can be the base of a new collision (see IsTemplate), or null if it can't.
    /// </summary>
    public static IHavokBody GetTemplateBody(Havok.HavokDocument document)
    {
        return IsTemplate(document) ? CollisionGeometry.Bodies(document.Objects).First() : null;
    }

    /// <summary>
    /// Turns a freshly read template document into a new collision of the given triangles: its body gets a mesh built
    /// in its class set, is placed at the origin unrotated, and is named for the new model (the template's name with
    /// its model number replaced, as the game names a file's body after it). The older games' (hkp) bodies are named
    /// bodyName (their fixed bodies are already at the origin).
    /// </summary>
    public static bool MakeNewCollision(Havok.HavokDocument document, string modelNumber,
        List<Vector3> vertices, List<int> indices, List<uint> materials, string bodyName = null)
    {
        if (!IsTemplate(document))
            return false;

        var body = CollisionGeometry.Bodies(document.Objects).First();
        var built = BuildShape(body.Shape, vertices, indices, materials);

        if (built == null || !SetShape(body, built.Shape))
            return false;

        if (HkpMeshShapeRebuild.GetBodyName(body) != null)
            return bodyName == null || HkpMeshShapeRebuild.SetBodyName(body, bodyName);

        switch (body)
        {
            case Havok.hk2014.hknpBodyCinfo b:
                b.m_position = new Vector4(0, 0, 0, b.m_position.W);
                b.m_orientation = Quaternion.Identity;
                b.m_name = RenameBody(b.m_name, modelNumber);
                break;
            case Havok.hk2016.hknpBodyCinfo b:
                b.m_position = new Vector4(0, 0, 0, b.m_position.W);
                b.m_orientation = Quaternion.Identity;
                b.m_name = RenameBody(b.m_name, modelNumber);
                break;
            case Havok.hk2018.hknpBodyCinfo b:
                b.m_position = new Vector4(0, 0, 0, b.m_position.W);
                b.m_orientation = Quaternion.Identity;
                b.m_name = RenameBody(b.m_name, modelNumber);
                break;
            case Havok.hk2019.hknpBodyCinfo b:
                b.m_position = new Vector4(0, 0, 0, b.m_position.W);
                b.m_orientation = Quaternion.Identity;
                b.m_name = RenameBody(b.m_name, modelNumber);
                break;
        }

        return true;
    }

    // h000100 becomes h001234: the model number is the name's last six characters
    private static string RenameBody(string name, string modelNumber)
    {
        if (string.IsNullOrEmpty(name) || name.Length < modelNumber.Length)
            return name;

        var suffix = name.Substring(name.Length - modelNumber.Length);

        return suffix.All(char.IsDigit) ? name.Substring(0, name.Length - modelNumber.Length) + modelNumber : name;
    }

    /// <summary>Gives a body a shape of its own class set. False if they don't match.</summary>
    public static bool SetShape(IHavokBody body, object shape)
    {
        switch (body)
        {
            case Havok.hk2014.hknpBodyCinfo b when shape is Havok.hk2014.hknpShape s:
                b.m_shape = s;
                return true;
            case Havok.hk2016.hknpBodyCinfo b when shape is Havok.hk2016.hknpShape s:
                b.m_shape = s;
                return true;
            case Havok.hk2018.hknpBodyCinfo b when shape is Havok.hk2018.hknpShape s:
                b.m_shape = s;
                return true;
            case Havok.hk2019.hknpBodyCinfo b when shape is Havok.hk2019.hknpShape s:
                b.m_shape = s;
                return true;
        }

        // An older game's rigid body (hkp), whose collidable holds the shape
        return HkpMeshShapeRebuild.SetBodyShape(body, shape);
    }
}
