// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiAvoidancePairPropertiesPairDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiAvoidancePairProperties.PairData>
{
    public hkaiAvoidancePairPropertiesPairDataData(Havok.hk2014.hkaiAvoidancePairProperties.PairData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_key":
            case "key":
            {
                value = instance.m_key;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
                return true;
            }
            case "m_cosViewAngle":
            case "cosViewAngle":
            {
                value = instance.m_cosViewAngle;
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
            case "m_key":
            case "key":
            {
                if (value is not uint castValue) return false;
                instance.m_key = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (value is not float castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_cosViewAngle":
            case "cosViewAngle":
            {
                if (value is not float castValue) return false;
                instance.m_cosViewAngle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
