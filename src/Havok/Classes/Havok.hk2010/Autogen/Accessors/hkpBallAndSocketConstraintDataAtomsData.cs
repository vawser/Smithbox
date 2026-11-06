// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpBallAndSocketConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpBallAndSocketConstraintData.Atoms>
{
    public hkpBallAndSocketConstraintDataAtomsData(Havok.hk2010.hkpBallAndSocketConstraintData.Atoms instance) : base(instance) { }

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
            case "m_ballSocket":
            case "ballSocket":
            {
                value = instance.m_ballSocket;
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
                if (value is not Havok.hk2010.hkpSetLocalTranslationsConstraintAtom castValue) return false;
                instance.m_pivots = castValue;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                if (value is not Havok.hk2010.hkpSetupStabilizationAtom castValue) return false;
                instance.m_setupStabilization = castValue;
                return true;
            }
            case "m_ballSocket":
            case "ballSocket":
            {
                if (value is not Havok.hk2010.hkpBallSocketConstraintAtom castValue) return false;
                instance.m_ballSocket = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
