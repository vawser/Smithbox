// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiAvoidancePairPropertiesData : HavokData<hkaiAvoidancePairProperties> 
{
    public hkaiAvoidancePairPropertiesData(HavokType type, hkaiAvoidancePairProperties instance) : base(type, instance) {}

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
            case "m_avoidancePairDataMap":
            case "avoidancePairDataMap":
            {
                if (instance.m_avoidancePairDataMap is not TGet castValue) return false;
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
            case "m_avoidancePairDataMap":
            case "avoidancePairDataMap":
            {
                if (value is not List<hkaiAvoidancePairProperties.PairData> castValue) return false;
                instance.m_avoidancePairDataMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
