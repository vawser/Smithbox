// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkAabb16Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkAabb16>
{
    private static readonly System.Reflection.FieldInfo _minInfo = typeof(Havok.hk2019.hkAabb16).GetField("m_min")!;
    private static readonly System.Reflection.FieldInfo _maxInfo = typeof(Havok.hk2019.hkAabb16).GetField("m_max")!;
    public hkAabb16Data(Havok.hk2019.hkAabb16 instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_min":
            case "min":
            {
                value = instance.m_min;
                return true;
            }
            case "m_key":
            case "key":
            {
                value = instance.m_key;
                return true;
            }
            case "m_max":
            case "max":
            {
                value = instance.m_max;
                return true;
            }
            case "m_key1":
            case "key1":
            {
                value = instance.m_key1;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
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
