// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiOverlapManagerSectionData : HavokData<hkaiOverlapManager.Section> 
{
    public hkaiOverlapManagerSectionData(HavokType type, hkaiOverlapManager.Section instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_navMeshSectionIndex":
            case "navMeshSectionIndex":
            {
                if (instance.m_navMeshSectionIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_navMesh":
            case "navMesh":
            {
                if (instance.m_navMesh is null)
                {
                    return true;
                }
                if (instance.m_navMesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_mediator":
            case "mediator":
            {
                if (instance.m_mediator is null)
                {
                    return true;
                }
                if (instance.m_mediator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (instance.m_referenceFrame is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
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
            case "m_faceToOverlapsMap":
            case "faceToOverlapsMap":
            {
                if (instance.m_faceToOverlapsMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pendingUpdates":
            case "pendingUpdates":
            {
                if (instance.m_pendingUpdates is not TGet castValue) return false;
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
            case "m_navMeshSectionIndex":
            case "navMeshSectionIndex":
            {
                if (value is not int castValue) return false;
                instance.m_navMeshSectionIndex = castValue;
                return true;
            }
            case "m_navMesh":
            case "navMesh":
            {
                if (value is null)
                {
                    instance.m_navMesh = default;
                    return true;
                }
                if (value is hkaiNavMesh castValue)
                {
                    instance.m_navMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_mediator":
            case "mediator":
            {
                if (value is null)
                {
                    instance.m_mediator = default;
                    return true;
                }
                if (value is hkaiNavMeshQueryMediator castValue)
                {
                    instance.m_mediator = castValue;
                    return true;
                }
                return false;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is not hkaiReferenceFrame castValue) return false;
                instance.m_referenceFrame = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not hkQTransform castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (value is not int castValue) return false;
                instance.m_layerIndex = castValue;
                return true;
            }
            case "m_faceToOverlapsMap":
            case "faceToOverlapsMap":
            {
                if (value is not List<List<int>> castValue) return false;
                instance.m_faceToOverlapsMap = castValue;
                return true;
            }
            case "m_pendingUpdates":
            case "pendingUpdates":
            {
                if (value is not List<hkaiOverlapManager.PendingUpdate> castValue) return false;
                instance.m_pendingUpdates = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
