// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpSerializedSubTrack1nInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpSerializedSubTrack1nInfo>
{
    public hkpSerializedSubTrack1nInfoData(Havok.hk2014.hkpSerializedSubTrack1nInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sectors":
            case "sectors":
            {
                value = instance.m_sectors;
                return true;
            }
            case "m_subTracks":
            case "subTracks":
            {
                value = instance.m_subTracks;
                return true;
            }
            case "m_sectorIndex":
            case "sectorIndex":
            {
                value = instance.m_sectorIndex;
                return true;
            }
            case "m_offsetInSector":
            case "offsetInSector":
            {
                value = instance.m_offsetInSector;
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
            case "m_sectors":
            case "sectors":
            {
                if (value is not List<Havok.hk2014.hkpAgent1nSector?> castValue) return false;
                instance.m_sectors = castValue;
                return true;
            }
            case "m_subTracks":
            case "subTracks":
            {
                if (value is not List<Havok.hk2014.hkpSerializedSubTrack1nInfo?> castValue) return false;
                instance.m_subTracks = castValue;
                return true;
            }
            case "m_sectorIndex":
            case "sectorIndex":
            {
                if (value is not int castValue) return false;
                instance.m_sectorIndex = castValue;
                return true;
            }
            case "m_offsetInSector":
            case "offsetInSector":
            {
                if (value is not int castValue) return false;
                instance.m_offsetInSector = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
