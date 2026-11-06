// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpPointToPlaneConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpPointToPlaneConstraintData.Atoms>
{
    public hkpPointToPlaneConstraintDataAtomsData(Havok.hk2019.hkpPointToPlaneConstraintData.Atoms instance) : base(instance) { }

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
            case "m_lin":
            case "lin":
            {
                if (value is not Havok.hk2019.hkpLinConstraintAtom castValue) return false;
                instance.m_lin = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
