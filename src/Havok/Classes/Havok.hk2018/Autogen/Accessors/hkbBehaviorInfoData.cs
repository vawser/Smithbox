// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbBehaviorInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbBehaviorInfo>
{
    public hkbBehaviorInfoData(Havok.hk2018.hkbBehaviorInfo instance) : base(instance) { }

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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
                if (value is Havok.hk2018.hkbBehaviorGraphData castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            case "m_idToNamePairs":
            case "idToNamePairs":
            {
                if (value is not List<Havok.hk2018.hkbBehaviorInfo.IdToNamePair> castValue) return false;
                instance.m_idToNamePairs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
