// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpWheelFrictionConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpWheelFrictionConstraintData.Atoms>
{
    public hkpWheelFrictionConstraintDataAtomsData(Havok.hk2018.hkpWheelFrictionConstraintData.Atoms instance) : base(instance) { }

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
            case "m_friction":
            case "friction":
            {
                value = instance.m_friction;
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
            case "m_friction":
            case "friction":
            {
                if (value is not Havok.hk2018.hkpWheelFrictionConstraintAtom castValue) return false;
                instance.m_friction = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
