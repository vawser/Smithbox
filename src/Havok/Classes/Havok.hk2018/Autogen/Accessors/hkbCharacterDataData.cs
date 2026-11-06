// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbCharacterDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbCharacterData>
{
    public hkbCharacterDataData(Havok.hk2018.hkbCharacterData instance) : base(instance) { }

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
            case "m_characterControllerSetup":
            case "characterControllerSetup":
            {
                value = instance.m_characterControllerSetup;
                return true;
            }
            case "m_modelUpMS":
            case "modelUpMS":
            {
                value = instance.m_modelUpMS;
                return true;
            }
            case "m_modelForwardMS":
            case "modelForwardMS":
            {
                value = instance.m_modelForwardMS;
                return true;
            }
            case "m_modelRightMS":
            case "modelRightMS":
            {
                value = instance.m_modelRightMS;
                return true;
            }
            case "m_characterPropertyInfos":
            case "characterPropertyInfos":
            {
                value = instance.m_characterPropertyInfos;
                return true;
            }
            case "m_numBonesPerLod":
            case "numBonesPerLod":
            {
                value = instance.m_numBonesPerLod;
                return true;
            }
            case "m_characterPropertyValues":
            case "characterPropertyValues":
            {
                value = instance.m_characterPropertyValues;
                return true;
            }
            case "m_stringData":
            case "stringData":
            {
                value = instance.m_stringData;
                return true;
            }
            case "m_boneAttachmentBoneIndices":
            case "boneAttachmentBoneIndices":
            {
                value = instance.m_boneAttachmentBoneIndices;
                return true;
            }
            case "m_boneAttachmentTransforms":
            case "boneAttachmentTransforms":
            {
                value = instance.m_boneAttachmentTransforms;
                return true;
            }
            case "m_scale":
            case "scale":
            {
                value = instance.m_scale;
                return true;
            }
            case "m_propertySheets":
            case "propertySheets":
            {
                value = instance.m_propertySheets;
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
            case "m_characterControllerSetup":
            case "characterControllerSetup":
            {
                if (value is not Havok.hk2018.hkbCharacterControllerSetup castValue) return false;
                instance.m_characterControllerSetup = castValue;
                return true;
            }
            case "m_modelUpMS":
            case "modelUpMS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_modelUpMS = castValue;
                return true;
            }
            case "m_modelForwardMS":
            case "modelForwardMS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_modelForwardMS = castValue;
                return true;
            }
            case "m_modelRightMS":
            case "modelRightMS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_modelRightMS = castValue;
                return true;
            }
            case "m_characterPropertyInfos":
            case "characterPropertyInfos":
            {
                if (value is not List<Havok.hk2018.hkbVariableInfo> castValue) return false;
                instance.m_characterPropertyInfos = castValue;
                return true;
            }
            case "m_numBonesPerLod":
            case "numBonesPerLod":
            {
                if (value is not List<int> castValue) return false;
                instance.m_numBonesPerLod = castValue;
                return true;
            }
            case "m_characterPropertyValues":
            case "characterPropertyValues":
            {
                if (value is null)
                {
                    instance.m_characterPropertyValues = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbVariableValueSet castValue)
                {
                    instance.m_characterPropertyValues = castValue;
                    return true;
                }
                return false;
            }
            case "m_stringData":
            case "stringData":
            {
                if (value is null)
                {
                    instance.m_stringData = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbCharacterStringData castValue)
                {
                    instance.m_stringData = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneAttachmentBoneIndices":
            case "boneAttachmentBoneIndices":
            {
                if (value is not List<short> castValue) return false;
                instance.m_boneAttachmentBoneIndices = castValue;
                return true;
            }
            case "m_boneAttachmentTransforms":
            case "boneAttachmentTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_boneAttachmentTransforms = castValue;
                return true;
            }
            case "m_scale":
            case "scale":
            {
                if (value is not float castValue) return false;
                instance.m_scale = castValue;
                return true;
            }
            case "m_propertySheets":
            case "propertySheets":
            {
                if (value is not List<Havok.hk2018.hkbCustomPropertySheet?> castValue) return false;
                instance.m_propertySheets = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
