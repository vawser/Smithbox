// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbClipGeneratorEchoData : HavokData<hkbClipGenerator.Echo> 
{
    public hkbClipGeneratorEchoData(HavokType type, hkbClipGenerator.Echo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_offsetLocalTime":
            case "offsetLocalTime":
            {
                if (instance.m_offsetLocalTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (instance.m_weight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dwdt":
            case "dwdt":
            {
                if (instance.m_dwdt is not TGet castValue) return false;
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
            case "m_offsetLocalTime":
            case "offsetLocalTime":
            {
                if (value is not float castValue) return false;
                instance.m_offsetLocalTime = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (value is not float castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_dwdt":
            case "dwdt":
            {
                if (value is not float castValue) return false;
                instance.m_dwdt = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
