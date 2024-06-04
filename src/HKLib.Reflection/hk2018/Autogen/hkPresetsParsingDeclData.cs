// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkPresetsParsingDeclData : HavokData<Presets.ParsingDecl> 
{
    public hkPresetsParsingDeclData(HavokType type, Presets.ParsingDecl instance) : base(type, instance) {}

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
            case "m_type":
            case "type":
            {
                if (instance.m_type is null)
                {
                    return true;
                }
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_values":
            case "values":
            {
                if (instance.m_values is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is null)
                {
                    instance.m_type = default;
                    return true;
                }
                if (value is IHavokObject castValue)
                {
                    instance.m_type = castValue;
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
