// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkPresetsParsingDeclData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hk.Presets.ParsingDecl>
{
    public hkPresetsParsingDeclData(Havok.hk2019.hk.Presets.ParsingDecl instance) : base(instance) { }

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
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_builtinType":
            case "builtinType":
            {
                value = instance.m_builtinType;
                return true;
            }
            case "m_values":
            case "values":
            {
                value = instance.m_values;
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
            case "m_type":
            case "type":
            {
                if (value is null)
                {
                    instance.m_type = default;
                    return true;
                }
                if (value is Havok.hk2019.IHavokObject castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                return false;
            }
            case "m_builtinType":
            case "builtinType":
            {
                if (value is null)
                {
                    instance.m_builtinType = default;
                    return true;
                }
                if (value is Havok.hk2019.IHavokObject castValue)
                {
                    instance.m_builtinType = castValue;
                    return true;
                }
                return false;
            }
            case "m_values":
            case "values":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_values = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
