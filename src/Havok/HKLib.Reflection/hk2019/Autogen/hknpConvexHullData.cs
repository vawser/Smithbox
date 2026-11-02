// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpConvexHullData : HavokData<HKLib.hk2019.hknpConvexHull> 
{
    public hknpConvexHullData(HavokType type, HKLib.hk2019.hknpConvexHull instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertices":
            case "vertices":
            {
                if (instance.m_vertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_planes":
            case "planes":
            {
                if (instance.m_planes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                if (instance.m_faces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indices":
            case "indices":
            {
                if (instance.m_indices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceLinks":
            case "faceLinks":
            {
                if (instance.m_faceLinks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexEdges":
            case "vertexEdges":
            {
                if (instance.m_vertexEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<HKLib.hk2019.hkFloat3> castValue) return false;
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
                if (value is not List<HKLib.hk2019.hknpConvexHull.Face> castValue) return false;
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
                if (value is not List<HKLib.hk2019.hknpConvexHull.Edge> castValue) return false;
                instance.m_faceLinks = castValue;
                return true;
            }
            case "m_vertexEdges":
            case "vertexEdges":
            {
                if (value is not List<HKLib.hk2019.hknpConvexHull.Edge> castValue) return false;
                instance.m_vertexEdges = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
