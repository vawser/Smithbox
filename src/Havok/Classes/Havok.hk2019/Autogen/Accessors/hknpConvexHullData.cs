// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpConvexHullData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpConvexHull>
{
    public hknpConvexHullData(Havok.hk2019.hknpConvexHull instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
                return true;
            }
            case "m_planes":
            case "planes":
            {
                value = instance.m_planes;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                value = instance.m_faces;
                return true;
            }
            case "m_indices":
            case "indices":
            {
                value = instance.m_indices;
                return true;
            }
            case "m_faceLinks":
            case "faceLinks":
            {
                value = instance.m_faceLinks;
                return true;
            }
            case "m_vertexEdges":
            case "vertexEdges":
            {
                value = instance.m_vertexEdges;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<Havok.hk2019.hkFloat3> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            case "m_planes":
            case "planes":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_planes = castValue;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                if (value is not List<Havok.hk2019.hknpConvexHull.Face> castValue) return false;
                instance.m_faces = castValue;
                return true;
            }
            case "m_indices":
            case "indices":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_indices = castValue;
                return true;
            }
            case "m_faceLinks":
            case "faceLinks":
            {
                if (value is not List<Havok.hk2019.hknpConvexHull.Edge> castValue) return false;
                instance.m_faceLinks = castValue;
                return true;
            }
            case "m_vertexEdges":
            case "vertexEdges":
            {
                if (value is not List<Havok.hk2019.hknpConvexHull.Edge> castValue) return false;
                instance.m_vertexEdges = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
