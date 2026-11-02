// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxAnimationDataCurveData : HavokData<HKLib.hk2019.hkxAnimationData.Curve> 
{
    public hkxAnimationDataCurveData(HavokType type, HKLib.hk2019.hkxAnimationData.Curve instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_type is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_keys":
            case "keys":
            {
                if (instance.m_keys is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timeStart":
            case "timeStart":
            {
                if (instance.m_timeStart is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timeEnd":
            case "timeEnd":
            {
                if (instance.m_timeEnd is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_beforeFirst":
            case "beforeFirst":
            {
                if (instance.m_beforeFirst is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_beforeFirst is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_beforeFirstCount":
            case "beforeFirstCount":
            {
                if (instance.m_beforeFirstCount is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_afterLast":
            case "afterLast":
            {
                if (instance.m_afterLast is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_afterLast is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_afterLastCount":
            case "afterLastCount":
            {
                if (instance.m_afterLastCount is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ownerType":
            case "ownerType":
            {
                if (instance.m_ownerType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_ownerType is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_channelName":
            case "channelName":
            {
                if (instance.m_channelName is null)
                {
                    return true;
                }
                if (instance.m_channelName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_ownerName":
            case "ownerName":
            {
                if (instance.m_ownerName is null)
                {
                    return true;
                }
                if (instance.m_ownerName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_enums":
            case "enums":
            {
                if (instance.m_enums is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is HKLib.hk2019.hkxAnimationData.CurveType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_type = (HKLib.hk2019.hkxAnimationData.CurveType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_keys":
            case "keys":
            {
                if (value is not List<HKLib.hk2019.hkxAnimationData.CurveKey> castValue) return false;
                instance.m_keys = castValue;
                return true;
            }
            case "m_timeStart":
            case "timeStart":
            {
                if (value is not double castValue) return false;
                instance.m_timeStart = castValue;
                return true;
            }
            case "m_timeEnd":
            case "timeEnd":
            {
                if (value is not double castValue) return false;
                instance.m_timeEnd = castValue;
                return true;
            }
            case "m_beforeFirst":
            case "beforeFirst":
            {
                if (value is HKLib.hk2019.hkxAnimationData.ExtrapolationType castValue)
                {
                    instance.m_beforeFirst = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_beforeFirst = (HKLib.hk2019.hkxAnimationData.ExtrapolationType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_beforeFirstCount":
            case "beforeFirstCount":
            {
                if (value is not ulong castValue) return false;
                instance.m_beforeFirstCount = castValue;
                return true;
            }
            case "m_afterLast":
            case "afterLast":
            {
                if (value is HKLib.hk2019.hkxAnimationData.ExtrapolationType castValue)
                {
                    instance.m_afterLast = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_afterLast = (HKLib.hk2019.hkxAnimationData.ExtrapolationType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_afterLastCount":
            case "afterLastCount":
            {
                if (value is not ulong castValue) return false;
                instance.m_afterLastCount = castValue;
                return true;
            }
            case "m_ownerType":
            case "ownerType":
            {
                if (value is HKLib.hk2019.hkxAnimationData.CurveOwnerType castValue)
                {
                    instance.m_ownerType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_ownerType = (HKLib.hk2019.hkxAnimationData.CurveOwnerType)intValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_channelName":
            case "channelName":
            {
                if (value is null)
                {
                    instance.m_channelName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_channelName = castValue;
                    return true;
                }
                return false;
            }
            case "m_ownerName":
            case "ownerName":
            {
                if (value is null)
                {
                    instance.m_ownerName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_ownerName = castValue;
                    return true;
                }
                return false;
            }
            case "m_enums":
            case "enums":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_enums = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
