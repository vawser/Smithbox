// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpRackAndPinionConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpRackAndPinionConstraintAtom>
{
    public hkpRackAndPinionConstraintAtomData(Havok.hk2019.hkpRackAndPinionConstraintAtom instance) : base(instance) { }

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
            case "m_pinionRadiusOrScrewPitch":
            case "pinionRadiusOrScrewPitch":
            {
                value = instance.m_pinionRadiusOrScrewPitch;
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
