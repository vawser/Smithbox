// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbCharacterSteppedInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbCharacterSteppedInfo>
{
    public hkbCharacterSteppedInfoData(Havok.hk2018.hkbCharacterSteppedInfo instance) : base(instance) { }

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
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_deltaTime":
            case "deltaTime":
            {
                value = instance.m_deltaTime;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                value = instance.m_worldFromModel;
                return true;
            }
            case "m_poseModelSpace":
            case "poseModelSpace":
            {
                value = instance.m_poseModelSpace;
                return true;
            }
            case "m_rigidAttachmentTransforms":
            case "rigidAttachmentTransforms":
            {
                value = instance.m_rigidAttachmentTransforms;
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
                if (value is not Havok.hk2018.hkQsTransform castValue) return false;
                instance.m_worldFromModel = castValue;
                return true;
            }
            case "m_poseModelSpace":
            case "poseModelSpace":
            {
                if (value is not List<Havok.hk2018.hkQsTransform> castValue) return false;
                instance.m_poseModelSpace = castValue;
                return true;
            }
            case "m_rigidAttachmentTransforms":
            case "rigidAttachmentTransforms":
            {
                if (value is not List<Havok.hk2018.hkQsTransform> castValue) return false;
                instance.m_rigidAttachmentTransforms = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
