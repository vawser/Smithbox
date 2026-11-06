// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiNavVolumeGenerationSnapshotData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiNavVolumeGenerationSnapshot>
{
    public hkaiNavVolumeGenerationSnapshotData(Havok.hk2014.hkaiNavVolumeGenerationSnapshot instance) : base(instance) { }

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
            case "m_settings":
            case "settings":
            {
                value = instance.m_settings;
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
                if (value is not Havok.hk2014.hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (value is not Havok.hk2014.hkaiNavVolumeGenerationSettings castValue) return false;
                instance.m_settings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
