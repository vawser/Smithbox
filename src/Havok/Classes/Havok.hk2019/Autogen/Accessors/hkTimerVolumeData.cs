// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkTimerVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkTimerVolume>
{
    public hkTimerVolumeData(Havok.hk2019.hkTimerVolume instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabbMin":
            case "aabbMin":
            {
                value = instance.m_aabbMin;
                return true;
            }
            case "m_aabbMax":
            case "aabbMax":
            {
                value = instance.m_aabbMax;
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
            case "m_aabbMin":
            case "aabbMin":
            {
                if (value is not Havok.hk2019.hkFloat3 castValue) return false;
                instance.m_aabbMin = castValue;
                return true;
            }
            case "m_aabbMax":
            case "aabbMax":
            {
                if (value is not Havok.hk2019.hkFloat3 castValue) return false;
                instance.m_aabbMax = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
