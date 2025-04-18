// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterDataData : HavokData<HKLib.hk2018.hkbCharacterData> 
{
    public hkbCharacterDataData(HavokType type, HKLib.hk2018.hkbCharacterData instance) : base(type, instance) {}

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
            case "m_characterControllerSetup":
            case "characterControllerSetup":
            {
                if (instance.m_characterControllerSetup is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_modelUpMS":
            case "modelUpMS":
            {
                if (instance.m_modelUpMS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_modelForwardMS":
            case "modelForwardMS":
            {
                if (instance.m_modelForwardMS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_modelRightMS":
            case "modelRightMS":
            {
                if (instance.m_modelRightMS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterPropertyInfos":
            case "characterPropertyInfos":
            {
                if (instance.m_characterPropertyInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numBonesPerLod":
            case "numBonesPerLod":
            {
                if (instance.m_numBonesPerLod is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterPropertyValues":
            case "characterPropertyValues":
            {
                if (instance.m_characterPropertyValues is null)
                {
                    return true;
                }
                if (instance.m_characterPropertyValues is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_stringData":
            case "stringData":
            {
                if (instance.m_stringData is null)
                {
                    return true;
                }
                if (instance.m_stringData is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneAttachmentBoneIndices":
            case "boneAttachmentBoneIndices":
            {
                if (instance.m_boneAttachmentBoneIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneAttachmentTransforms":
            case "boneAttachmentTransforms":
            {
                if (instance.m_boneAttachmentTransforms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scale":
            case "scale":
            {
                if (instance.m_scale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_propertySheets":
            case "propertySheets":
            {
                if (instance.m_propertySheets is not TGet castValue) return false;
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
            case "m_characterControllerSetup":
            case "characterControllerSetup":
            {
                if (value is not hkbCharacterControllerSetup castValue) return false;
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
                if (value is not List<hkbVariableInfo> castValue) return false;
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
                if (value is hkbVariableValueSet castValue)
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
                if (value is hkbCharacterStringData castValue)
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
                if (value is not List<hkbCustomPropertySheet?> castValue) return false;
                instance.m_propertySheets = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
