// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpCogWheelConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpCogWheelConstraintAtom>
{
    public hkpCogWheelConstraintAtomData(Havok.hk2018.hkpCogWheelConstraintAtom instance) : base(instance) { }

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
            case "m_cogWheelRadiusA":
            case "cogWheelRadiusA":
            {
                value = instance.m_cogWheelRadiusA;
                return true;
            }
            case "m_cogWheelRadiusB":
            case "cogWheelRadiusB":
            {
                value = instance.m_cogWheelRadiusB;
                return true;
            }
            case "m_isScrew":
            case "isScrew":
            {
                value = instance.m_isScrew;
                return true;
            }
            case "m_memOffsetToInitialAngleOffset":
            case "memOffsetToInitialAngleOffset":
            {
                value = instance.m_memOffsetToInitialAngleOffset;
                return true;
            }
            case "m_memOffsetToPrevAngle":
            case "memOffsetToPrevAngle":
            {
                value = instance.m_memOffsetToPrevAngle;
                return true;
            }
            case "m_memOffsetToRevolutionCounter":
            case "memOffsetToRevolutionCounter":
            {
                value = instance.m_memOffsetToRevolutionCounter;
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
                if (value is Havok.hk2018.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2018.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_cogWheelRadiusA":
            case "cogWheelRadiusA":
            {
                if (value is not float castValue) return false;
                instance.m_cogWheelRadiusA = castValue;
                return true;
            }
            case "m_cogWheelRadiusB":
            case "cogWheelRadiusB":
            {
                if (value is not float castValue) return false;
                instance.m_cogWheelRadiusB = castValue;
                return true;
            }
            case "m_isScrew":
            case "isScrew":
            {
                if (value is not bool castValue) return false;
                instance.m_isScrew = castValue;
                return true;
            }
            case "m_memOffsetToInitialAngleOffset":
            case "memOffsetToInitialAngleOffset":
            {
                if (value is not sbyte castValue) return false;
                instance.m_memOffsetToInitialAngleOffset = castValue;
                return true;
            }
            case "m_memOffsetToPrevAngle":
            case "memOffsetToPrevAngle":
            {
                if (value is not sbyte castValue) return false;
                instance.m_memOffsetToPrevAngle = castValue;
                return true;
            }
            case "m_memOffsetToRevolutionCounter":
            case "memOffsetToRevolutionCounter":
            {
                if (value is not sbyte castValue) return false;
                instance.m_memOffsetToRevolutionCounter = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
