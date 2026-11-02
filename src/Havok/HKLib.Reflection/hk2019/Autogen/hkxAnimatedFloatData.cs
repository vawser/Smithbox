// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxAnimatedFloatData : HavokData<HKLib.hk2019.hkxAnimatedFloat> 
{
    public hkxAnimatedFloatData(HavokType type, HKLib.hk2019.hkxAnimatedFloat instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_floats":
            case "floats":
            {
                if (instance.m_floats is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hint":
            case "hint":
            {
                if (instance.m_hint is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_hint is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_floats":
            case "floats":
            {
                if (value is not List<float> castValue) return false;
                instance.m_floats = castValue;
                return true;
            }
            case "m_hint":
            case "hint":
            {
                if (value is HKLib.hk2019.hkxAttribute.Hint castValue)
                {
                    instance.m_hint = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_hint = (HKLib.hk2019.hkxAttribute.Hint)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
