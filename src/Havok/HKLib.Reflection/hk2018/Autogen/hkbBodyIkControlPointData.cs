// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkbBodyIkControlBits;
using Enum = HKLib.hk2018.hkbBodyIkControlBits.Enum;

namespace HKLib.Reflection.hk2018;

internal class hkbBodyIkControlPointData : HavokData<hkbBodyIkControlPoint> 
{
    public hkbBodyIkControlPointData(HavokType type, hkbBodyIkControlPoint instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneIdx":
            case "boneIdx":
            {
                if (instance.m_boneIdx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_effectorsOffsetLS":
            case "effectorsOffsetLS":
            {
                if (instance.m_effectorsOffsetLS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_effectors":
            case "effectors":
            {
                if (instance.m_effectors is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((short)instance.m_effectors is TGet shortValue)
                {
                    value = shortValue;
                    return true;
                }
                return false;
            }
            case "m_priority":
            case "priority":
            {
                if (instance.m_priority is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_priority is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_taskInfluenceDistance":
            case "taskInfluenceDistance":
            {
                if (instance.m_taskInfluenceDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_animationInfluenceDistance":
            case "animationInfluenceDistance":
            {
                if (instance.m_animationInfluenceDistance is not TGet castValue) return false;
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
                if (value is Enum castValue)
                {
                    instance.m_effectors = castValue;
                    return true;
                }
                if (value is short shortValue)
                {
                    instance.m_effectors = (Enum)shortValue;
                    return true;
                }
                return false;
            }
            case "m_priority":
            case "priority":
            {
                if (value is HKLib.hk2018.hkbBodyIkControlPriority.Enum castValue)
                {
                    instance.m_priority = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_priority = (HKLib.hk2018.hkbBodyIkControlPriority.Enum)sbyteValue;
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
