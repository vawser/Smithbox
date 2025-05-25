// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkParsingDeclData : HavokData<ParsingDecl> 
{
    public hkParsingDeclData(HavokType type, ParsingDecl instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_decl":
            case "decl":
            {
                if (instance.m_decl is null)
                {
                    return true;
                }
                if (instance.m_decl is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_convertFunc":
            case "convertFunc":
            {
                if (instance.m_convertFunc is null)
                {
                    return true;
                }
                if (instance.m_convertFunc is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_decl":
            case "decl":
            {
                if (value is null)
                {
                    instance.m_decl = default;
                    return true;
                }
                if (value is IHavokObject castValue)
                {
                    instance.m_decl = castValue;
                    return true;
                }
                return false;
            }
            case "m_convertFunc":
            case "convertFunc":
            {
                if (value is null)
                {
                    instance.m_convertFunc = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_convertFunc = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
