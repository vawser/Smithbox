// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpMassDistributionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpMassDistribution>
{
    public hknpMassDistributionData(Havok.hk2019.hknpMassDistribution instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_centerOfMassAndVolume":
            case "centerOfMassAndVolume":
            {
                value = instance.m_centerOfMassAndVolume;
                return true;
            }
            case "m_majorAxisSpace":
            case "majorAxisSpace":
            {
                value = instance.m_majorAxisSpace;
                return true;
            }
            case "m_inertiaTensor":
            case "inertiaTensor":
            {
                value = instance.m_inertiaTensor;
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
            case "m_centerOfMassAndVolume":
            case "centerOfMassAndVolume":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_centerOfMassAndVolume = castValue;
                return true;
            }
            case "m_majorAxisSpace":
            case "majorAxisSpace":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_majorAxisSpace = castValue;
                return true;
            }
            case "m_inertiaTensor":
            case "inertiaTensor":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_inertiaTensor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
