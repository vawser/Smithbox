// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshPathRequestInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshPathRequestInfo>
{
    public hkaiNavMeshPathRequestInfoData(Havok.hk2019.hkaiNavMeshPathRequestInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_input":
            case "input":
            {
                value = instance.m_input;
                return true;
            }
            case "m_output":
            case "output":
            {
                value = instance.m_output;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                value = instance.m_priority;
                return true;
            }
            case "m_queueId":
            case "queueId":
            {
                value = instance.m_queueId;
                return true;
            }
            case "m_markedForDeletion":
            case "markedForDeletion":
            {
                value = instance.m_markedForDeletion;
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
            case "m_input":
            case "input":
            {
                if (value is null)
                {
                    instance.m_input = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiPathfindingUtil.FindPathInput castValue)
                {
                    instance.m_input = castValue;
                    return true;
                }
                return false;
            }
            case "m_output":
            case "output":
            {
                if (value is null)
                {
                    instance.m_output = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiPathfindingUtil.FindPathOutput castValue)
                {
                    instance.m_output = castValue;
                    return true;
                }
                return false;
            }
            case "m_priority":
            case "priority":
            {
                if (value is not int castValue) return false;
                instance.m_priority = castValue;
                return true;
            }
            case "m_queueId":
            case "queueId":
            {
                if (value is not Havok.hk2019.hkHandle<byte> castValue) return false;
                instance.m_queueId = castValue;
                return true;
            }
            case "m_markedForDeletion":
            case "markedForDeletion":
            {
                if (value is not bool castValue) return false;
                instance.m_markedForDeletion = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
