// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiPathRequestQueueData : HavokData<HKLib.hk2019.hkaiPathRequestQueue> 
{
    public hkaiPathRequestQueueData(HavokType type, HKLib.hk2019.hkaiPathRequestQueue instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_queueId":
            case "queueId":
            {
                if (value is not HKLib.hk2019.hkHandle<byte> castValue) return false;
                instance.m_queueId = castValue;
                return true;
            }
            case "m_navMeshPathRequestInfos":
            case "navMeshPathRequestInfos":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshPathRequestInfo> castValue) return false;
                instance.m_navMeshPathRequestInfos = castValue;
                return true;
            }
            case "m_navVolumePathRequestInfos":
            case "navVolumePathRequestInfos":
            {
                if (value is not List<HKLib.hk2019.hkaiNavVolumePathRequestInfo> castValue) return false;
                instance.m_navVolumePathRequestInfos = castValue;
                return true;
            }
            case "m_navMeshPathRequests":
            case "navMeshPathRequests":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshPathRequest?> castValue) return false;
                instance.m_navMeshPathRequests = castValue;
                return true;
            }
            case "m_navVolumePathRequests":
            case "navVolumePathRequests":
            {
                if (value is not List<HKLib.hk2019.hkaiNavVolumePathRequest?> castValue) return false;
                instance.m_navVolumePathRequests = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
