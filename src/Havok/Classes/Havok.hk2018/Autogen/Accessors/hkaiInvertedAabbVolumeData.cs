// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiInvertedAabbVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiInvertedAabbVolume>
{
    public hkaiInvertedAabbVolumeData(Havok.hk2018.hkaiInvertedAabbVolume instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                value = instance.m_geometry;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                if (value is not Havok.hk2018.hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
