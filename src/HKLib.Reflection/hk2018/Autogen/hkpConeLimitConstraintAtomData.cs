// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpConeLimitConstraintAtomData : HavokData<hkpConeLimitConstraintAtom> 
{
    public hkpConeLimitConstraintAtomData(HavokType type, hkpConeLimitConstraintAtom instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_type is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (instance.m_isEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_twistAxisInA":
            case "twistAxisInA":
            {
                if (instance.m_twistAxisInA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_refAxisInB":
            case "refAxisInB":
            {
                if (instance.m_refAxisInB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angleMeasurementMode":
            case "angleMeasurementMode":
            {
                if (instance.m_angleMeasurementMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_angleMeasurementMode is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_memOffsetToAngleOffset":
            case "memOffsetToAngleOffset":
            {
                if (instance.m_memOffsetToAngleOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minAngle":
            case "minAngle":
            {
                if (instance.m_minAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (instance.m_maxAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularLimitsTauFactor":
            case "angularLimitsTauFactor":
            {
                if (instance.m_angularLimitsTauFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularLimitsDampFactor":
            case "angularLimitsDampFactor":
            {
                if (instance.m_angularLimitsDampFactor is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (hkpConstraintAtom.AtomType)ushortValue;
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
                if (value is hkpConeLimitConstraintAtom.MeasurementMode castValue)
                {
                    instance.m_angleMeasurementMode = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_angleMeasurementMode = (hkpConeLimitConstraintAtom.MeasurementMode)byteValue;
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
