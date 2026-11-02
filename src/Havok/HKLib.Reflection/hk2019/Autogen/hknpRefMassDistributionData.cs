// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpRefMassDistributionData : HavokData<HKLib.hk2019.hknpRefMassDistribution> 
{
    public hknpRefMassDistributionData(HavokType type, HKLib.hk2019.hknpRefMassDistribution instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_massDistribution":
            case "massDistribution":
            {
                if (instance.m_massDistribution is not TGet castValue) return false;
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
            case "m_massDistribution":
            case "massDistribution":
            {
                if (value is not HKLib.hk2019.hknpMassDistribution castValue) return false;
                instance.m_massDistribution = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
