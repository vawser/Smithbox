// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpRefMassDistributionData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpRefMassDistribution>
{
    public hknpRefMassDistributionData(Havok.hk2016.hknpRefMassDistribution instance) : base(instance) { }

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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_massDistribution":
            case "massDistribution":
            {
                if (value is not Havok.hk2016.hknpMassDistribution castValue) return false;
                instance.m_massDistribution = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
