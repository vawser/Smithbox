// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkImageFormat;
using Enum = HKLib.hk2018.hkImageFormat.Enum;

namespace HKLib.Reflection.hk2018;

internal class hkImageHeaderData : HavokData<hkImageHeader> 
{
    public hkImageHeaderData(HavokType type, hkImageHeader instance) : base(type, instance) {}

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
                if (value is Enum castValue)
                {
                    instance.m_format = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_format = (Enum)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
