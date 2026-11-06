// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpMotionCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpMotionCinfo>
{
    public hknpMotionCinfoData(Havok.hk2018.hknpMotionCinfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_motionPropertiesId":
            case "motionPropertiesId":
            {
                value = instance.m_motionPropertiesId;
                return true;
            }
            case "m_enableDeactivation":
            case "enableDeactivation":
            {
                value = instance.m_enableDeactivation;
                return true;
            }
            case "m_inverseMass":
            case "inverseMass":
            {
                value = instance.m_inverseMass;
                return true;
            }
            case "m_inverseInertiaLocal":
            case "inverseInertiaLocal":
            {
                value = instance.m_inverseInertiaLocal;
                return true;
            }
            case "m_centerOfMassWorld":
            case "centerOfMassWorld":
            {
                value = instance.m_centerOfMassWorld;
                return true;
            }
            case "m_orientation":
            case "orientation":
            {
                value = instance.m_orientation;
                return true;
            }
            case "m_linearVelocity":
            case "linearVelocity":
            {
                value = instance.m_linearVelocity;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                value = instance.m_angularVelocity;
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
            case "m_motionPropertiesId":
            case "motionPropertiesId":
            {
                if (value is not ushort castValue) return false;
                instance.m_motionPropertiesId = castValue;
                return true;
            }
            case "m_enableDeactivation":
            case "enableDeactivation":
            {
                if (value is not bool castValue) return false;
                instance.m_enableDeactivation = castValue;
                return true;
            }
            case "m_inverseMass":
            case "inverseMass":
            {
                if (value is not float castValue) return false;
                instance.m_inverseMass = castValue;
                return true;
            }
            case "m_inverseInertiaLocal":
            case "inverseInertiaLocal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_inverseInertiaLocal = castValue;
                return true;
            }
            case "m_centerOfMassWorld":
            case "centerOfMassWorld":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_centerOfMassWorld = castValue;
                return true;
            }
            case "m_orientation":
            case "orientation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_orientation = castValue;
                return true;
            }
            case "m_linearVelocity":
            case "linearVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_linearVelocity = castValue;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_angularVelocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
