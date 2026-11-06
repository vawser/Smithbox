// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiAvoidancePairPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiAvoidancePairProperties>
{
    public hkaiAvoidancePairPropertiesData(Havok.hk2014.hkaiAvoidancePairProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_avoidancePairDataMap":
            case "avoidancePairDataMap":
            {
                if (value is not List<Havok.hk2014.hkaiAvoidancePairProperties.PairData> castValue) return false;
                instance.m_avoidancePairDataMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
