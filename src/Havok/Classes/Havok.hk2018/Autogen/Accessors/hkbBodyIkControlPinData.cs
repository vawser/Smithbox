// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbBodyIkControlPinData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbBodyIkControlPin>
{
    public hkbBodyIkControlPinData(Havok.hk2018.hkbBodyIkControlPin instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
