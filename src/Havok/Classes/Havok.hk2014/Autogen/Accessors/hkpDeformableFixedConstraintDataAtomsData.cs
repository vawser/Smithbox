// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpDeformableFixedConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpDeformableFixedConstraintData.Atoms>
{
    public hkpDeformableFixedConstraintDataAtomsData(Havok.hk2014.hkpDeformableFixedConstraintData.Atoms instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transforms":
            case "transforms":
            {
                value = instance.m_transforms;
                return true;
            }
            case "m_lin":
            case "lin":
            {
                value = instance.m_lin;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                value = instance.m_ang;
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
            case "m_transforms":
            case "transforms":
            {
                if (value is not Havok.hk2014.hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_lin":
            case "lin":
            {
                if (value is not Havok.hk2014.hkpDeformableLinConstraintAtom castValue) return false;
                instance.m_lin = castValue;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                if (value is not Havok.hk2014.hkpDeformableAngConstraintAtom castValue) return false;
                instance.m_ang = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
