// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiAvoidancePairPropertiesPairDataData : HavokData<hkaiAvoidancePairProperties.PairData> 
{
    public hkaiAvoidancePairPropertiesPairDataData(HavokType type, hkaiAvoidancePairProperties.PairData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_key":
            case "key":
            {
                if (instance.m_key is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (instance.m_weight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cosViewAngle":
            case "cosViewAngle":
            {
                if (instance.m_cosViewAngle is not TGet castValue) return false;
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
