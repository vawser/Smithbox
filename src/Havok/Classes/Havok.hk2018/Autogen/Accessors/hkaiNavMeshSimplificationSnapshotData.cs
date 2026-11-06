// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshSimplificationSnapshotData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshSimplificationSnapshot>
{
    public hkaiNavMeshSimplificationSnapshotData(Havok.hk2018.hkaiNavMeshSimplificationSnapshot instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometry":
            case "geometry":
            {
                value = instance.m_geometry;
                return true;
            }
            case "m_carvers":
            case "carvers":
            {
                value = instance.m_carvers;
                return true;
            }
            case "m_cuttingTriangles":
            case "cuttingTriangles":
            {
                value = instance.m_cuttingTriangles;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                value = instance.m_settings;
                return true;
            }
            case "m_unsimplifiedNavMesh":
            case "unsimplifiedNavMesh":
            {
                value = instance.m_unsimplifiedNavMesh;
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
            case "m_geometry":
            case "geometry":
            {
                if (value is null)
                {
                    instance.m_geometry = default;
                    return true;
                }
                if (value is Havok.hk2018.hkGeometry castValue)
                {
                    instance.m_geometry = castValue;
                    return true;
                }
                return false;
            }
            case "m_carvers":
            case "carvers":
            {
                if (value is not List<Havok.hk2018.hkaiCarver?> castValue) return false;
                instance.m_carvers = castValue;
                return true;
            }
            case "m_cuttingTriangles":
            case "cuttingTriangles":
            {
                if (value is not Havok.hk2018.hkBitField castValue) return false;
                instance.m_cuttingTriangles = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (value is not Havok.hk2018.hkaiNavMeshGenerationUtilsSettings castValue) return false;
                instance.m_settings = castValue;
                return true;
            }
            case "m_unsimplifiedNavMesh":
            case "unsimplifiedNavMesh":
            {
                if (value is null)
                {
                    instance.m_unsimplifiedNavMesh = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiNavMesh castValue)
                {
                    instance.m_unsimplifiedNavMesh = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
