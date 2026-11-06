// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkAlignSceneToNodeOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkAlignSceneToNodeOptions>
{
    public hkAlignSceneToNodeOptionsData(Havok.hk2018.hkAlignSceneToNodeOptions instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_invert":
            case "invert":
            {
                value = instance.m_invert;
                return true;
            }
            case "m_transformPositionX":
            case "transformPositionX":
            {
                value = instance.m_transformPositionX;
                return true;
            }
            case "m_transformPositionY":
            case "transformPositionY":
            {
                value = instance.m_transformPositionY;
                return true;
            }
            case "m_transformPositionZ":
            case "transformPositionZ":
            {
                value = instance.m_transformPositionZ;
                return true;
            }
            case "m_transformRotation":
            case "transformRotation":
            {
                value = instance.m_transformRotation;
                return true;
            }
            case "m_transformScale":
            case "transformScale":
            {
                value = instance.m_transformScale;
                return true;
            }
            case "m_transformSkew":
            case "transformSkew":
            {
                value = instance.m_transformSkew;
                return true;
            }
            case "m_keyframe":
            case "keyframe":
            {
                value = instance.m_keyframe;
                return true;
            }
            case "m_nodeName":
            case "nodeName":
            {
                value = instance.m_nodeName;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
