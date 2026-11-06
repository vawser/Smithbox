// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkSweptTransformfData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkSweptTransformf>
{
    public hkSweptTransformfData(Havok.hk2014.hkSweptTransformf instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_centerOfMass0":
            case "centerOfMass0":
            {
                value = instance.m_centerOfMass0;
                return true;
            }
            case "m_centerOfMass1":
            case "centerOfMass1":
            {
                value = instance.m_centerOfMass1;
                return true;
            }
            case "m_rotation0":
            case "rotation0":
            {
                value = instance.m_rotation0;
                return true;
            }
            case "m_rotation1":
            case "rotation1":
            {
                value = instance.m_rotation1;
                return true;
            }
            case "m_centerOfMassLocal":
            case "centerOfMassLocal":
            {
                value = instance.m_centerOfMassLocal;
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
            case "m_centerOfMass0":
            case "centerOfMass0":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_centerOfMass0 = castValue;
                return true;
            }
            case "m_centerOfMass1":
            case "centerOfMass1":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_centerOfMass1 = castValue;
                return true;
            }
            case "m_rotation0":
            case "rotation0":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotation0 = castValue;
                return true;
            }
            case "m_rotation1":
            case "rotation1":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotation1 = castValue;
                return true;
            }
            case "m_centerOfMassLocal":
            case "centerOfMassLocal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_centerOfMassLocal = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
