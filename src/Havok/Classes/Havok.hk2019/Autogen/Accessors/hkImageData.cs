// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkImageData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkImage>
{
    public hkImageData(Havok.hk2019.hkImage instance) : base(instance) { }

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
            case "m_subImages":
            case "subImages":
            {
                value = instance.m_subImages;
                return true;
            }
            case "m_dataPtr":
            case "dataPtr":
            {
                value = instance.m_dataPtr;
                return true;
            }
            case "m_dataSize":
            case "dataSize":
            {
                value = instance.m_dataSize;
                return true;
            }
            case "m_dataCapacity":
            case "dataCapacity":
            {
                value = instance.m_dataCapacity;
                return true;
            }
            case "m_userExternalStorage":
            case "userExternalStorage":
            {
                value = instance.m_userExternalStorage;
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
            case "m_subImages":
            case "subImages":
            {
                if (value is not List<Havok.hk2019.hkImage.SubImage> castValue) return false;
                instance.m_subImages = castValue;
                return true;
            }
            case "m_dataPtr":
            case "dataPtr":
            {
                if (value is null)
                {
                    instance.m_dataPtr = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_dataPtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_dataSize":
            case "dataSize":
            {
                if (value is not ulong castValue) return false;
                instance.m_dataSize = castValue;
                return true;
            }
            case "m_dataCapacity":
            case "dataCapacity":
            {
                if (value is not ulong castValue) return false;
                instance.m_dataCapacity = castValue;
                return true;
            }
            case "m_userExternalStorage":
            case "userExternalStorage":
            {
                if (value is not bool castValue) return false;
                instance.m_userExternalStorage = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
