// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkpStiffSpringConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkpStiffSpringConstraintData.Atoms>
{
    public hkpStiffSpringConstraintDataAtomsData(Havok.hk2015.hkpStiffSpringConstraintData.Atoms instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pivots":
            case "pivots":
            {
                value = instance.m_pivots;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                value = instance.m_setupStabilization;
                return true;
            }
            case "m_spring":
            case "spring":
            {
                value = instance.m_spring;
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
            case "m_pivots":
            case "pivots":
            {
                if (value is not Havok.hk2015.hkpSetLocalTranslationsConstraintAtom castValue) return false;
                instance.m_pivots = castValue;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                if (value is not Havok.hk2015.hkpSetupStabilizationAtom castValue) return false;
                instance.m_setupStabilization = castValue;
                return true;
            }
            case "m_spring":
            case "spring":
            {
                if (value is not Havok.hk2015.hkpStiffSpringConstraintAtom castValue) return false;
                instance.m_spring = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
