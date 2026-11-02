// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkImageData : HavokData<HKLib.hk2019.hkImage> 
{
    public hkImageData(HavokType type, HKLib.hk2019.hkImage instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_numMipLevels":
            case "numMipLevels":
            {
                if (instance.m_numMipLevels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numFaces":
            case "numFaces":
            {
                if (instance.m_numFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numArrayElements":
            case "numArrayElements":
            {
                if (instance.m_numArrayElements is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_width":
            case "width":
            {
                if (instance.m_width is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_height":
            case "height":
            {
                if (instance.m_height is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_depth":
            case "depth":
            {
                if (instance.m_depth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_format":
            case "format":
            {
                if (instance.m_format is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_format is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_subImages":
            case "subImages":
            {
                if (instance.m_subImages is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dataPtr":
            case "dataPtr":
            {
                if (instance.m_dataPtr is null)
                {
                    return true;
                }
                if (instance.m_dataPtr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_dataSize":
            case "dataSize":
            {
                if (instance.m_dataSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dataCapacity":
            case "dataCapacity":
            {
                if (instance.m_dataCapacity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userExternalStorage":
            case "userExternalStorage":
            {
                if (instance.m_userExternalStorage is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkImageFormat.Enum castValue)
                {
                    instance.m_format = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_format = (HKLib.hk2019.hkImageFormat.Enum)intValue;
                    return true;
                }
                return false;
            }
            case "m_subImages":
            case "subImages":
            {
                if (value is not List<HKLib.hk2019.hkImage.SubImage> castValue) return false;
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
