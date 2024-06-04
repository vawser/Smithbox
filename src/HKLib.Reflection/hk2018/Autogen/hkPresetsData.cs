// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkPresetsData : HavokData<Presets> 
{
    public hkPresetsData(HavokType type, Presets instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_strict":
            case "strict":
            {
                if (instance.m_strict is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_valueType":
            case "valueType":
            {
                if (instance.m_valueType is null)
                {
                    return true;
                }
                if (instance.m_valueType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_numPresets":
            case "numPresets":
            {
                if (instance.m_numPresets is not TGet castValue) return false;
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
            case "m_strict":
            case "strict":
            {
                if (value is not bool castValue) return false;
                instance.m_strict = castValue;
                return true;
            }
            case "m_valueType":
            case "valueType":
            {
                if (value is null)
                {
                    instance.m_valueType = default;
                    return true;
                }
                if (value is IHavokObject castValue)
                {
                    instance.m_valueType = castValue;
                    return true;
                }
                return false;
            }
            case "m_numPresets":
            case "numPresets":
            {
                if (value is not int castValue) return false;
                instance.m_numPresets = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
