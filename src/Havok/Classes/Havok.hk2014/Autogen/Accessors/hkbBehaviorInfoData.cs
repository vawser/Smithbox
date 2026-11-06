// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbBehaviorInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbBehaviorInfo>
{
    public hkbBehaviorInfoData(Havok.hk2014.hkbBehaviorInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_idToNamePairs":
            case "idToNamePairs":
            {
                value = instance.m_idToNamePairs;
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
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is null)
                {
                    instance.m_data = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbBehaviorGraphData castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            case "m_idToNamePairs":
            case "idToNamePairs":
            {
                if (value is not List<Havok.hk2014.hkbBehaviorInfo.IdToNamePair> castValue) return false;
                instance.m_idToNamePairs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
