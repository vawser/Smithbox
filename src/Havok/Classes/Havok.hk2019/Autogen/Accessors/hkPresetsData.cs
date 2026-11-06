// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkPresetsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hk.Presets>
{
    public hkPresetsData(Havok.hk2019.hk.Presets instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_strict":
            case "strict":
            {
                value = instance.m_strict;
                return true;
            }
            case "m_valueType":
            case "valueType":
            {
                value = instance.m_valueType;
                return true;
            }
            case "m_numPresets":
            case "numPresets":
            {
                value = instance.m_numPresets;
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
                if (value is Havok.hk2019.IHavokObject castValue)
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
