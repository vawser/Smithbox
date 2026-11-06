// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshInstance>
{
    public hkaiNavMeshInstanceData(Havok.hk2019.hkaiNavMeshInstance instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_instanceData":
            case "instanceData":
            {
                value = instance.m_instanceData;
                return true;
            }
            case "m_clearanceCaches":
            case "clearanceCaches":
            {
                value = instance.m_clearanceCaches;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                value = instance.m_referenceFrame;
                return true;
            }
            case "m_mediator":
            case "mediator":
            {
                value = instance.m_mediator;
                return true;
            }
            case "m_used":
            case "used":
            {
                value = instance.m_used;
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
                if (value is not Havok.hk2019.hkaiCopyOnWritePtr<Havok.hk2019.hkaiNavMeshInstanceData> castValue) return false;
                instance.m_instanceData = castValue;
                return true;
            }
            case "m_clearanceCaches":
            case "clearanceCaches":
            {
                if (value is not List<Havok.hk2019.hkaiCopyOnWritePtr<Havok.hk2019.hkaiNavMeshClearanceCache>> castValue) return false;
                instance.m_clearanceCaches = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is not Havok.hk2019.hkaiReferenceFrame castValue) return false;
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
                if (value is Havok.hk2019.hkaiNavMeshInstanceQueryMediator castValue)
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
