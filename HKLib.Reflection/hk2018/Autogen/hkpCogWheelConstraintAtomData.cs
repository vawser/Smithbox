// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpCogWheelConstraintAtomData : HavokData<hkpCogWheelConstraintAtom> 
{
    public hkpCogWheelConstraintAtomData(HavokType type, hkpCogWheelConstraintAtom instance) : base(type, instance) {}

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
            case "m_cogWheelRadiusA":
            case "cogWheelRadiusA":
            {
                if (instance.m_cogWheelRadiusA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cogWheelRadiusB":
            case "cogWheelRadiusB":
            {
                if (instance.m_cogWheelRadiusB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isScrew":
            case "isScrew":
            {
                if (instance.m_isScrew is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_memOffsetToInitialAngleOffset":
            case "memOffsetToInitialAngleOffset":
            {
                if (instance.m_memOffsetToInitialAngleOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_memOffsetToPrevAngle":
            case "memOffsetToPrevAngle":
            {
                if (instance.m_memOffsetToPrevAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_memOffsetToRevolutionCounter":
            case "memOffsetToRevolutionCounter":
            {
                if (instance.m_memOffsetToRevolutionCounter is not TGet castValue) return false;
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
