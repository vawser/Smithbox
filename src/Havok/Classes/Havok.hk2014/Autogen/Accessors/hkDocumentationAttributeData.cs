// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkDocumentationAttributeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkDocumentationAttribute>
{
    public hkDocumentationAttributeData(Havok.hk2014.hkDocumentationAttribute instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_docsSectionTag":
            case "docsSectionTag":
            {
                value = instance.m_docsSectionTag;
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
            case "m_docsSectionTag":
            case "docsSectionTag":
            {
                if (value is null)
                {
                    instance.m_docsSectionTag = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_docsSectionTag = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
