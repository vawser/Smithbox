// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiIntersectionVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiIntersectionVolume>
{
    public hkaiIntersectionVolumeData(Havok.hk2019.hkaiIntersectionVolume instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_volumes":
            case "volumes":
            {
                if (value is not List<Havok.hk2019.hkaiVolume?> castValue) return false;
                instance.m_volumes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
