// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxAnimationDataData : HavokData<HKLib.hk2019.hkxAnimationData> 
{
    public hkxAnimationDataData(HavokType type, HKLib.hk2019.hkxAnimationData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_layers":
            case "layers":
            {
                if (instance.m_layers is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_info":
            case "info":
            {
                if (instance.m_info is not TGet castValue) return false;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_layers":
            case "layers":
            {
                if (value is not List<HKLib.hk2019.hkxAnimationData.Layer_2> castValue) return false;
                instance.m_layers = castValue;
                return true;
            }
            case "m_info":
            case "info":
            {
                if (value is not HKLib.hk2019.hkxAnimationData.AnimationInfo castValue) return false;
                instance.m_info = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
