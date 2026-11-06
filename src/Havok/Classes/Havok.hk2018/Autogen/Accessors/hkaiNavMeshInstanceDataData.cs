// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshInstanceDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshInstanceData>
{
    public hkaiNavMeshInstanceDataData(Havok.hk2018.hkaiNavMeshInstanceData instance) : base(instance) { }

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
            case "m_originalMesh":
            case "originalMesh":
            {
                value = instance.m_originalMesh;
                return true;
            }
            case "m_edgeOrigToInstancedMap":
            case "edgeOrigToInstancedMap":
            {
                value = instance.m_edgeOrigToInstancedMap;
                return true;
            }
            case "m_edgeInstancedToOrigMap":
            case "edgeInstancedToOrigMap":
            {
                value = instance.m_edgeInstancedToOrigMap;
                return true;
            }
            case "m_faceOrigToInstancedMap":
            case "faceOrigToInstancedMap":
            {
                value = instance.m_faceOrigToInstancedMap;
                return true;
            }
            case "m_faceInstancedToOrigMap":
            case "faceInstancedToOrigMap":
            {
                value = instance.m_faceInstancedToOrigMap;
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
            case "m_origEdgeOffsets":
            case "origEdgeOffsets":
            {
                value = instance.m_origEdgeOffsets;
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
            case "m_numGarbageFaces":
            case "numGarbageFaces":
            {
                value = instance.m_numGarbageFaces;
                return true;
            }
            case "m_numGarbageEdges":
            case "numGarbageEdges":
            {
                value = instance.m_numGarbageEdges;
                return true;
            }
            case "m_faceMapping":
            case "faceMapping":
            {
                value = instance.m_faceMapping;
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
            case "m_layerIndex":
            case "layerIndex":
            {
                value = instance.m_layerIndex;
                return true;
            }
            case "m_dynUserEdgeBases":
            case "dynUserEdgeBases":
            {
                value = instance.m_dynUserEdgeBases;
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
            case "m_originalMesh":
            case "originalMesh":
            {
                if (value is null)
                {
                    instance.m_originalMesh = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiNavMesh castValue)
                {
                    instance.m_originalMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_edgeOrigToInstancedMap":
            case "edgeOrigToInstancedMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_edgeOrigToInstancedMap = castValue;
                return true;
            }
            case "m_edgeInstancedToOrigMap":
            case "edgeInstancedToOrigMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_edgeInstancedToOrigMap = castValue;
                return true;
            }
            case "m_faceOrigToInstancedMap":
            case "faceOrigToInstancedMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_faceOrigToInstancedMap = castValue;
                return true;
            }
            case "m_faceInstancedToOrigMap":
            case "faceInstancedToOrigMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_faceInstancedToOrigMap = castValue;
                return true;
            }
            case "m_instancedFaces":
            case "instancedFaces":
            {
                if (value is not List<Havok.hk2018.hkaiNavMesh.Face> castValue) return false;
                instance.m_instancedFaces = castValue;
                return true;
            }
            case "m_instancedEdges":
            case "instancedEdges":
            {
                if (value is not List<Havok.hk2018.hkaiNavMesh.Edge> castValue) return false;
                instance.m_instancedEdges = castValue;
                return true;
            }
            case "m_ownedFaces":
            case "ownedFaces":
            {
                if (value is not List<Havok.hk2018.hkaiNavMesh.Face> castValue) return false;
                instance.m_ownedFaces = castValue;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                if (value is not List<Havok.hk2018.hkaiNavMesh.Edge> castValue) return false;
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
            case "m_origEdgeOffsets":
            case "origEdgeOffsets":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_origEdgeOffsets = castValue;
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
            case "m_numGarbageFaces":
            case "numGarbageFaces":
            {
                if (value is not int castValue) return false;
                instance.m_numGarbageFaces = castValue;
                return true;
            }
            case "m_numGarbageEdges":
            case "numGarbageEdges":
            {
                if (value is not int castValue) return false;
                instance.m_numGarbageEdges = castValue;
                return true;
            }
            case "m_faceMapping":
            case "faceMapping":
            {
                if (value is not List<int> castValue) return false;
                instance.m_faceMapping = castValue;
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
            case "m_layerIndex":
            case "layerIndex":
            {
                if (value is not int castValue) return false;
                instance.m_layerIndex = castValue;
                return true;
            }
            case "m_dynUserEdgeBases":
            case "dynUserEdgeBases":
            {
                if (value is not Havok.hk2018.hkHashMap<int, Havok.hk2018.hkaiNavMeshInstanceData.FaceDynUserEdgeBases> castValue) return false;
                instance.m_dynUserEdgeBases = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
