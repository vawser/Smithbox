// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavVolumePathRequestData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavVolumePathRequest>
{
    public hkaiNavVolumePathRequestData(Havok.hk2019.hkaiNavVolumePathRequest instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_queueId":
            case "queueId":
            {
                value = instance.m_queueId;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                value = instance.m_priority;
                return true;
            }
            case "m_requestState":
            case "requestState":
            {
                value = instance.m_requestState;
                return true;
            }
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
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_queueId":
            case "queueId":
            {
                if (value is not Havok.hk2019.hkHandle<byte> castValue) return false;
                instance.m_queueId = castValue;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                if (value is not int castValue) return false;
                instance.m_priority = castValue;
                return true;
            }
            case "m_requestState":
            case "requestState":
            {
                if (value is not Havok.hk2019.hkAtomic.Variable<Havok.hk2019.hkaiPathRequestBase.RequestState> castValue) return false;
                instance.m_requestState = castValue;
                return true;
            }
            case "m_input":
            case "input":
            {
                if (value is null)
                {
                    instance.m_input = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiVolumePathfindingUtil.FindPathInput castValue)
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
                if (value is Havok.hk2019.hkaiVolumePathfindingUtil.FindPathOutput castValue)
                {
                    instance.m_output = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
