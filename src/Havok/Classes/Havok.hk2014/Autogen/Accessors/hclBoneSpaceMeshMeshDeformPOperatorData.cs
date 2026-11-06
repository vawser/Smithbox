// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclBoneSpaceMeshMeshDeformPOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclBoneSpaceMeshMeshDeformPOperator>
{
    public hclBoneSpaceMeshMeshDeformPOperatorData(Havok.hk2014.hclBoneSpaceMeshMeshDeformPOperator instance) : base(instance) { }

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
            case "m_outputBufferIdx":
            case "outputBufferIdx":
            {
                value = instance.m_outputBufferIdx;
                return true;
            }
            case "m_scaleNormalBehaviour":
            case "scaleNormalBehaviour":
            {
                value = instance.m_scaleNormalBehaviour;
                return true;
            }
            case "m_inputTrianglesSubset":
            case "inputTrianglesSubset":
            {
                value = instance.m_inputTrianglesSubset;
                return true;
            }
            case "m_boneSpaceDeformer":
            case "boneSpaceDeformer":
            {
                value = instance.m_boneSpaceDeformer;
                return true;
            }
            case "m_localPs":
            case "localPs":
            {
                value = instance.m_localPs;
                return true;
            }
            case "m_localUnpackedPs":
            case "localUnpackedPs":
            {
                value = instance.m_localUnpackedPs;
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
            case "m_outputBufferIdx":
            case "outputBufferIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_outputBufferIdx = castValue;
                return true;
            }
            case "m_scaleNormalBehaviour":
            case "scaleNormalBehaviour":
            {
                if (value is Havok.hk2014.hclBoneSpaceMeshMeshDeformOperator.ScaleNormalBehaviour castValue)
                {
                    instance.m_scaleNormalBehaviour = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_scaleNormalBehaviour = (Havok.hk2014.hclBoneSpaceMeshMeshDeformOperator.ScaleNormalBehaviour)uintValue;
                    return true;
                }
                return false;
            }
            case "m_inputTrianglesSubset":
            case "inputTrianglesSubset":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_inputTrianglesSubset = castValue;
                return true;
            }
            case "m_boneSpaceDeformer":
            case "boneSpaceDeformer":
            {
                if (value is not Havok.hk2014.hclBoneSpaceDeformer castValue) return false;
                instance.m_boneSpaceDeformer = castValue;
                return true;
            }
            case "m_localPs":
            case "localPs":
            {
                if (value is not List<Havok.hk2014.hclBoneSpaceDeformer.LocalBlockP> castValue) return false;
                instance.m_localPs = castValue;
                return true;
            }
            case "m_localUnpackedPs":
            case "localUnpackedPs":
            {
                if (value is not List<Havok.hk2014.hclBoneSpaceDeformer.LocalBlockUnpackedP> castValue) return false;
                instance.m_localUnpackedPs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
