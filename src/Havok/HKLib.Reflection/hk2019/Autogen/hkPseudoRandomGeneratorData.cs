// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkPseudoRandomGeneratorData : HavokData<HKLib.hk2019.hkPseudoRandomGenerator> 
{
    public hkPseudoRandomGeneratorData(HavokType type, HKLib.hk2019.hkPseudoRandomGenerator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_seed":
            case "seed":
            {
                if (instance.m_seed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_current":
            case "current":
            {
                if (instance.m_current is not TGet castValue) return false;
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
            case "m_seed":
            case "seed":
            {
                if (value is not uint castValue) return false;
                instance.m_seed = castValue;
                return true;
            }
            case "m_current":
            case "current":
            {
                if (value is not uint castValue) return false;
                instance.m_current = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
