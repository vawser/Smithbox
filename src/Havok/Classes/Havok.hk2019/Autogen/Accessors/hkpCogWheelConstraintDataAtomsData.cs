// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpCogWheelConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpCogWheelConstraintData.Atoms>
{
    public hkpCogWheelConstraintDataAtomsData(Havok.hk2019.hkpCogWheelConstraintData.Atoms instance) : base(instance) { }

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
            case "m_cogWheels":
            case "cogWheels":
            {
                value = instance.m_cogWheels;
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
            case "m_cogWheels":
            case "cogWheels":
            {
                if (value is not Havok.hk2019.hkpCogWheelConstraintAtom castValue) return false;
                instance.m_cogWheels = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
