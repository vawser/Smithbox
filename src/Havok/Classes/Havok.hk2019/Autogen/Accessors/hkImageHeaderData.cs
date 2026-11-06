// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkImageHeaderData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkImageHeader>
{
    public hkImageHeaderData(Havok.hk2019.hkImageHeader instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_numMipLevels":
            case "numMipLevels":
            {
                value = instance.m_numMipLevels;
                return true;
            }
            case "m_numFaces":
            case "numFaces":
            {
                value = instance.m_numFaces;
                return true;
            }
            case "m_numArrayElements":
            case "numArrayElements":
            {
                value = instance.m_numArrayElements;
                return true;
            }
            case "m_width":
            case "width":
            {
                value = instance.m_width;
                return true;
            }
            case "m_height":
            case "height":
            {
                value = instance.m_height;
                return true;
            }
            case "m_depth":
            case "depth":
            {
                value = instance.m_depth;
                return true;
            }
            case "m_format":
            case "format":
            {
                value = instance.m_format;
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
            case "m_numMipLevels":
            case "numMipLevels":
            {
                if (value is not uint castValue) return false;
                instance.m_numMipLevels = castValue;
                return true;
            }
            case "m_numFaces":
            case "numFaces":
            {
                if (value is not uint castValue) return false;
                instance.m_numFaces = castValue;
                return true;
            }
            case "m_numArrayElements":
            case "numArrayElements":
            {
                if (value is not uint castValue) return false;
                instance.m_numArrayElements = castValue;
                return true;
            }
            case "m_width":
            case "width":
            {
                if (value is not uint castValue) return false;
                instance.m_width = castValue;
                return true;
            }
            case "m_height":
            case "height":
            {
                if (value is not uint castValue) return false;
                instance.m_height = castValue;
                return true;
            }
            case "m_depth":
            case "depth":
            {
                if (value is not uint castValue) return false;
                instance.m_depth = castValue;
                return true;
            }
            case "m_format":
            case "format":
            {
                if (value is Havok.hk2019.hkImageFormat.Enum castValue)
                {
                    instance.m_format = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_format = (Havok.hk2019.hkImageFormat.Enum)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
