// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpRefMassDistributionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpRefMassDistribution>
{
    public hknpRefMassDistributionData(Havok.hk2019.hknpRefMassDistribution instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_massDistribution":
            case "massDistribution":
            {
                value = instance.m_massDistribution;
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
            case "m_massDistribution":
            case "massDistribution":
            {
                if (value is not Havok.hk2019.hknpMassDistribution castValue) return false;
                instance.m_massDistribution = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
