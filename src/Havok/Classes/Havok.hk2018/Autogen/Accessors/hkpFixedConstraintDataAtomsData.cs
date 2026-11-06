// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpFixedConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpFixedConstraintData.Atoms>
{
    public hkpFixedConstraintDataAtomsData(Havok.hk2018.hkpFixedConstraintData.Atoms instance) : base(instance) { }

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
            case "m_setupStabilization":
            case "setupStabilization":
            {
                value = instance.m_setupStabilization;
                return true;
            }
            case "m_ballSocket":
            case "ballSocket":
            {
                value = instance.m_ballSocket;
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
                if (value is not Havok.hk2018.hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                if (value is not Havok.hk2018.hkpSetupStabilizationAtom castValue) return false;
                instance.m_setupStabilization = castValue;
                return true;
            }
            case "m_ballSocket":
            case "ballSocket":
            {
                if (value is not Havok.hk2018.hkpBallSocketConstraintAtom castValue) return false;
                instance.m_ballSocket = castValue;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                if (value is not Havok.hk2018.hkp3dAngConstraintAtom castValue) return false;
                instance.m_ang = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
