// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiOverlapManagerSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiOverlapManager.Section>
{
    public hkaiOverlapManagerSectionData(Havok.hk2018.hkaiOverlapManager.Section instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_navMeshSectionIndex":
            case "navMeshSectionIndex":
            {
                value = instance.m_navMeshSectionIndex;
                return true;
            }
            case "m_navMesh":
            case "navMesh":
            {
                value = instance.m_navMesh;
                return true;
            }
            case "m_mediator":
            case "mediator":
            {
                value = instance.m_mediator;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                value = instance.m_referenceFrame;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                value = instance.m_layerIndex;
                return true;
            }
            case "m_faceToOverlapsMap":
            case "faceToOverlapsMap":
            {
                value = instance.m_faceToOverlapsMap;
                return true;
            }
            case "m_pendingUpdates":
            case "pendingUpdates":
            {
                value = instance.m_pendingUpdates;
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
                if (value is Havok.hk2018.hkaiNavMesh castValue)
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
                if (value is Havok.hk2018.hkaiNavMeshQueryMediator castValue)
                {
                    instance.m_mediator = castValue;
                    return true;
                }
                return false;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is not Havok.hk2018.hkaiReferenceFrame castValue) return false;
                instance.m_referenceFrame = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Havok.hk2018.hkQTransform castValue) return false;
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
                if (value is not List<Havok.hk2018.hkaiOverlapManager.PendingUpdate> castValue) return false;
                instance.m_pendingUpdates = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
