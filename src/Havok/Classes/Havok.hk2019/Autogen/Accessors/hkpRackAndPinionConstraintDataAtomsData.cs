// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpRackAndPinionConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpRackAndPinionConstraintData.Atoms>
{
    public hkpRackAndPinionConstraintDataAtomsData(Havok.hk2019.hkpRackAndPinionConstraintData.Atoms instance) : base(instance) { }

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
            case "m_rackAndPinion":
            case "rackAndPinion":
            {
                value = instance.m_rackAndPinion;
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
                if (value is not Havok.hk2019.hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_rackAndPinion":
            case "rackAndPinion":
            {
                if (value is not Havok.hk2019.hkpRackAndPinionConstraintAtom castValue) return false;
                instance.m_rackAndPinion = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
