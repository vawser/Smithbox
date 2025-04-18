// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbPoseStoringGeneratorOutputListenerData : HavokData<hkbPoseStoringGeneratorOutputListener> 
{
    public hkbPoseStoringGeneratorOutputListenerData(HavokType type, hkbPoseStoringGeneratorOutputListener instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_storedPoses":
            case "storedPoses":
            {
                if (instance.m_storedPoses is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dirty":
            case "dirty":
            {
                if (instance.m_dirty is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_storedPoses":
            case "storedPoses":
            {
                if (value is not List<hkbPoseStoringGeneratorOutputListener.StoredPose?> castValue) return false;
                instance.m_storedPoses = castValue;
                return true;
            }
            case "m_dirty":
            case "dirty":
            {
                if (value is not bool castValue) return false;
                instance.m_dirty = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
