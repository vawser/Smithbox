// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiPathRequestQueueData : HavokData<hkaiPathRequestQueue> 
{
    public hkaiPathRequestQueueData(HavokType type, hkaiPathRequestQueue instance) : base(type, instance) {}

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
            case "m_queueId":
            case "queueId":
            {
                if (instance.m_queueId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_navMeshPathRequestInfos":
            case "navMeshPathRequestInfos":
            {
                if (instance.m_navMeshPathRequestInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_navVolumePathRequestInfos":
            case "navVolumePathRequestInfos":
            {
                if (instance.m_navVolumePathRequestInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_navMeshPathRequests":
            case "navMeshPathRequests":
            {
                if (instance.m_navMeshPathRequests is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_navVolumePathRequests":
            case "navVolumePathRequests":
            {
                if (instance.m_navVolumePathRequests is not TGet castValue) return false;
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
            case "m_queueId":
            case "queueId":
            {
                if (value is not hkHandle<byte> castValue) return false;
                instance.m_queueId = castValue;
                return true;
            }
            case "m_navMeshPathRequestInfos":
            case "navMeshPathRequestInfos":
            {
                if (value is not List<hkaiNavMeshPathRequestInfo> castValue) return false;
                instance.m_navMeshPathRequestInfos = castValue;
                return true;
            }
            case "m_navVolumePathRequestInfos":
            case "navVolumePathRequestInfos":
            {
                if (value is not List<hkaiNavVolumePathRequestInfo> castValue) return false;
                instance.m_navVolumePathRequestInfos = castValue;
                return true;
            }
            case "m_navMeshPathRequests":
            case "navMeshPathRequests":
            {
                if (value is not List<hkaiNavMeshPathRequest?> castValue) return false;
                instance.m_navMeshPathRequests = castValue;
                return true;
            }
            case "m_navVolumePathRequests":
            case "navVolumePathRequests":
            {
                if (value is not List<hkaiNavVolumePathRequest?> castValue) return false;
                instance.m_navVolumePathRequests = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
