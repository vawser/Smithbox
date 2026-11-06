// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclObjectSpaceMeshMeshDeformPNTOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclObjectSpaceMeshMeshDeformPNTOperator>
{
    public hclObjectSpaceMeshMeshDeformPNTOperatorData(Havok.hk2014.hclObjectSpaceMeshMeshDeformPNTOperator instance) : base(instance) { }

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
            case "m_triangleFromMeshTransforms":
            case "triangleFromMeshTransforms":
            {
                value = instance.m_triangleFromMeshTransforms;
                return true;
            }
            case "m_objectSpaceDeformer":
            case "objectSpaceDeformer":
            {
                value = instance.m_objectSpaceDeformer;
                return true;
            }
            case "m_localPNTs":
            case "localPNTs":
            {
                value = instance.m_localPNTs;
                return true;
            }
            case "m_localUnpackedPNTs":
            case "localUnpackedPNTs":
            {
                value = instance.m_localUnpackedPNTs;
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
                if (value is Havok.hk2014.hclObjectSpaceMeshMeshDeformOperator.ScaleNormalBehaviour castValue)
                {
                    instance.m_scaleNormalBehaviour = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_scaleNormalBehaviour = (Havok.hk2014.hclObjectSpaceMeshMeshDeformOperator.ScaleNormalBehaviour)uintValue;
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
            case "m_triangleFromMeshTransforms":
            case "triangleFromMeshTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_triangleFromMeshTransforms = castValue;
                return true;
            }
            case "m_objectSpaceDeformer":
            case "objectSpaceDeformer":
            {
                if (value is not Havok.hk2014.hclObjectSpaceDeformer castValue) return false;
                instance.m_objectSpaceDeformer = castValue;
                return true;
            }
            case "m_localPNTs":
            case "localPNTs":
            {
                if (value is not List<Havok.hk2014.hclObjectSpaceDeformer.LocalBlockPNT> castValue) return false;
                instance.m_localPNTs = castValue;
                return true;
            }
            case "m_localUnpackedPNTs":
            case "localUnpackedPNTs":
            {
                if (value is not List<Havok.hk2014.hclObjectSpaceDeformer.LocalBlockUnpackedPNT> castValue) return false;
                instance.m_localUnpackedPNTs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
