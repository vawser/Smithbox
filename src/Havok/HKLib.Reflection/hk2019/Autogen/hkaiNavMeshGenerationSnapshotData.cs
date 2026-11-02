// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshotData : HavokData<HKLib.hk2019.hkaiNavMeshGenerationSnapshot> 
{
    public hkaiNavMeshGenerationSnapshotData(HavokType type, HKLib.hk2019.hkaiNavMeshGenerationSnapshot instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometry":
            case "geometry":
            {
                if (instance.m_geometry is not TGet castValue) return false;
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
                if (value is not HKLib.hk2019.hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings castValue) return false;
                instance.m_settings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
