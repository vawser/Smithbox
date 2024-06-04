// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkAlignSceneToNodeOptionsData : HavokData<hkAlignSceneToNodeOptions> 
{
    public hkAlignSceneToNodeOptionsData(HavokType type, hkAlignSceneToNodeOptions instance) : base(type, instance) {}

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
            case "m_invert":
            case "invert":
            {
                if (instance.m_invert is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformPositionX":
            case "transformPositionX":
            {
                if (instance.m_transformPositionX is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformPositionY":
            case "transformPositionY":
            {
                if (instance.m_transformPositionY is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformPositionZ":
            case "transformPositionZ":
            {
                if (instance.m_transformPositionZ is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformRotation":
            case "transformRotation":
            {
                if (instance.m_transformRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformScale":
            case "transformScale":
            {
                if (instance.m_transformScale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformSkew":
            case "transformSkew":
            {
                if (instance.m_transformSkew is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_keyframe":
            case "keyframe":
            {
                if (instance.m_keyframe is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nodeName":
            case "nodeName":
            {
                if (instance.m_nodeName is null)
                {
                    return true;
                }
                if (instance.m_nodeName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_invert":
            case "invert":
            {
                if (value is not bool castValue) return false;
                instance.m_invert = castValue;
                return true;
            }
            case "m_transformPositionX":
            case "transformPositionX":
            {
                if (value is not bool castValue) return false;
                instance.m_transformPositionX = castValue;
                return true;
            }
            case "m_transformPositionY":
            case "transformPositionY":
            {
                if (value is not bool castValue) return false;
                instance.m_transformPositionY = castValue;
                return true;
            }
            case "m_transformPositionZ":
            case "transformPositionZ":
            {
                if (value is not bool castValue) return false;
                instance.m_transformPositionZ = castValue;
                return true;
            }
            case "m_transformRotation":
            case "transformRotation":
            {
                if (value is not bool castValue) return false;
                instance.m_transformRotation = castValue;
                return true;
            }
            case "m_transformScale":
            case "transformScale":
            {
                if (value is not bool castValue) return false;
                instance.m_transformScale = castValue;
                return true;
            }
            case "m_transformSkew":
            case "transformSkew":
            {
                if (value is not bool castValue) return false;
                instance.m_transformSkew = castValue;
                return true;
            }
            case "m_keyframe":
            case "keyframe":
            {
                if (value is not int castValue) return false;
                instance.m_keyframe = castValue;
                return true;
            }
            case "m_nodeName":
            case "nodeName":
            {
                if (value is null)
                {
                    instance.m_nodeName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_nodeName = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
