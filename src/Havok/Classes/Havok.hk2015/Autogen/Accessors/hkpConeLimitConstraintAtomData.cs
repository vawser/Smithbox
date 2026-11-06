// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkpConeLimitConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkpConeLimitConstraintAtom>
{
    public hkpConeLimitConstraintAtomData(Havok.hk2015.hkpConeLimitConstraintAtom instance) : base(instance) { }

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
            case "m_twistAxisInA":
            case "twistAxisInA":
            {
                value = instance.m_twistAxisInA;
                return true;
            }
            case "m_refAxisInB":
            case "refAxisInB":
            {
                value = instance.m_refAxisInB;
                return true;
            }
            case "m_angleMeasurementMode":
            case "angleMeasurementMode":
            {
                value = instance.m_angleMeasurementMode;
                return true;
            }
            case "m_memOffsetToAngleOffset":
            case "memOffsetToAngleOffset":
            {
                value = instance.m_memOffsetToAngleOffset;
                return true;
            }
            case "m_minAngle":
            case "minAngle":
            {
                value = instance.m_minAngle;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                value = instance.m_maxAngle;
                return true;
            }
            case "m_angularLimitsTauFactor":
            case "angularLimitsTauFactor":
            {
                value = instance.m_angularLimitsTauFactor;
                return true;
            }
            case "m_angularLimitsDampFactor":
            case "angularLimitsDampFactor":
            {
                value = instance.m_angularLimitsDampFactor;
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
                if (value is Havok.hk2015.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2015.hkpConstraintAtom.AtomType)ushortValue;
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
            case "m_twistAxisInA":
            case "twistAxisInA":
            {
                if (value is not byte castValue) return false;
                instance.m_twistAxisInA = castValue;
                return true;
            }
            case "m_refAxisInB":
            case "refAxisInB":
            {
                if (value is not byte castValue) return false;
                instance.m_refAxisInB = castValue;
                return true;
            }
            case "m_angleMeasurementMode":
            case "angleMeasurementMode":
            {
                if (value is Havok.hk2015.hkpConeLimitConstraintAtom.MeasurementMode castValue)
                {
                    instance.m_angleMeasurementMode = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_angleMeasurementMode = (Havok.hk2015.hkpConeLimitConstraintAtom.MeasurementMode)byteValue;
                    return true;
                }
                return false;
            }
            case "m_memOffsetToAngleOffset":
            case "memOffsetToAngleOffset":
            {
                if (value is not ushort castValue) return false;
                instance.m_memOffsetToAngleOffset = castValue;
                return true;
            }
            case "m_minAngle":
            case "minAngle":
            {
                if (value is not float castValue) return false;
                instance.m_minAngle = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngle = castValue;
                return true;
            }
            case "m_angularLimitsTauFactor":
            case "angularLimitsTauFactor":
            {
                if (value is not float castValue) return false;
                instance.m_angularLimitsTauFactor = castValue;
                return true;
            }
            case "m_angularLimitsDampFactor":
            case "angularLimitsDampFactor":
            {
                if (value is not float castValue) return false;
                instance.m_angularLimitsDampFactor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
