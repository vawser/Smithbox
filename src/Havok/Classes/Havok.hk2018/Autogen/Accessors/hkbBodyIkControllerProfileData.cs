// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbBodyIkControllerProfileData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbBodyIkControllerProfile>
{
    public hkbBodyIkControllerProfileData(Havok.hk2018.hkbBodyIkControllerProfile instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_pins":
            case "pins":
            {
                value = instance.m_pins;
                return true;
            }
            case "m_controlPoints":
            case "controlPoints":
            {
                value = instance.m_controlPoints;
                return true;
            }
            case "m_animationInfluences":
            case "animationInfluences":
            {
                value = instance.m_animationInfluences;
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
                if (value is not List<Havok.hk2018.hkbBodyIkControlPin> castValue) return false;
                instance.m_pins = castValue;
                return true;
            }
            case "m_controlPoints":
            case "controlPoints":
            {
                if (value is not List<Havok.hk2018.hkbBodyIkControlPoint> castValue) return false;
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
                if (value is Havok.hk2018.hkbBoneWeightArray castValue)
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
