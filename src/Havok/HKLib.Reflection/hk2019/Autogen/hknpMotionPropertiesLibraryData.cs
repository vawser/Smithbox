// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpMotionPropertiesLibraryData : HavokData<HKLib.hk2019.hknpMotionPropertiesLibrary> 
{
    public hknpMotionPropertiesLibraryData(HavokType type, HKLib.hk2019.hknpMotionPropertiesLibrary instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_motionProperties":
            case "motionProperties":
            {
                if (instance.m_motionProperties is not TGet castValue) return false;
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
            case "m_motionProperties":
            case "motionProperties":
            {
                if (value is not HKLib.hk2019.hknpThreadSafeObjectPoolHknpMotionProperties castValue) return false;
                instance.m_motionProperties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
