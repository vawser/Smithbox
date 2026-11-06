// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiPathRequestQueueData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiPathRequestQueue>
{
    public hkaiPathRequestQueueData(Havok.hk2018.hkaiPathRequestQueue instance) : base(instance) { }

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
            case "m_queueId":
            case "queueId":
            {
                value = instance.m_queueId;
                return true;
            }
            case "m_navMeshPathRequestInfos":
            case "navMeshPathRequestInfos":
            {
                value = instance.m_navMeshPathRequestInfos;
                return true;
            }
            case "m_navVolumePathRequestInfos":
            case "navVolumePathRequestInfos":
            {
                value = instance.m_navVolumePathRequestInfos;
                return true;
            }
            case "m_navMeshPathRequests":
            case "navMeshPathRequests":
            {
                value = instance.m_navMeshPathRequests;
                return true;
            }
            case "m_navVolumePathRequests":
            case "navVolumePathRequests":
            {
                value = instance.m_navVolumePathRequests;
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
            case "m_queueId":
            case "queueId":
            {
                if (value is not Havok.hk2018.hkHandle<byte> castValue) return false;
                instance.m_queueId = castValue;
                return true;
            }
            case "m_navMeshPathRequestInfos":
            case "navMeshPathRequestInfos":
            {
                if (value is not List<Havok.hk2018.hkaiNavMeshPathRequestInfo> castValue) return false;
                instance.m_navMeshPathRequestInfos = castValue;
                return true;
            }
            case "m_navVolumePathRequestInfos":
            case "navVolumePathRequestInfos":
            {
                if (value is not List<Havok.hk2018.hkaiNavVolumePathRequestInfo> castValue) return false;
                instance.m_navVolumePathRequestInfos = castValue;
                return true;
            }
            case "m_navMeshPathRequests":
            case "navMeshPathRequests":
            {
                if (value is not List<Havok.hk2018.hkaiNavMeshPathRequest?> castValue) return false;
                instance.m_navMeshPathRequests = castValue;
                return true;
            }
            case "m_navVolumePathRequests":
            case "navVolumePathRequests":
            {
                if (value is not List<Havok.hk2018.hkaiNavVolumePathRequest?> castValue) return false;
                instance.m_navVolumePathRequests = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
