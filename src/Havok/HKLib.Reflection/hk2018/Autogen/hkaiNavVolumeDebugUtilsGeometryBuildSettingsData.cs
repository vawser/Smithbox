// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiNavVolumeDebugUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavVolumeDebugUtilsGeometryBuildSettingsData : HavokData<GeometryBuildSettings> 
{
    public hkaiNavVolumeDebugUtilsGeometryBuildSettingsData(HavokType type, GeometryBuildSettings instance) : base(type, instance) {}

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
