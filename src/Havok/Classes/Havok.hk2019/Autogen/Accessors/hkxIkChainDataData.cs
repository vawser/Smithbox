// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxIkChainDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxIkChainData>
{
    public hkxIkChainDataData(Havok.hk2019.hkxIkChainData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_firstJoint":
            case "firstJoint":
            {
                value = instance.m_firstJoint;
                return true;
            }
            case "m_endJoint":
            case "endJoint":
            {
                value = instance.m_endJoint;
                return true;
            }
            case "m_effector":
            case "effector":
            {
                value = instance.m_effector;
                return true;
            }
            case "m_effectorFromEnd":
            case "effectorFromEnd":
            {
                value = instance.m_effectorFromEnd;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
                return true;
            }
            case "m_twist":
            case "twist":
            {
                value = instance.m_twist;
                return true;
            }
            case "m_active":
            case "active":
            {
                value = instance.m_active;
                return true;
            }
            case "m_lock":
            case "lock":
            {
                value = instance.m_lock;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_firstJoint":
            case "firstJoint":
            {
                if (value is null)
                {
                    instance.m_firstJoint = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_firstJoint = castValue;
                    return true;
                }
                return false;
            }
            case "m_endJoint":
            case "endJoint":
            {
                if (value is null)
                {
                    instance.m_endJoint = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_endJoint = castValue;
                    return true;
                }
                return false;
            }
            case "m_effector":
            case "effector":
            {
                if (value is null)
                {
                    instance.m_effector = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_effector = castValue;
                    return true;
                }
                return false;
            }
            case "m_effectorFromEnd":
            case "effectorFromEnd":
            {
                if (value is not Havok.hk2019.hkQsTransform castValue) return false;
                instance.m_effectorFromEnd = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (value is not float castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_twist":
            case "twist":
            {
                if (value is not float castValue) return false;
                instance.m_twist = castValue;
                return true;
            }
            case "m_active":
            case "active":
            {
                if (value is not bool castValue) return false;
                instance.m_active = castValue;
                return true;
            }
            case "m_lock":
            case "lock":
            {
                if (value is not bool castValue) return false;
                instance.m_lock = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
