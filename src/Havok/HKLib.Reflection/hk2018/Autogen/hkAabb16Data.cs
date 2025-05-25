// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkAabb16Data : HavokData<hkAabb16> 
{
    private static readonly System.Reflection.FieldInfo _minInfo = typeof(hkAabb16).GetField("m_min")!;
    private static readonly System.Reflection.FieldInfo _maxInfo = typeof(hkAabb16).GetField("m_max")!;
    public hkAabb16Data(HavokType type, hkAabb16 instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_min":
            case "min":
            {
                if (instance.m_min is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_key":
            case "key":
            {
                if (instance.m_key is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_max":
            case "max":
            {
                if (instance.m_max is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_key1":
            case "key1":
            {
                if (instance.m_key1 is not TGet castValue) return false;
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
            case "m_min":
            case "min":
            {
                if (value is not ushort[] castValue || castValue.Length != 3) return false;
                try
                {
                    _minInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_key":
            case "key":
            {
                if (value is not ushort castValue) return false;
                instance.m_key = castValue;
                return true;
            }
            case "m_max":
            case "max":
            {
                if (value is not ushort[] castValue || castValue.Length != 3) return false;
                try
                {
                    _maxInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_key1":
            case "key1":
            {
                if (value is not ushort castValue) return false;
                instance.m_key1 = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
