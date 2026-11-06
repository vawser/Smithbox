// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiStreamingSetVolumeConnectionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiStreamingSet.VolumeConnection>
{
    public hkaiStreamingSetVolumeConnectionData(Havok.hk2019.hkaiStreamingSet.VolumeConnection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aCellIndex":
            case "aCellIndex":
            {
                value = instance.m_aCellIndex;
                return true;
            }
            case "m_bCellIndex":
            case "bCellIndex":
            {
                value = instance.m_bCellIndex;
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
            case "m_aCellIndex":
            case "aCellIndex":
            {
                if (value is not int castValue) return false;
                instance.m_aCellIndex = castValue;
                return true;
            }
            case "m_bCellIndex":
            case "bCellIndex":
            {
                if (value is not int castValue) return false;
                instance.m_bCellIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
