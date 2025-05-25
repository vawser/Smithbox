// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMotionCinfoData : HavokData<hknpMotionCinfo> 
{
    public hknpMotionCinfoData(HavokType type, hknpMotionCinfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_motionPropertiesId":
            case "motionPropertiesId":
            {
                if (instance.m_motionPropertiesId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableDeactivation":
            case "enableDeactivation":
            {
                if (instance.m_enableDeactivation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inverseMass":
            case "inverseMass":
            {
                if (instance.m_inverseMass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inverseInertiaLocal":
            case "inverseInertiaLocal":
            {
                if (instance.m_inverseInertiaLocal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_centerOfMassWorld":
            case "centerOfMassWorld":
            {
                if (instance.m_centerOfMassWorld is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_orientation":
            case "orientation":
            {
                if (instance.m_orientation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearVelocity":
            case "linearVelocity":
            {
                if (instance.m_linearVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                if (instance.m_angularVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
