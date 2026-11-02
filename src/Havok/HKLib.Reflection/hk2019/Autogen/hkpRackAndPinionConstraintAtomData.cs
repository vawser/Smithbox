// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpRackAndPinionConstraintAtomData : HavokData<HKLib.hk2019.hkpRackAndPinionConstraintAtom> 
{
    public hkpRackAndPinionConstraintAtomData(HavokType type, HKLib.hk2019.hkpRackAndPinionConstraintAtom instance) : base(type, instance) {}

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
            case "m_pinionRadiusOrScrewPitch":
            case "pinionRadiusOrScrewPitch":
            {
                if (instance.m_pinionRadiusOrScrewPitch is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (HKLib.hk2019.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_pinionRadiusOrScrewPitch":
            case "pinionRadiusOrScrewPitch":
            {
                if (value is not float castValue) return false;
                instance.m_pinionRadiusOrScrewPitch = castValue;
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
