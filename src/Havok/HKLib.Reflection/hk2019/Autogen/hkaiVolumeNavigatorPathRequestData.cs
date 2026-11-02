// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiVolumeNavigatorPathRequestData : HavokData<HKLib.hk2019.hkaiVolumeNavigator.PathRequest> 
{
    public hkaiVolumeNavigatorPathRequestData(HavokType type, HKLib.hk2019.hkaiVolumeNavigator.PathRequest instance) : base(type, instance) {}

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
            case "m_priority":
            case "priority":
            {
                if (instance.m_priority is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_requestState":
            case "requestState":
            {
                if (instance.m_requestState is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_input":
            case "input":
            {
                if (instance.m_input is null)
                {
                    return true;
                }
                if (instance.m_input is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_output":
            case "output":
            {
                if (instance.m_output is null)
                {
                    return true;
                }
                if (instance.m_output is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_navigator":
            case "navigator":
            {
                if (instance.m_navigator is null)
                {
                    return true;
                }
                if (instance.m_navigator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_pathRequestInput":
            case "pathRequestInput":
            {
                if (instance.m_pathRequestInput is null)
                {
                    return true;
                }
                if (instance.m_pathRequestInput is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
                if (value is not HKLib.hk2019.hkAtomic.Variable<HKLib.hk2019.hkaiPathRequestBase.RequestState> castValue) return false;
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
                if (value is HKLib.hk2019.hkaiVolumePathfindingUtil.FindPathInput castValue)
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
                if (value is HKLib.hk2019.hkaiVolumePathfindingUtil.FindPathOutput castValue)
                {
                    instance.m_output = castValue;
                    return true;
                }
                return false;
            }
            case "m_navigator":
            case "navigator":
            {
                if (value is null)
                {
                    instance.m_navigator = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiVolumeNavigator castValue)
                {
                    instance.m_navigator = castValue;
                    return true;
                }
                return false;
            }
            case "m_pathRequestInput":
            case "pathRequestInput":
            {
                if (value is null)
                {
                    instance.m_pathRequestInput = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiVolumeNavigator.PathRequestInput castValue)
                {
                    instance.m_pathRequestInput = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
