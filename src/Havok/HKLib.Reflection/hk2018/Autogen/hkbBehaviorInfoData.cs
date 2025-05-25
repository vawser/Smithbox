// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBehaviorInfoData : HavokData<hkbBehaviorInfo> 
{
    public hkbBehaviorInfoData(HavokType type, hkbBehaviorInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterId":
            case "characterId":
            {
                if (instance.m_characterId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is null)
                {
                    return true;
                }
                if (instance.m_data is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_idToNamePairs":
            case "idToNamePairs":
            {
                if (instance.m_idToNamePairs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is HKLib.hk2018.hkbBehaviorGraphData castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            case "m_idToNamePairs":
            case "idToNamePairs":
            {
                if (value is not List<hkbBehaviorInfo.IdToNamePair> castValue) return false;
                instance.m_idToNamePairs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
