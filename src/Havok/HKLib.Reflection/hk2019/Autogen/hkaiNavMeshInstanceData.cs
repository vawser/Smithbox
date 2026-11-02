// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshInstanceData : HavokData<HKLib.hk2019.hkaiNavMeshInstance> 
{
    public hkaiNavMeshInstanceData(HavokType type, HKLib.hk2019.hkaiNavMeshInstance instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instanceData":
            case "instanceData":
            {
                if (instance.m_instanceData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceCaches":
            case "clearanceCaches":
            {
                if (instance.m_clearanceCaches is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (instance.m_referenceFrame is not TGet castValue) return false;
                value = castValue;
                return true;
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
            case "m_used":
            case "used":
            {
                if (instance.m_used is not TGet castValue) return false;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_instanceData":
            case "instanceData":
            {
                if (value is not HKLib.hk2019.hkaiCopyOnWritePtr<HKLib.hk2019.hkaiNavMeshInstanceData> castValue) return false;
                instance.m_instanceData = castValue;
                return true;
            }
            case "m_clearanceCaches":
            case "clearanceCaches":
            {
                if (value is not List<HKLib.hk2019.hkaiCopyOnWritePtr<HKLib.hk2019.hkaiNavMeshClearanceCache>> castValue) return false;
                instance.m_clearanceCaches = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is not HKLib.hk2019.hkaiReferenceFrame castValue) return false;
                instance.m_referenceFrame = castValue;
                return true;
            }
            case "m_mediator":
            case "mediator":
            {
                if (value is null)
                {
                    instance.m_mediator = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiNavMeshInstanceQueryMediator castValue)
                {
                    instance.m_mediator = castValue;
                    return true;
                }
                return false;
            }
            case "m_used":
            case "used":
            {
                if (value is not bool castValue) return false;
                instance.m_used = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
