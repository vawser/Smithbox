// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiIntersectionVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiIntersectionVolume>
{
    public hkaiIntersectionVolumeData(Havok.hk2018.hkaiIntersectionVolume instance) : base(instance) { }

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
            case "m_volumes":
            case "volumes":
            {
                value = instance.m_volumes;
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
            case "m_volumes":
            case "volumes":
            {
                if (value is not List<Havok.hk2018.hkaiVolume?> castValue) return false;
                instance.m_volumes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
