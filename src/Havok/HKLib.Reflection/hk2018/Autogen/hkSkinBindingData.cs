// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkSkinBindingData : HavokData<hkSkinBinding> 
{
    public hkSkinBindingData(HavokType type, hkSkinBinding instance) : base(type, instance) {}

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
            case "m_skin":
            case "skin":
            {
                if (instance.m_skin is null)
                {
                    return true;
                }
                if (instance.m_skin is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_worldFromBoneTransforms":
            case "worldFromBoneTransforms":
            {
                if (instance.m_worldFromBoneTransforms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneNames":
            case "boneNames":
            {
                if (instance.m_boneNames is not TGet castValue) return false;
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
            case "m_skin":
            case "skin":
            {
                if (value is null)
                {
                    instance.m_skin = default;
                    return true;
                }
                if (value is hkMeshShape castValue)
                {
                    instance.m_skin = castValue;
                    return true;
                }
                return false;
            }
            case "m_worldFromBoneTransforms":
            case "worldFromBoneTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_worldFromBoneTransforms = castValue;
                return true;
            }
            case "m_boneNames":
            case "boneNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_boneNames = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
