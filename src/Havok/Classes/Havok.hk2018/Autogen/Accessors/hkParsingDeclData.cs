// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkParsingDeclData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.ParsingDecl>
{
    public hkParsingDeclData(Havok.hk2018.hk.ParsingDecl instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_decl":
            case "decl":
            {
                value = instance.m_decl;
                return true;
            }
            case "m_convertFunc":
            case "convertFunc":
            {
                value = instance.m_convertFunc;
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
            case "m_decl":
            case "decl":
            {
                if (value is null)
                {
                    instance.m_decl = default;
                    return true;
                }
                if (value is Havok.hk2018.IHavokObject castValue)
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
