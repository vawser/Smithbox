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

        if (!IsCompressedMeshShape(oldShape))
            return null;

        var radius = oldShape switch
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

        switch (oldShape)
        {
            case Havok.hk2014.fsnpCustomParamCompressedMeshShape old:
            {
                var built = HavokCompressedMeshGenerator_hk2014.ToShape(build, old.m_flags);
                built.m_userData = old.m_userData;
                built.m_properties = old.m_properties;
                shape = built;
                break;
            }
            case Havok.hk2016.fsnpCustomParamCompressedMeshShape old:
            {
                var built = Havok.hk2016.Builders.CollisionShapeBuilder.ToShape(build);
                built.m_userData = old.m_userData;
                built.m_properties = old.m_properties;
                shape = built;
                break;
            }
            case Havok.hk2018.fsnpCustomParamCompressedMeshShape old:
            {
                var built = Havok.hk2018.Builders.CollisionShapeBuilder.ToShape(build);
                built.m_userData = old.m_userData;
                built.m_properties = old.m_properties;
                shape = built;
                break;
            }
            case Havok.hk2019.fsnpCustomParamCompressedMeshShape old:
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
