// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiNavMeshInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiNavMeshInstance>
{
    public hkaiNavMeshInstanceData(Havok.hk2014.hkaiNavMeshInstance instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_originalMesh":
            case "originalMesh":
            {
                value = instance.m_originalMesh;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                value = instance.m_referenceFrame;
                return true;
            }
            case "m_edgeMap":
            case "edgeMap":
            {
                value = instance.m_edgeMap;
                return true;
            }
            case "m_faceMap":
            case "faceMap":
            {
                value = instance.m_faceMap;
                return true;
            }
            case "m_instancedFaces":
            case "instancedFaces":
            {
                value = instance.m_instancedFaces;
                return true;
            }
            case "m_instancedEdges":
            case "instancedEdges":
            {
                value = instance.m_instancedEdges;
                return true;
            }
            case "m_ownedFaces":
            case "ownedFaces":
            {
                value = instance.m_ownedFaces;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                value = instance.m_ownedEdges;
                return true;
            }
            case "m_ownedVertices":
            case "ownedVertices":
            {
                value = instance.m_ownedVertices;
                return true;
            }
            case "m_faceFlags":
            case "faceFlags":
            {
                value = instance.m_faceFlags;
                return true;
            }
            case "m_cuttingInfo":
            case "cuttingInfo":
            {
                value = instance.m_cuttingInfo;
                return true;
            }
            case "m_instancedFaceData":
            case "instancedFaceData":
            {
                value = instance.m_instancedFaceData;
                return true;
            }
            case "m_instancedEdgeData":
            case "instancedEdgeData":
            {
                value = instance.m_instancedEdgeData;
                return true;
            }
            case "m_ownedFaceData":
            case "ownedFaceData":
            {
                value = instance.m_ownedFaceData;
                return true;
            }
            case "m_ownedEdgeData":
            case "ownedEdgeData":
            {
                value = instance.m_ownedEdgeData;
                return true;
            }
            case "m_clearanceCache":
            case "clearanceCache":
            {
                value = instance.m_clearanceCache;
                return true;
            }
            case "m_globalClearanceCache":
            case "globalClearanceCache":
            {
                value = instance.m_globalClearanceCache;
                return true;
            }
            case "m_faceClearanceIndices":
            case "faceClearanceIndices":
            {
                value = instance.m_faceClearanceIndices;
                return true;
            }
            case "m_maxGlobalClearance":
            case "maxGlobalClearance":
            {
                value = instance.m_maxGlobalClearance;
                return true;
            }
            case "m_sectionUid":
            case "sectionUid":
            {
                value = instance.m_sectionUid;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                value = instance.m_runtimeId;
                return true;
            }
            case "m_layer":
            case "layer":
            {
                value = instance.m_layer;
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
            case "m_originalMesh":
            case "originalMesh":
            {
                if (value is null)
                {
                    instance.m_originalMesh = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiNavMesh castValue)
                {
                    instance.m_originalMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is not Havok.hk2014.hkaiReferenceFrame castValue) return false;
                instance.m_referenceFrame = castValue;
                return true;
            }
            case "m_edgeMap":
            case "edgeMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_edgeMap = castValue;
                return true;
            }
            case "m_faceMap":
            case "faceMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_faceMap = castValue;
                return true;
            }
            case "m_instancedFaces":
            case "instancedFaces":
            {
                if (value is not List<Havok.hk2014.hkaiNavMesh.Face> castValue) return false;
                instance.m_instancedFaces = castValue;
                return true;
            }
            case "m_instancedEdges":
            case "instancedEdges":
            {
                if (value is not List<Havok.hk2014.hkaiNavMesh.Edge> castValue) return false;
                instance.m_instancedEdges = castValue;
                return true;
            }
            case "m_ownedFaces":
            case "ownedFaces":
            {
                if (value is not List<Havok.hk2014.hkaiNavMesh.Face> castValue) return false;
                instance.m_ownedFaces = castValue;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                if (value is not List<Havok.hk2014.hkaiNavMesh.Edge> castValue) return false;
                instance.m_ownedEdges = castValue;
                return true;
            }
            case "m_ownedVertices":
            case "ownedVertices":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_ownedVertices = castValue;
                return true;
            }
            case "m_faceFlags":
            case "faceFlags":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_faceFlags = castValue;
                return true;
            }
            case "m_cuttingInfo":
            case "cuttingInfo":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_cuttingInfo = castValue;
                return true;
            }
            case "m_instancedFaceData":
            case "instancedFaceData":
            {
                if (value is not List<int> castValue) return false;
                instance.m_instancedFaceData = castValue;
                return true;
            }
            case "m_instancedEdgeData":
            case "instancedEdgeData":
            {
                if (value is not List<int> castValue) return false;
                instance.m_instancedEdgeData = castValue;
                return true;
            }
            case "m_ownedFaceData":
            case "ownedFaceData":
            {
                if (value is not List<int> castValue) return false;
                instance.m_ownedFaceData = castValue;
                return true;
            }
            case "m_ownedEdgeData":
            case "ownedEdgeData":
            {
                if (value is not List<int> castValue) return false;
                instance.m_ownedEdgeData = castValue;
                return true;
            }
            case "m_clearanceCache":
            case "clearanceCache":
            {
                if (value is not List<float> castValue) return false;
                instance.m_clearanceCache = castValue;
                return true;
            }
            case "m_globalClearanceCache":
            case "globalClearanceCache":
            {
                if (value is not List<float> castValue) return false;
                instance.m_globalClearanceCache = castValue;
                return true;
            }
            case "m_faceClearanceIndices":
            case "faceClearanceIndices":
            {
                if (value is not List<int> castValue) return false;
                instance.m_faceClearanceIndices = castValue;
                return true;
            }
            case "m_maxGlobalClearance":
            case "maxGlobalClearance":
            {
                if (value is not float castValue) return false;
                instance.m_maxGlobalClearance = castValue;
                return true;
            }
            case "m_sectionUid":
            case "sectionUid":
            {
                if (value is not uint castValue) return false;
                instance.m_sectionUid = castValue;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                if (value is not int castValue) return false;
                instance.m_runtimeId = castValue;
                return true;
            }
            case "m_layer":
            case "layer":
            {
                if (value is not uint castValue) return false;
                instance.m_layer = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
