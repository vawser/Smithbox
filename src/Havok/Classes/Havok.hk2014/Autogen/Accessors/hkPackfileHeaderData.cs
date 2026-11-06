// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkPackfileHeaderData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkPackfileHeader>
{
    private static readonly System.Reflection.FieldInfo _magicInfo = typeof(Havok.hk2014.hkPackfileHeader).GetField("m_magic")!;
    private static readonly System.Reflection.FieldInfo _layoutRulesInfo = typeof(Havok.hk2014.hkPackfileHeader).GetField("m_layoutRules")!;
    private static readonly System.Reflection.FieldInfo _contentsVersionInfo = typeof(Havok.hk2014.hkPackfileHeader).GetField("m_contentsVersion")!;
    public hkPackfileHeaderData(Havok.hk2014.hkPackfileHeader instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_magic":
            case "magic":
            {
                value = instance.m_magic;
                return true;
            }
            case "m_userTag":
            case "userTag":
            {
                value = instance.m_userTag;
                return true;
            }
            case "m_fileVersion":
            case "fileVersion":
            {
                value = instance.m_fileVersion;
                return true;
            }
            case "m_layoutRules":
            case "layoutRules":
            {
                value = instance.m_layoutRules;
                return true;
            }
            case "m_numSections":
            case "numSections":
            {
                value = instance.m_numSections;
                return true;
            }
            case "m_contentsSectionIndex":
            case "contentsSectionIndex":
            {
                value = instance.m_contentsSectionIndex;
                return true;
            }
            case "m_contentsSectionOffset":
            case "contentsSectionOffset":
            {
                value = instance.m_contentsSectionOffset;
                return true;
            }
            case "m_contentsClassNameSectionIndex":
            case "contentsClassNameSectionIndex":
            {
                value = instance.m_contentsClassNameSectionIndex;
                return true;
            }
            case "m_contentsClassNameSectionOffset":
            case "contentsClassNameSectionOffset":
            {
                value = instance.m_contentsClassNameSectionOffset;
                return true;
            }
            case "m_contentsVersion":
            case "contentsVersion":
            {
                value = instance.m_contentsVersion;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_maxpredicate":
            case "maxpredicate":
            {
                value = instance.m_maxpredicate;
                return true;
            }
            case "m_predicateArraySizePlusPadding":
            case "predicateArraySizePlusPadding":
            {
                value = instance.m_predicateArraySizePlusPadding;
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
            case "m_magic":
            case "magic":
            {
                if (value is not int[] castValue || castValue.Length != 2) return false;
                try
                {
                    _magicInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_userTag":
            case "userTag":
            {
                if (value is not int castValue) return false;
                instance.m_userTag = castValue;
                return true;
            }
            case "m_fileVersion":
            case "fileVersion":
            {
                if (value is not int castValue) return false;
                instance.m_fileVersion = castValue;
                return true;
            }
            case "m_layoutRules":
            case "layoutRules":
            {
                if (value is not byte[] castValue || castValue.Length != 4) return false;
                try
                {
                    _layoutRulesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_numSections":
            case "numSections":
            {
                if (value is not int castValue) return false;
                instance.m_numSections = castValue;
                return true;
            }
            case "m_contentsSectionIndex":
            case "contentsSectionIndex":
            {
                if (value is not int castValue) return false;
                instance.m_contentsSectionIndex = castValue;
                return true;
            }
            case "m_contentsSectionOffset":
            case "contentsSectionOffset":
            {
                if (value is not int castValue) return false;
                instance.m_contentsSectionOffset = castValue;
                return true;
            }
            case "m_contentsClassNameSectionIndex":
            case "contentsClassNameSectionIndex":
            {
                if (value is not int castValue) return false;
                instance.m_contentsClassNameSectionIndex = castValue;
                return true;
            }
            case "m_contentsClassNameSectionOffset":
            case "contentsClassNameSectionOffset":
            {
                if (value is not int castValue) return false;
                instance.m_contentsClassNameSectionOffset = castValue;
                return true;
            }
            case "m_contentsVersion":
            case "contentsVersion":
            {
                if (value is not byte[] castValue || castValue.Length != 16) return false;
                try
                {
                    _contentsVersionInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_flags":
            case "flags":
            {
                if (value is not int castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_maxpredicate":
            case "maxpredicate":
            {
                if (value is not ushort castValue) return false;
                instance.m_maxpredicate = castValue;
                return true;
            }
            case "m_predicateArraySizePlusPadding":
            case "predicateArraySizePlusPadding":
            {
                if (value is not ushort castValue) return false;
                instance.m_predicateArraySizePlusPadding = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
