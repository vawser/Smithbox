// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpBreakableBodyData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpBreakableBody>
{
    public hkpBreakableBodyData(Havok.hk2014.hkpBreakableBody instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_controller":
            case "controller":
            {
                value = instance.m_controller;
                return true;
            }
            case "m_breakableShape":
            case "breakableShape":
            {
                value = instance.m_breakableShape;
                return true;
            }
            case "m_bodyTypeAndFlags":
            case "bodyTypeAndFlags":
            {
                value = instance.m_bodyTypeAndFlags;
                return true;
            }
            case "m_constraintStrength":
            case "constraintStrength":
            {
                value = instance.m_constraintStrength;
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
            case "m_controller":
            case "controller":
            {
                if (value is null)
                {
                    instance.m_controller = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpBreakableBodyController castValue)
                {
                    instance.m_controller = castValue;
                    return true;
                }
                return false;
            }
            case "m_breakableShape":
            case "breakableShape":
            {
                if (value is null)
                {
                    instance.m_breakableShape = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpBreakableShape castValue)
                {
                    instance.m_breakableShape = castValue;
                    return true;
                }
                return false;
            }
            case "m_bodyTypeAndFlags":
            case "bodyTypeAndFlags":
            {
                if (value is not byte castValue) return false;
                instance.m_bodyTypeAndFlags = castValue;
                return true;
            }
            case "m_constraintStrength":
            case "constraintStrength":
            {
                if (value is not float castValue) return false;
                instance.m_constraintStrength = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
