// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiAvoidancePairPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiAvoidancePairProperties>
{
    public hkaiAvoidancePairPropertiesData(Havok.hk2018.hkaiAvoidancePairProperties instance) : base(instance) { }

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
            case "m_avoidancePairDataMap":
            case "avoidancePairDataMap":
            {
                value = instance.m_avoidancePairDataMap;
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
            case "m_avoidancePairDataMap":
            case "avoidancePairDataMap":
            {
                if (value is not List<Havok.hk2018.hkaiAvoidancePairProperties.PairData> castValue) return false;
                instance.m_avoidancePairDataMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
