// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbJigglerGroupData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbJigglerGroup>
{
    public hkbJigglerGroupData(Havok.hk2014.hkbJigglerGroup instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_boneIndices":
            case "boneIndices":
            {
                value = instance.m_boneIndices;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                value = instance.m_mass;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                value = instance.m_stiffness;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                value = instance.m_damping;
                return true;
            }
            case "m_maxElongation":
            case "maxElongation":
            {
                value = instance.m_maxElongation;
                return true;
            }
            case "m_maxCompression":
            case "maxCompression":
            {
                value = instance.m_maxCompression;
                return true;
            }
            case "m_propagateToChildren":
            case "propagateToChildren":
            {
                value = instance.m_propagateToChildren;
                return true;
            }
            case "m_affectSiblings":
            case "affectSiblings":
            {
                value = instance.m_affectSiblings;
                return true;
            }
            case "m_rotateBonesForSkinning":
            case "rotateBonesForSkinning":
            {
                value = instance.m_rotateBonesForSkinning;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
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
                if (value is Havok.hk2014.hkbBoneIndexArray castValue)
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
