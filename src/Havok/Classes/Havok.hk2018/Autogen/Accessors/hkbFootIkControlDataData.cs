// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbFootIkControlDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbFootIkControlData>
{
    private static readonly System.Reflection.FieldInfo _enabledInfo = typeof(Havok.hk2018.hkbFootIkControlData).GetField("m_enabled")!;
    public hkbFootIkControlDataData(Havok.hk2018.hkbFootIkControlData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_gains":
            case "gains":
            {
                value = instance.m_gains;
                return true;
            }
            case "m_enabled":
            case "enabled":
            {
                value = instance.m_enabled;
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
            case "m_gains":
            case "gains":
            {
                if (value is not Havok.hk2018.hkbFootIkGains castValue) return false;
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
