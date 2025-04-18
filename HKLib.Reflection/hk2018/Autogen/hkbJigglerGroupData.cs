// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbJigglerGroupData : HavokData<hkbJigglerGroup> 
{
    public hkbJigglerGroupData(HavokType type, hkbJigglerGroup instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneIndices":
            case "boneIndices":
            {
                if (instance.m_boneIndices is null)
                {
                    return true;
                }
                if (instance.m_boneIndices is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_mass":
            case "mass":
            {
                if (instance.m_mass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (instance.m_stiffness is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                if (instance.m_damping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxElongation":
            case "maxElongation":
            {
                if (instance.m_maxElongation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxCompression":
            case "maxCompression":
            {
                if (instance.m_maxCompression is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_propagateToChildren":
            case "propagateToChildren":
            {
                if (instance.m_propagateToChildren is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_affectSiblings":
            case "affectSiblings":
            {
                if (instance.m_affectSiblings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotateBonesForSkinning":
            case "rotateBonesForSkinning":
            {
                if (instance.m_rotateBonesForSkinning is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneIndices":
            case "boneIndices":
            {
                if (value is null)
                {
                    instance.m_boneIndices = default;
                    return true;
                }
                if (value is hkbBoneIndexArray castValue)
                {
                    instance.m_boneIndices = castValue;
                    return true;
                }
                return false;
            }
            case "m_mass":
            case "mass":
            {
                if (value is not float castValue) return false;
                instance.m_mass = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not float castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                if (value is not float castValue) return false;
                instance.m_damping = castValue;
                return true;
            }
            case "m_maxElongation":
            case "maxElongation":
            {
                if (value is not float castValue) return false;
                instance.m_maxElongation = castValue;
                return true;
            }
            case "m_maxCompression":
            case "maxCompression":
            {
                if (value is not float castValue) return false;
                instance.m_maxCompression = castValue;
                return true;
            }
            case "m_propagateToChildren":
            case "propagateToChildren":
            {
                if (value is not bool castValue) return false;
                instance.m_propagateToChildren = castValue;
                return true;
            }
            case "m_affectSiblings":
            case "affectSiblings":
            {
                if (value is not bool castValue) return false;
                instance.m_affectSiblings = castValue;
                return true;
            }
            case "m_rotateBonesForSkinning":
            case "rotateBonesForSkinning":
            {
                if (value is not bool castValue) return false;
                instance.m_rotateBonesForSkinning = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
