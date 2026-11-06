// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxAnimationDataCurveData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxAnimationData.Curve>
{
    public hkxAnimationDataCurveData(Havok.hk2019.hkxAnimationData.Curve instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_keys":
            case "keys":
            {
                value = instance.m_keys;
                return true;
            }
            case "m_timeStart":
            case "timeStart":
            {
                value = instance.m_timeStart;
                return true;
            }
            case "m_timeEnd":
            case "timeEnd":
            {
                value = instance.m_timeEnd;
                return true;
            }
            case "m_beforeFirst":
            case "beforeFirst":
            {
                value = instance.m_beforeFirst;
                return true;
            }
            case "m_beforeFirstCount":
            case "beforeFirstCount":
            {
                value = instance.m_beforeFirstCount;
                return true;
            }
            case "m_afterLast":
            case "afterLast":
            {
                value = instance.m_afterLast;
                return true;
            }
            case "m_afterLastCount":
            case "afterLastCount":
            {
                value = instance.m_afterLastCount;
                return true;
            }
            case "m_ownerType":
            case "ownerType":
            {
                value = instance.m_ownerType;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_channelName":
            case "channelName":
            {
                value = instance.m_channelName;
                return true;
            }
            case "m_ownerName":
            case "ownerName":
            {
                value = instance.m_ownerName;
                return true;
            }
            case "m_enums":
            case "enums":
            {
                value = instance.m_enums;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2019.hkxAnimationData.CurveType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_type = (Havok.hk2019.hkxAnimationData.CurveType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_keys":
            case "keys":
            {
                if (value is not List<Havok.hk2019.hkxAnimationData.CurveKey> castValue) return false;
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
                if (value is Havok.hk2019.hkxAnimationData.ExtrapolationType castValue)
                {
                    instance.m_beforeFirst = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_beforeFirst = (Havok.hk2019.hkxAnimationData.ExtrapolationType)uintValue;
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
                if (value is Havok.hk2019.hkxAnimationData.ExtrapolationType castValue)
                {
                    instance.m_afterLast = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_afterLast = (Havok.hk2019.hkxAnimationData.ExtrapolationType)uintValue;
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
                if (value is Havok.hk2019.hkxAnimationData.CurveOwnerType castValue)
                {
                    instance.m_ownerType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_ownerType = (Havok.hk2019.hkxAnimationData.CurveOwnerType)intValue;
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
