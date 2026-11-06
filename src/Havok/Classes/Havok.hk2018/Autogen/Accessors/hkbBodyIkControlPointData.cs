// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbBodyIkControlPointData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbBodyIkControlPoint>
{
    public hkbBodyIkControlPointData(Havok.hk2018.hkbBodyIkControlPoint instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneIdx":
            case "boneIdx":
            {
                value = instance.m_boneIdx;
                return true;
            }
            case "m_effectorsOffsetLS":
            case "effectorsOffsetLS":
            {
                value = instance.m_effectorsOffsetLS;
                return true;
            }
            case "m_effectors":
            case "effectors":
            {
                value = instance.m_effectors;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                value = instance.m_priority;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_taskInfluenceDistance":
            case "taskInfluenceDistance":
            {
                value = instance.m_taskInfluenceDistance;
                return true;
            }
            case "m_animationInfluenceDistance":
            case "animationInfluenceDistance":
            {
                value = instance.m_animationInfluenceDistance;
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
            case "m_boneIdx":
            case "boneIdx":
            {
                if (value is not short castValue) return false;
                instance.m_boneIdx = castValue;
                return true;
            }
            case "m_effectorsOffsetLS":
            case "effectorsOffsetLS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_effectorsOffsetLS = castValue;
                return true;
            }
            case "m_effectors":
            case "effectors":
            {
                if (value is Havok.hk2018.hkbBodyIkControlBits.Enum castValue)
                {
                    instance.m_effectors = castValue;
                    return true;
                }
                if (value is short shortValue)
                {
                    instance.m_effectors = (Havok.hk2018.hkbBodyIkControlBits.Enum)shortValue;
                    return true;
                }
                return false;
            }
            case "m_priority":
            case "priority":
            {
                if (value is Havok.hk2018.hkbBodyIkControlPriority.Enum castValue)
                {
                    instance.m_priority = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_priority = (Havok.hk2018.hkbBodyIkControlPriority.Enum)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_taskInfluenceDistance":
            case "taskInfluenceDistance":
            {
                if (value is not byte castValue) return false;
                instance.m_taskInfluenceDistance = castValue;
                return true;
            }
            case "m_animationInfluenceDistance":
            case "animationInfluenceDistance":
            {
                if (value is not byte castValue) return false;
                instance.m_animationInfluenceDistance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
