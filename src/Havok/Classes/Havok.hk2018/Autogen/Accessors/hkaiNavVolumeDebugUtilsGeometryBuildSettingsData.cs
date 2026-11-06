// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavVolumeDebugUtilsGeometryBuildSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavVolumeDebugUtils.GeometryBuildSettings>
{
    public hkaiNavVolumeDebugUtilsGeometryBuildSettingsData(Havok.hk2018.hkaiNavVolumeDebugUtils.GeometryBuildSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_skipBoundingFaces":
            case "skipBoundingFaces":
            {
                value = instance.m_skipBoundingFaces;
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
