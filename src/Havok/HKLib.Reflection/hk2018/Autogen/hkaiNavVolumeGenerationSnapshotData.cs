// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavVolumeGenerationSnapshotData : HavokData<hkaiNavVolumeGenerationSnapshot> 
{
    public hkaiNavVolumeGenerationSnapshotData(HavokType type, hkaiNavVolumeGenerationSnapshot instance) : base(type, instance) {}

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
                if (value is not hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (value is not hkaiNavVolumeGenerationSettings castValue) return false;
                instance.m_settings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
