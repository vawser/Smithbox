// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclMeshBoneDeformSetupObjectData : HavokData<hclMeshBoneDeformSetupObject> 
{
    public hclMeshBoneDeformSetupObjectData(HavokType type, hclMeshBoneDeformSetupObject instance) : base(type, instance) {}

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
            case "m_inputBufferSetupObject":
            case "inputBufferSetupObject":
            {
                if (instance.m_inputBufferSetupObject is null)
                {
                    return true;
                }
                if (instance.m_inputBufferSetupObject is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_inputTriangleSelection":
            case "inputTriangleSelection":
            {
                if (instance.m_inputTriangleSelection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outputTransformSetSetupObject":
            case "outputTransformSetSetupObject":
            {
                if (instance.m_outputTransformSetSetupObject is null)
                {
                    return true;
                }
                if (instance.m_outputTransformSetSetupObject is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_deformedBones":
            case "deformedBones":
            {
                if (instance.m_deformedBones is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxTrianglesPerBone":
            case "maxTrianglesPerBone":
            {
                if (instance.m_maxTrianglesPerBone is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minimumTriangleWeight":
            case "minimumTriangleWeight":
            {
                if (instance.m_minimumTriangleWeight is not TGet castValue) return false;
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
            case "m_inputBufferSetupObject":
            case "inputBufferSetupObject":
            {
                if (value is null)
                {
                    instance.m_inputBufferSetupObject = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_inputBufferSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_inputTriangleSelection":
            case "inputTriangleSelection":
            {
                if (value is not hclTriangleSelectionInput castValue) return false;
                instance.m_inputTriangleSelection = castValue;
                return true;
            }
            case "m_outputTransformSetSetupObject":
            case "outputTransformSetSetupObject":
            {
                if (value is null)
                {
                    instance.m_outputTransformSetSetupObject = default;
                    return true;
                }
                if (value is hclTransformSetSetupObject castValue)
                {
                    instance.m_outputTransformSetSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_deformedBones":
            case "deformedBones":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_deformedBones = castValue;
                return true;
            }
            case "m_maxTrianglesPerBone":
            case "maxTrianglesPerBone":
            {
                if (value is not uint castValue) return false;
                instance.m_maxTrianglesPerBone = castValue;
                return true;
            }
            case "m_minimumTriangleWeight":
            case "minimumTriangleWeight":
            {
                if (value is not float castValue) return false;
                instance.m_minimumTriangleWeight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
