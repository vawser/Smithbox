// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpRotationalConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpRotationalConstraintData.Atoms>
{
    public hkpRotationalConstraintDataAtomsData(Havok.hk2014.hkpRotationalConstraintData.Atoms instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rotations":
            case "rotations":
            {
                value = instance.m_rotations;
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
            case "m_rotations":
            case "rotations":
            {
                if (value is not Havok.hk2014.hkpSetLocalRotationsConstraintAtom castValue) return false;
                instance.m_rotations = castValue;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                if (value is not Havok.hk2014.hkpAngConstraintAtom castValue) return false;
                instance.m_ang = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
