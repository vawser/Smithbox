// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshSimplificationSnapshotData : HavokData<HKLib.hk2019.hkaiNavMeshSimplificationSnapshot> 
{
    public hkaiNavMeshSimplificationSnapshotData(HavokType type, HKLib.hk2019.hkaiNavMeshSimplificationSnapshot instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometry":
            case "geometry":
            {
                if (instance.m_geometry is null)
                {
                    return true;
                }
                if (instance.m_geometry is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_carvers":
            case "carvers":
            {
                if (instance.m_carvers is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cuttingTriangles":
            case "cuttingTriangles":
            {
                if (instance.m_cuttingTriangles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (instance.m_settings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_unsimplifiedNavMesh":
            case "unsimplifiedNavMesh":
            {
                if (instance.m_unsimplifiedNavMesh is null)
                {
                    return true;
                }
                if (instance.m_unsimplifiedNavMesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is HKLib.hk2019.hkGeometry castValue)
                {
                    instance.m_geometry = castValue;
                    return true;
                }
                return false;
            }
            case "m_carvers":
            case "carvers":
            {
                if (value is not List<HKLib.hk2019.hkaiCarver?> castValue) return false;
                instance.m_carvers = castValue;
                return true;
            }
            case "m_cuttingTriangles":
            case "cuttingTriangles":
            {
                if (value is not HKLib.hk2019.hkBitField castValue) return false;
                instance.m_cuttingTriangles = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings castValue) return false;
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
                if (value is HKLib.hk2019.hkaiNavMesh castValue)
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
