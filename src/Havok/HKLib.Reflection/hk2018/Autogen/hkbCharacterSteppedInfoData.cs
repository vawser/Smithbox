// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterSteppedInfoData : HavokData<hkbCharacterSteppedInfo> 
{
    public hkbCharacterSteppedInfoData(HavokType type, hkbCharacterSteppedInfo instance) : base(type, instance) {}

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
            case "m_characterId":
            case "characterId":
            {
                if (instance.m_characterId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deltaTime":
            case "deltaTime":
            {
                if (instance.m_deltaTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (instance.m_worldFromModel is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_poseModelSpace":
            case "poseModelSpace":
            {
                if (instance.m_poseModelSpace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rigidAttachmentTransforms":
            case "rigidAttachmentTransforms":
            {
                if (instance.m_rigidAttachmentTransforms is not TGet castValue) return false;
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
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_deltaTime":
            case "deltaTime":
            {
                if (value is not float castValue) return false;
                instance.m_deltaTime = castValue;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (value is not hkQsTransform castValue) return false;
                instance.m_worldFromModel = castValue;
                return true;
            }
            case "m_poseModelSpace":
            case "poseModelSpace":
            {
                if (value is not List<hkQsTransform> castValue) return false;
                instance.m_poseModelSpace = castValue;
                return true;
            }
            case "m_rigidAttachmentTransforms":
            case "rigidAttachmentTransforms":
            {
                if (value is not List<hkQsTransform> castValue) return false;
                instance.m_rigidAttachmentTransforms = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
