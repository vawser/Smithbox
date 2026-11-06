// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxNodeLimitsLimit_2Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxNodeLimits.Limit_2>
{
    private static readonly System.Reflection.FieldInfo _valuesInfo = typeof(Havok.hk2019.hkxNodeLimits.Limit_2).GetField("m_values")!;
    public hkxNodeLimitsLimit_2Data(Havok.hk2019.hkxNodeLimits.Limit_2 instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_values":
            case "values":
            {
                value = instance.m_values;
                return true;
            }
            case "m_active":
            case "active":
            {
                value = instance.m_active;
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
            case "m_values":
            case "values":
            {
                if (value is not float[] castValue || castValue.Length != 6) return false;
                try
                {
                    _valuesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_active":
            case "active":
            {
                if (value is not byte castValue) return false;
                instance.m_active = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
