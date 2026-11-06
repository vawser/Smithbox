// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiPathPathPointData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiPath.PathPoint>
{
    public hkaiPathPathPointData(Havok.hk2018.hkaiPath.PathPoint instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_normal":
            case "normal":
            {
                value = instance.m_normal;
                return true;
            }
            case "m_userEdgeData":
            case "userEdgeData":
            {
                value = instance.m_userEdgeData;
                return true;
            }
            case "m_sectionId":
            case "sectionId":
            {
                value = instance.m_sectionId;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
                if (value is Havok.hk2018.hkaiPath.PathPointBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (Havok.hk2018.hkaiPath.PathPointBits)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
