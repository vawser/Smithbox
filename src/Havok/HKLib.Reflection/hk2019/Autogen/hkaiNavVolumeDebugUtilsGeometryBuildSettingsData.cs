// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavVolumeDebugUtilsGeometryBuildSettingsData : HavokData<HKLib.hk2019.hkaiNavVolumeDebugUtils.GeometryBuildSettings> 
{
    public hkaiNavVolumeDebugUtilsGeometryBuildSettingsData(HavokType type, HKLib.hk2019.hkaiNavVolumeDebugUtils.GeometryBuildSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_skipBoundingFaces":
            case "skipBoundingFaces":
            {
                if (instance.m_skipBoundingFaces is not TGet castValue) return false;
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
            case "m_skipBoundingFaces":
            case "skipBoundingFaces":
            {
                if (value is not bool castValue) return false;
                instance.m_skipBoundingFaces = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
