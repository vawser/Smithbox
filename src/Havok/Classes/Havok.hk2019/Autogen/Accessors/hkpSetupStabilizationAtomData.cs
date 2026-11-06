// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpSetupStabilizationAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpSetupStabilizationAtom>
{
    public hkpSetupStabilizationAtomData(Havok.hk2019.hkpSetupStabilizationAtom instance) : base(instance) { }

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
            case "m_enabled":
            case "enabled":
            {
                value = instance.m_enabled;
                return true;
            }
            case "m_maxLinImpulse":
            case "maxLinImpulse":
            {
                value = instance.m_maxLinImpulse;
                return true;
            }
            case "m_maxAngImpulse":
            case "maxAngImpulse":
            {
                value = instance.m_maxAngImpulse;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                value = instance.m_maxAngle;
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
            case "m_enabled":
            case "enabled":
            {
                if (value is not bool castValue) return false;
                instance.m_enabled = castValue;
                return true;
            }
            case "m_maxLinImpulse":
            case "maxLinImpulse":
            {
                if (value is not float castValue) return false;
                instance.m_maxLinImpulse = castValue;
                return true;
            }
            case "m_maxAngImpulse":
            case "maxAngImpulse":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngImpulse = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
