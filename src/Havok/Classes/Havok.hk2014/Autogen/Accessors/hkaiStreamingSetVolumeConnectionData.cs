// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiStreamingSetVolumeConnectionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiStreamingSet.VolumeConnection>
{
    public hkaiStreamingSetVolumeConnectionData(Havok.hk2014.hkaiStreamingSet.VolumeConnection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cellIndex":
            case "cellIndex":
            {
                value = instance.m_cellIndex;
                return true;
            }
            case "m_oppositeCellIndex":
            case "oppositeCellIndex":
            {
                value = instance.m_oppositeCellIndex;
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
            case "m_cellIndex":
            case "cellIndex":
            {
                if (value is not int castValue) return false;
                instance.m_cellIndex = castValue;
                return true;
            }
            case "m_oppositeCellIndex":
            case "oppositeCellIndex":
            {
                if (value is not int castValue) return false;
                instance.m_oppositeCellIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
