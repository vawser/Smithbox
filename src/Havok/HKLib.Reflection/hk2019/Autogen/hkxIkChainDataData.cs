// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxIkChainDataData : HavokData<HKLib.hk2019.hkxIkChainData> 
{
    public hkxIkChainDataData(HavokType type, HKLib.hk2019.hkxIkChainData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_firstJoint":
            case "firstJoint":
            {
                if (instance.m_firstJoint is null)
                {
                    return true;
                }
                if (instance.m_firstJoint is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_endJoint":
            case "endJoint":
            {
                if (instance.m_endJoint is null)
                {
                    return true;
                }
                if (instance.m_endJoint is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_effector":
            case "effector":
            {
                if (instance.m_effector is null)
                {
                    return true;
                }
                if (instance.m_effector is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_effectorFromEnd":
            case "effectorFromEnd":
            {
                if (instance.m_effectorFromEnd is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (instance.m_weight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_twist":
            case "twist":
            {
                if (instance.m_twist is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_active":
            case "active":
            {
                if (instance.m_active is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lock":
            case "lock":
            {
                if (instance.m_lock is not TGet castValue) return false;
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
                if (value is not HKLib.hk2019.hkQsTransform castValue) return false;
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
