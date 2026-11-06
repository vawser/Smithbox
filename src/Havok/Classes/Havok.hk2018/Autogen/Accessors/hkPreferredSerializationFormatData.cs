// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkPreferredSerializationFormatData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.PreferredSerializationFormat>
{
    public hkPreferredSerializationFormatData(Havok.hk2018.hk.PreferredSerializationFormat instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                value = instance.m_value;
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
            case "m_value":
            case "value":
            {
                if (value is Havok.hk2018.hk.PreferredSerializationFormat.Format castValue)
                {
                    instance.m_value = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_value = (Havok.hk2018.hk.PreferredSerializationFormat.Format)uintValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
