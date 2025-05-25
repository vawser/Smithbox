// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBodyIkControllerProfileData : HavokData<hkbBodyIkControllerProfile> 
{
    public hkbBodyIkControllerProfileData(HavokType type, hkbBodyIkControllerProfile instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_pins":
            case "pins":
            {
                if (instance.m_pins is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_controlPoints":
            case "controlPoints":
            {
                if (instance.m_controlPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_animationInfluences":
            case "animationInfluences":
            {
                if (instance.m_animationInfluences is null)
                {
                    return true;
                }
                if (instance.m_animationInfluences is TGet castValue)
                {
                    value = castValue;
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
            case "m_pins":
            case "pins":
            {
                if (value is not List<hkbBodyIkControlPin> castValue) return false;
                instance.m_pins = castValue;
                return true;
            }
            case "m_controlPoints":
            case "controlPoints":
            {
                if (value is not List<hkbBodyIkControlPoint> castValue) return false;
                instance.m_controlPoints = castValue;
                return true;
            }
            case "m_animationInfluences":
            case "animationInfluences":
            {
                if (value is null)
                {
                    instance.m_animationInfluences = default;
                    return true;
                }
                if (value is hkbBoneWeightArray castValue)
                {
                    instance.m_animationInfluences = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
