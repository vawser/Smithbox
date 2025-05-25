// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMotionPropertiesIdData : HavokData<hknpMotionPropertiesId> 
{
    public hknpMotionPropertiesIdData(HavokType type, hknpMotionPropertiesId instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                if (instance.m_value is not TGet castValue) return false;
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
            case "m_value":
            case "value":
            {
                if (value is not ushort castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
