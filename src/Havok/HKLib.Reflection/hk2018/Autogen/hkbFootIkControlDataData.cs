// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbFootIkControlDataData : HavokData<hkbFootIkControlData> 
{
    private static readonly System.Reflection.FieldInfo _enabledInfo = typeof(hkbFootIkControlData).GetField("m_enabled")!;
    public hkbFootIkControlDataData(HavokType type, hkbFootIkControlData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_gains":
            case "gains":
            {
                if (instance.m_gains is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enabled":
            case "enabled":
            {
                if (instance.m_enabled is not TGet castValue) return false;
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
            case "m_gains":
            case "gains":
            {
                if (value is not hkbFootIkGains castValue) return false;
                instance.m_gains = castValue;
                return true;
            }
            case "m_enabled":
            case "enabled":
            {
                if (value is not float[] castValue || castValue.Length != 32) return false;
                try
                {
                    _enabledInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
