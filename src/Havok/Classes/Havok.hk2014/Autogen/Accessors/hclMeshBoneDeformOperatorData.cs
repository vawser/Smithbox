// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclMeshBoneDeformOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclMeshBoneDeformOperator>
{
    public hclMeshBoneDeformOperatorData(Havok.hk2014.hclMeshBoneDeformOperator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_inputBufferIdx":
            case "inputBufferIdx":
            {
                value = instance.m_inputBufferIdx;
                return true;
            }
            case "m_outputTransformSetIdx":
            case "outputTransformSetIdx":
            {
                value = instance.m_outputTransformSetIdx;
                return true;
            }
            case "m_triangleBonePairs":
            case "triangleBonePairs":
            {
                value = instance.m_triangleBonePairs;
                return true;
            }
            case "m_triangleBoneStartForBone":
            case "triangleBoneStartForBone":
            {
                value = instance.m_triangleBoneStartForBone;
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
            case "m_inputBufferIdx":
            case "inputBufferIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_inputBufferIdx = castValue;
                return true;
            }
            case "m_outputTransformSetIdx":
            case "outputTransformSetIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_outputTransformSetIdx = castValue;
                return true;
            }
            case "m_triangleBonePairs":
            case "triangleBonePairs":
            {
                if (value is not List<Havok.hk2014.hclMeshBoneDeformOperator.TriangleBonePair> castValue) return false;
                instance.m_triangleBonePairs = castValue;
                return true;
            }
            case "m_triangleBoneStartForBone":
            case "triangleBoneStartForBone":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_triangleBoneStartForBone = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
