// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclUpdateAllVertexFramesOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclUpdateAllVertexFramesOperator>
{
    public hclUpdateAllVertexFramesOperatorData(Havok.hk2014.hclUpdateAllVertexFramesOperator instance) : base(instance) { }

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
            case "m_vertToNormalID":
            case "vertToNormalID":
            {
                value = instance.m_vertToNormalID;
                return true;
            }
            case "m_triangleFlips":
            case "triangleFlips":
            {
                value = instance.m_triangleFlips;
                return true;
            }
            case "m_referenceVertices":
            case "referenceVertices":
            {
                value = instance.m_referenceVertices;
                return true;
            }
            case "m_tangentEdgeCosAngle":
            case "tangentEdgeCosAngle":
            {
                value = instance.m_tangentEdgeCosAngle;
                return true;
            }
            case "m_tangentEdgeSinAngle":
            case "tangentEdgeSinAngle":
            {
                value = instance.m_tangentEdgeSinAngle;
                return true;
            }
            case "m_biTangentFlip":
            case "biTangentFlip":
            {
                value = instance.m_biTangentFlip;
                return true;
            }
            case "m_bufferIdx":
            case "bufferIdx":
            {
                value = instance.m_bufferIdx;
                return true;
            }
            case "m_numUniqueNormalIDs":
            case "numUniqueNormalIDs":
            {
                value = instance.m_numUniqueNormalIDs;
                return true;
            }
            case "m_updateNormals":
            case "updateNormals":
            {
                value = instance.m_updateNormals;
                return true;
            }
            case "m_updateTangents":
            case "updateTangents":
            {
                value = instance.m_updateTangents;
                return true;
            }
            case "m_updateBiTangents":
            case "updateBiTangents":
            {
                value = instance.m_updateBiTangents;
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
            case "m_vertToNormalID":
            case "vertToNormalID":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_vertToNormalID = castValue;
                return true;
            }
            case "m_triangleFlips":
            case "triangleFlips":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_triangleFlips = castValue;
                return true;
            }
            case "m_referenceVertices":
            case "referenceVertices":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_referenceVertices = castValue;
                return true;
            }
            case "m_tangentEdgeCosAngle":
            case "tangentEdgeCosAngle":
            {
                if (value is not List<float> castValue) return false;
                instance.m_tangentEdgeCosAngle = castValue;
                return true;
            }
            case "m_tangentEdgeSinAngle":
            case "tangentEdgeSinAngle":
            {
                if (value is not List<float> castValue) return false;
                instance.m_tangentEdgeSinAngle = castValue;
                return true;
            }
            case "m_biTangentFlip":
            case "biTangentFlip":
            {
                if (value is not List<float> castValue) return false;
                instance.m_biTangentFlip = castValue;
                return true;
            }
            case "m_bufferIdx":
            case "bufferIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_bufferIdx = castValue;
                return true;
            }
            case "m_numUniqueNormalIDs":
            case "numUniqueNormalIDs":
            {
                if (value is not uint castValue) return false;
                instance.m_numUniqueNormalIDs = castValue;
                return true;
            }
            case "m_updateNormals":
            case "updateNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_updateNormals = castValue;
                return true;
            }
            case "m_updateTangents":
            case "updateTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_updateTangents = castValue;
                return true;
            }
            case "m_updateBiTangents":
            case "updateBiTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_updateBiTangents = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
