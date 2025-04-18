// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshInstanceDataData : HavokData<HKLib.hk2018.hkaiNavMeshInstanceData> 
{
    public hkaiNavMeshInstanceDataData(HavokType type, HKLib.hk2018.hkaiNavMeshInstanceData instance) : base(type, instance) {}

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
            case "m_originalMesh":
            case "originalMesh":
            {
                if (instance.m_originalMesh is null)
                {
                    return true;
                }
                if (instance.m_originalMesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_edgeOrigToInstancedMap":
            case "edgeOrigToInstancedMap":
            {
                if (instance.m_edgeOrigToInstancedMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeInstancedToOrigMap":
            case "edgeInstancedToOrigMap":
            {
                if (instance.m_edgeInstancedToOrigMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceOrigToInstancedMap":
            case "faceOrigToInstancedMap":
            {
                if (instance.m_faceOrigToInstancedMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceInstancedToOrigMap":
            case "faceInstancedToOrigMap":
            {
                if (instance.m_faceInstancedToOrigMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instancedFaces":
            case "instancedFaces":
            {
                if (instance.m_instancedFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instancedEdges":
            case "instancedEdges":
            {
                if (instance.m_instancedEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ownedFaces":
            case "ownedFaces":
            {
                if (instance.m_ownedFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                if (instance.m_ownedEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ownedVertices":
            case "ownedVertices":
            {
                if (instance.m_ownedVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceFlags":
            case "faceFlags":
            {
                if (instance.m_faceFlags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_origEdgeOffsets":
            case "origEdgeOffsets":
            {
                if (instance.m_origEdgeOffsets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instancedFaceData":
            case "instancedFaceData":
            {
                if (instance.m_instancedFaceData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instancedEdgeData":
            case "instancedEdgeData":
            {
                if (instance.m_instancedEdgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ownedFaceData":
            case "ownedFaceData":
            {
                if (instance.m_ownedFaceData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ownedEdgeData":
            case "ownedEdgeData":
            {
                if (instance.m_ownedEdgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numGarbageFaces":
            case "numGarbageFaces":
            {
                if (instance.m_numGarbageFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numGarbageEdges":
            case "numGarbageEdges":
            {
                if (instance.m_numGarbageEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceMapping":
            case "faceMapping":
            {
                if (instance.m_faceMapping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sectionUid":
            case "sectionUid":
            {
                if (instance.m_sectionUid is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                if (instance.m_runtimeId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (instance.m_layerIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynUserEdgeBases":
            case "dynUserEdgeBases":
            {
                if (instance.m_dynUserEdgeBases is not TGet castValue) return false;
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
            case "m_originalMesh":
            case "originalMesh":
            {
                if (value is null)
                {
                    instance.m_originalMesh = default;
                    return true;
                }
                if (value is hkaiNavMesh castValue)
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
                if (value is not List<hkaiNavMesh.Face> castValue) return false;
                instance.m_instancedFaces = castValue;
                return true;
            }
            case "m_instancedEdges":
            case "instancedEdges":
            {
                if (value is not List<hkaiNavMesh.Edge> castValue) return false;
                instance.m_instancedEdges = castValue;
                return true;
            }
            case "m_ownedFaces":
            case "ownedFaces":
            {
                if (value is not List<hkaiNavMesh.Face> castValue) return false;
                instance.m_ownedFaces = castValue;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                if (value is not List<hkaiNavMesh.Edge> castValue) return false;
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
                if (value is not hkHashMap<int, HKLib.hk2018.hkaiNavMeshInstanceData.FaceDynUserEdgeBases> castValue) return false;
                instance.m_dynUserEdgeBases = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
