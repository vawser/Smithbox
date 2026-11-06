// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpWheelFrictionConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpWheelFrictionConstraintAtom>
{
    private static readonly System.Reflection.FieldInfo _frictionImpulseInfo = typeof(Havok.hk2019.hkpWheelFrictionConstraintAtom).GetField("m_frictionImpulse")!;
    private static readonly System.Reflection.FieldInfo _slipImpulseInfo = typeof(Havok.hk2019.hkpWheelFrictionConstraintAtom).GetField("m_slipImpulse")!;
    public hkpWheelFrictionConstraintAtomData(Havok.hk2019.hkpWheelFrictionConstraintAtom instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                value = instance.m_isEnabled;
                return true;
            }
            case "m_forwardAxis":
            case "forwardAxis":
            {
                value = instance.m_forwardAxis;
                return true;
            }
            case "m_sideAxis":
            case "sideAxis":
            {
                value = instance.m_sideAxis;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_axle":
            case "axle":
            {
                value = instance.m_axle;
                return true;
            }
            case "m_maxFrictionForce":
            case "maxFrictionForce":
            {
                value = instance.m_maxFrictionForce;
                return true;
            }
            case "m_torque":
            case "torque":
            {
                value = instance.m_torque;
                return true;
            }
            case "m_frictionImpulse":
            case "frictionImpulse":
            {
                value = instance.m_frictionImpulse;
                return true;
            }
            case "m_slipImpulse":
            case "slipImpulse":
            {
                value = instance.m_slipImpulse;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2019.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2019.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (value is not byte castValue) return false;
                instance.m_isEnabled = castValue;
                return true;
            }
            case "m_forwardAxis":
            case "forwardAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_forwardAxis = castValue;
                return true;
            }
            case "m_sideAxis":
            case "sideAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_sideAxis = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_axle":
            case "axle":
            {
                if (value is null)
                {
                    instance.m_axle = default;
                    return true;
                }
                if (value is Havok.hk2019.hkpWheelFrictionConstraintAtom.Axle castValue)
                {
                    instance.m_axle = castValue;
                    return true;
                }
                return false;
            }
            case "m_maxFrictionForce":
            case "maxFrictionForce":
            {
                if (value is not float castValue) return false;
                instance.m_maxFrictionForce = castValue;
                return true;
            }
            case "m_torque":
            case "torque":
            {
                if (value is not float castValue) return false;
                instance.m_torque = castValue;
                return true;
            }
            case "m_frictionImpulse":
            case "frictionImpulse":
            {
                if (value is not float[] castValue || castValue.Length != 2) return false;
                try
                {
                    _frictionImpulseInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_slipImpulse":
            case "slipImpulse":
            {
                if (value is not float[] castValue || castValue.Length != 2) return false;
                try
                {
                    _slipImpulseInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
