// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiPathPathPointData : HavokData<HKLib.hk2019.hkaiPath.PathPoint> 
{
    public hkaiPathPathPointData(HavokType type, HKLib.hk2019.hkaiPath.PathPoint instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_normal":
            case "normal":
            {
                if (instance.m_normal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeData":
            case "userEdgeData":
            {
                if (instance.m_userEdgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sectionId":
            case "sectionId":
            {
                if (instance.m_sectionId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_flags is TGet byteValue)
                {
                    value = byteValue;
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
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_normal":
            case "normal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_normal = castValue;
                return true;
            }
            case "m_userEdgeData":
            case "userEdgeData":
            {
                if (value is not uint castValue) return false;
                instance.m_userEdgeData = castValue;
                return true;
            }
            case "m_sectionId":
            case "sectionId":
            {
                if (value is not int castValue) return false;
                instance.m_sectionId = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is HKLib.hk2019.hkaiPath.PathPointBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (HKLib.hk2019.hkaiPath.PathPointBits)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
