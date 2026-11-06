// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkPackfileSectionHeaderData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkPackfileSectionHeader>
{
    private static readonly System.Reflection.FieldInfo _sectionTagInfo = typeof(Havok.hk2014.hkPackfileSectionHeader).GetField("m_sectionTag")!;
    private static readonly System.Reflection.FieldInfo _padInfo = typeof(Havok.hk2014.hkPackfileSectionHeader).GetField("m_pad")!;
    public hkPackfileSectionHeaderData(Havok.hk2014.hkPackfileSectionHeader instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sectionTag":
            case "sectionTag":
            {
                value = instance.m_sectionTag;
                return true;
            }
            case "m_nullByte":
            case "nullByte":
            {
                value = instance.m_nullByte;
                return true;
            }
            case "m_absoluteDataStart":
            case "absoluteDataStart":
            {
                value = instance.m_absoluteDataStart;
                return true;
            }
            case "m_localFixupsOffset":
            case "localFixupsOffset":
            {
                value = instance.m_localFixupsOffset;
                return true;
            }
            case "m_globalFixupsOffset":
            case "globalFixupsOffset":
            {
                value = instance.m_globalFixupsOffset;
                return true;
            }
            case "m_virtualFixupsOffset":
            case "virtualFixupsOffset":
            {
                value = instance.m_virtualFixupsOffset;
                return true;
            }
            case "m_exportsOffset":
            case "exportsOffset":
            {
                value = instance.m_exportsOffset;
                return true;
            }
            case "m_importsOffset":
            case "importsOffset":
            {
                value = instance.m_importsOffset;
                return true;
            }
            case "m_endOffset":
            case "endOffset":
            {
                value = instance.m_endOffset;
                return true;
            }
            case "m_pad":
            case "pad":
            {
                value = instance.m_pad;
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
            case "m_sectionTag":
            case "sectionTag":
            {
                if (value is not byte[] castValue || castValue.Length != 19) return false;
                try
                {
                    _sectionTagInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_nullByte":
            case "nullByte":
            {
                if (value is not byte castValue) return false;
                instance.m_nullByte = castValue;
                return true;
            }
            case "m_absoluteDataStart":
            case "absoluteDataStart":
            {
                if (value is not int castValue) return false;
                instance.m_absoluteDataStart = castValue;
                return true;
            }
            case "m_localFixupsOffset":
            case "localFixupsOffset":
            {
                if (value is not int castValue) return false;
                instance.m_localFixupsOffset = castValue;
                return true;
            }
            case "m_globalFixupsOffset":
            case "globalFixupsOffset":
            {
                if (value is not int castValue) return false;
                instance.m_globalFixupsOffset = castValue;
                return true;
            }
            case "m_virtualFixupsOffset":
            case "virtualFixupsOffset":
            {
                if (value is not int castValue) return false;
                instance.m_virtualFixupsOffset = castValue;
                return true;
            }
            case "m_exportsOffset":
            case "exportsOffset":
            {
                if (value is not int castValue) return false;
                instance.m_exportsOffset = castValue;
                return true;
            }
            case "m_importsOffset":
            case "importsOffset":
            {
                if (value is not int castValue) return false;
                instance.m_importsOffset = castValue;
                return true;
            }
            case "m_endOffset":
            case "endOffset":
            {
                if (value is not int castValue) return false;
                instance.m_endOffset = castValue;
                return true;
            }
            case "m_pad":
            case "pad":
            {
                if (value is not int[] castValue || castValue.Length != 4) return false;
                try
                {
                    _padInfo.SetValue(instance, value);
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
