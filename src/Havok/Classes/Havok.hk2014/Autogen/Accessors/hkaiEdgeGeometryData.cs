// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiEdgeGeometryData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiEdgeGeometry>
{
    public hkaiEdgeGeometryData(Havok.hk2014.hkaiEdgeGeometry instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edges":
            case "edges":
            {
                value = instance.m_edges;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                value = instance.m_faces;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
                return true;
            }
            case "m_zeroFace":
            case "zeroFace":
            {
                value = instance.m_zeroFace;
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
            case "m_edges":
            case "edges":
            {
                if (value is not List<Havok.hk2014.hkaiEdgeGeometry.Edge> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                if (value is not List<Havok.hk2014.hkaiEdgeGeometry.Face> castValue) return false;
                instance.m_faces = castValue;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            case "m_zeroFace":
            case "zeroFace":
            {
                if (value is not Havok.hk2014.hkaiEdgeGeometry.Face castValue) return false;
                instance.m_zeroFace = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
