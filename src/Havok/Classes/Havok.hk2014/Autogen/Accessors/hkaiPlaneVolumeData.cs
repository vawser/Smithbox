// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiPlaneVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiPlaneVolume>
{
    public hkaiPlaneVolumeData(Havok.hk2014.hkaiPlaneVolume instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_planes":
            case "planes":
            {
                value = instance.m_planes;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                value = instance.m_geometry;
                return true;
            }
            case "m_isInverted":
            case "isInverted":
            {
                value = instance.m_isInverted;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
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
            case "m_planes":
            case "planes":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_planes = castValue;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                if (value is not Havok.hk2014.hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            case "m_isInverted":
            case "isInverted":
            {
                if (value is not bool castValue) return false;
                instance.m_isInverted = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not Havok.hk2014.hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
