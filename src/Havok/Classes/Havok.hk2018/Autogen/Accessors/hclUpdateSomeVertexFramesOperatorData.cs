// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclUpdateSomeVertexFramesOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclUpdateSomeVertexFramesOperator>
{
    public hclUpdateSomeVertexFramesOperatorData(Havok.hk2018.hclUpdateSomeVertexFramesOperator instance) : base(instance) { }

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
            case "m_operatorID":
            case "operatorID":
            {
                value = instance.m_operatorID;
                return true;
            }
            case "m_usedBuffers":
            case "usedBuffers":
            {
                value = instance.m_usedBuffers;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                value = instance.m_usedTransformSets;
                return true;
            }
            case "m_involvedTriangles":
            case "involvedTriangles":
            {
                value = instance.m_involvedTriangles;
                return true;
            }
            case "m_involvedVertices":
            case "involvedVertices":
            {
                value = instance.m_involvedVertices;
                return true;
            }
            case "m_selectionVertexToInvolvedVertex":
            case "selectionVertexToInvolvedVertex":
            {
                value = instance.m_selectionVertexToInvolvedVertex;
                return true;
            }
            case "m_involvedVertexToNormalID":
            case "involvedVertexToNormalID":
            {
                value = instance.m_involvedVertexToNormalID;
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
            case "m_operatorID":
            case "operatorID":
            {
                if (value is not uint castValue) return false;
                instance.m_operatorID = castValue;
                return true;
            }
            case "m_usedBuffers":
            case "usedBuffers":
            {
                if (value is not List<Havok.hk2018.hclClothState.BufferAccess> castValue) return false;
                instance.m_usedBuffers = castValue;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                if (value is not List<Havok.hk2018.hclClothState.TransformSetAccess> castValue) return false;
                instance.m_usedTransformSets = castValue;
                return true;
            }
            case "m_involvedTriangles":
            case "involvedTriangles":
            {
                if (value is not List<Havok.hk2018.hclUpdateSomeVertexFramesOperator.Triangle> castValue) return false;
                instance.m_involvedTriangles = castValue;
                return true;
            }
            case "m_involvedVertices":
            case "involvedVertices":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_involvedVertices = castValue;
                return true;
            }
            case "m_selectionVertexToInvolvedVertex":
            case "selectionVertexToInvolvedVertex":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_selectionVertexToInvolvedVertex = castValue;
                return true;
            }
            case "m_involvedVertexToNormalID":
            case "involvedVertexToNormalID":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_involvedVertexToNormalID = castValue;
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
