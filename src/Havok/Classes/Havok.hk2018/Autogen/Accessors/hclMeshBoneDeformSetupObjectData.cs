// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclMeshBoneDeformSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclMeshBoneDeformSetupObject>
{
    public hclMeshBoneDeformSetupObjectData(Havok.hk2018.hclMeshBoneDeformSetupObject instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_inputBufferSetupObject":
            case "inputBufferSetupObject":
            {
                value = instance.m_inputBufferSetupObject;
                return true;
            }
            case "m_inputTriangleSelection":
            case "inputTriangleSelection":
            {
                value = instance.m_inputTriangleSelection;
                return true;
            }
            case "m_outputTransformSetSetupObject":
            case "outputTransformSetSetupObject":
            {
                value = instance.m_outputTransformSetSetupObject;
                return true;
            }
            case "m_deformedBones":
            case "deformedBones":
            {
                value = instance.m_deformedBones;
                return true;
            }
            case "m_maxTrianglesPerBone":
            case "maxTrianglesPerBone":
            {
                value = instance.m_maxTrianglesPerBone;
                return true;
            }
            case "m_minimumTriangleWeight":
            case "minimumTriangleWeight":
            {
                value = instance.m_minimumTriangleWeight;
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
                if (value is Havok.hk2018.hclBufferSetupObject castValue)
                {
                    instance.m_inputBufferSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_inputTriangleSelection":
            case "inputTriangleSelection":
            {
                if (value is not Havok.hk2018.hclTriangleSelectionInput castValue) return false;
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
                if (value is Havok.hk2018.hclTransformSetSetupObject castValue)
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
