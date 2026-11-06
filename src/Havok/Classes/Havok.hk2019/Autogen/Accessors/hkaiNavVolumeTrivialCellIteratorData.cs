// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavVolumeTrivialCellIteratorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavVolumeTrivialCellIterator>
{
    public hkaiNavVolumeTrivialCellIteratorData(Havok.hk2019.hkaiNavVolumeTrivialCellIterator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_navVolume":
            case "navVolume":
            {
                value = instance.m_navVolume;
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
            case "m_navVolume":
            case "navVolume":
            {
                if (value is null)
                {
                    instance.m_navVolume = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavVolume castValue)
                {
                    instance.m_navVolume = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
