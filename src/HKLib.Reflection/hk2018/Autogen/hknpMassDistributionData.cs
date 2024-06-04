// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMassDistributionData : HavokData<hknpMassDistribution> 
{
    public hknpMassDistributionData(HavokType type, hknpMassDistribution instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_centerOfMassAndVolume":
            case "centerOfMassAndVolume":
            {
                if (instance.m_centerOfMassAndVolume is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_majorAxisSpace":
            case "majorAxisSpace":
            {
                if (instance.m_majorAxisSpace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inertiaTensor":
            case "inertiaTensor":
            {
                if (instance.m_inertiaTensor is not TGet castValue) return false;
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
