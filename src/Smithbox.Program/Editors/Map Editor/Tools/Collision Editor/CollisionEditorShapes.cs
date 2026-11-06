using Havok.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The parts of the Collision Editor that depend on the Havok class set: the FromSoftware compressed meshes of
/// hk2018 (ER, NR) and hk2019 (AC6), which the shared CollisionShapeBuilder builds.
/// </summary>
public static class CollisionEditorShapes
{
    public static bool IsSupportedShape(object shape)
    {
        return shape is Havok.hk2018.fsnpCustomParamCompressedMeshShape
            or Havok.hk2019.fsnpCustomParamCompressedMeshShape;
    }

    /// <summary>
    /// The collision material of each triangle, by its shape key (without the unused low bits), through the
    /// FromSoftware parameters: each authored triangle's key and its material group. As CollisionSurvey's rebuild
    /// mode reads them, which checks them against the game's files.
    /// </summary>
    public static Dictionary<uint, uint> GetMaterialsByKey(object shape)
    {
        var result = new Dictionary<uint, uint>();

        switch (shape)
        {
            case Havok.hk2018.fsnpCustomParamCompressedMeshShape s:
            {
                var bits = ((IHavokCompressedMesh)s).BitsPerKey;
                var triangles = s.m_pParam?.m_triangleDataArray;
                var groups = s.m_pParam?.m_primitiveDataArray;

                if (triangles == null || groups == null)
                    break;

                for (var i = 0; i < s.m_triangleIndexToShapeKey.Count && i < triangles.Count; i++)
                {
                    var group = (int)triangles[i].m_primitiveDataIndex;

                    if (group < groups.Count)
                        result[s.m_triangleIndexToShapeKey[i] >> (32 - bits)] = groups[group].m_materialNameData;
                }

                break;
            }
            case Havok.hk2019.fsnpCustomParamCompressedMeshShape s:
            {
                var bits = ((IHavokCompressedMesh)s).BitsPerKey;
                var triangles = s.m_pParam?.m_triangleDataArray;
                var groups = s.m_pParam?.m_primitiveDataArray;

                if (triangles == null || groups == null)
                    break;

                for (var i = 0; i < s.m_triangleIndexToShapeKey.Count && i < triangles.Count; i++)
                {
                    var group = (int)triangles[i].m_primitiveDataIndex;

                    if (group < groups.Count)
                        result[s.m_triangleIndexToShapeKey[i] >> (32 - bits)] = groups[group].m_materialNameData;
                }

                break;
            }
        }

        return result;
    }

    /// <summary>
    /// A new compressed mesh of the triangles, in the class set of the shape it replaces, keeping that shape's convex
    /// radius, user data and properties. Null if the shape isn't supported.
    /// </summary>
    public static object BuildShape(object oldShape, List<Vector3> vertices, List<int> indices, List<uint> materials)
    {
        var defaultMaterial = materials.FirstOrDefault();

        switch (oldShape)
        {
            case Havok.hk2018.fsnpCustomParamCompressedMeshShape old:
            {
                var shape = Havok.hk2018.Builders.CollisionShapeBuilder.BuildCompressedMeshShape(
                    vertices, indices, materials, defaultMaterial, old.m_convexRadius);

                shape.m_userData = old.m_userData;
                shape.m_properties = old.m_properties;

                return shape;
            }
            case Havok.hk2019.fsnpCustomParamCompressedMeshShape old:
            {
                var shape = Havok.hk2019.Builders.CollisionShapeBuilder.BuildCompressedMeshShape(
                    vertices, indices, materials, defaultMaterial, old.m_convexRadius);

                shape.m_userData = old.m_userData;
                shape.m_properties = old.m_properties;

                return shape;
            }
        }

        return null;
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
    /// Turns a freshly read template document into a new collision of the given triangles: its body gets a mesh built
    /// in its class set, is placed at the origin unrotated, and is named for the new model (the template's name with
    /// its model number replaced, as the game names a file's body after it).
    /// </summary>
    public static bool MakeNewCollision(Havok.HavokDocument document, string modelNumber,
        List<Vector3> vertices, List<int> indices, List<uint> materials)
    {
        if (!IsTemplate(document))
            return false;

        var body = CollisionGeometry.Bodies(document.Objects).First();
        var shape = BuildShape(body.Shape, vertices, indices, materials);

        if (shape == null || !SetShape(body, shape))
            return false;

        switch (body)
        {
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
            case Havok.hk2018.hknpBodyCinfo b when shape is Havok.hk2018.hknpShape s:
                b.m_shape = s;
                return true;
            case Havok.hk2019.hknpBodyCinfo b when shape is Havok.hk2019.hknpShape s:
                b.m_shape = s;
                return true;
        }

        return false;
    }
}
