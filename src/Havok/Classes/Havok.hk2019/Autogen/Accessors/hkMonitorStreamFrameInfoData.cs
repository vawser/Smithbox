// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkMonitorStreamFrameInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkMonitorStreamFrameInfo>
{
    public hkMonitorStreamFrameInfoData(Havok.hk2019.hkMonitorStreamFrameInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_heading":
            case "heading":
            {
                value = instance.m_heading;
                return true;
            }
            case "m_indexOfTimer0":
            case "indexOfTimer0":
            {
                value = instance.m_indexOfTimer0;
                return true;
            }
            case "m_indexOfTimer1":
            case "indexOfTimer1":
            {
                value = instance.m_indexOfTimer1;
                return true;
            }
            case "m_absoluteTimeCounter":
            case "absoluteTimeCounter":
            {
                value = instance.m_absoluteTimeCounter;
                return true;
            }
            case "m_timerFactor0":
            case "timerFactor0":
            {
                value = instance.m_timerFactor0;
                return true;
            }
            case "m_timerFactor1":
            case "timerFactor1":
            {
                value = instance.m_timerFactor1;
                return true;
            }
            case "m_threadId":
            case "threadId":
            {
                value = instance.m_threadId;
                return true;
            }
            case "m_frameStreamStart":
            case "frameStreamStart":
            {
                value = instance.m_frameStreamStart;
                return true;
            }
            case "m_frameStreamEnd":
            case "frameStreamEnd":
            {
                value = instance.m_frameStreamEnd;
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
            case "m_heading":
            case "heading":
            {
                if (value is null)
                {
                    instance.m_heading = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_heading = castValue;
                    return true;
                }
                return false;
            }
            case "m_indexOfTimer0":
            case "indexOfTimer0":
            {
                if (value is not int castValue) return false;
                instance.m_indexOfTimer0 = castValue;
                return true;
            }
            case "m_indexOfTimer1":
            case "indexOfTimer1":
            {
                if (value is not int castValue) return false;
                instance.m_indexOfTimer1 = castValue;
                return true;
            }
            case "m_absoluteTimeCounter":
            case "absoluteTimeCounter":
            {
                if (value is Havok.hk2019.hkMonitorStreamFrameInfo.AbsoluteTimeCounter castValue)
                {
                    instance.m_absoluteTimeCounter = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_absoluteTimeCounter = (Havok.hk2019.hkMonitorStreamFrameInfo.AbsoluteTimeCounter)intValue;
                    return true;
                }
                return false;
            }
            case "m_timerFactor0":
            case "timerFactor0":
            {
                if (value is not float castValue) return false;
                instance.m_timerFactor0 = castValue;
                return true;
            }
            case "m_timerFactor1":
            case "timerFactor1":
            {
                if (value is not float castValue) return false;
                instance.m_timerFactor1 = castValue;
                return true;
            }
            case "m_threadId":
            case "threadId":
            {
                if (value is not int castValue) return false;
                instance.m_threadId = castValue;
                return true;
            }
            case "m_frameStreamStart":
            case "frameStreamStart":
            {
                if (value is not int castValue) return false;
                instance.m_frameStreamStart = castValue;
                return true;
            }
            case "m_frameStreamEnd":
            case "frameStreamEnd":
            {
                if (value is not int castValue) return false;
                instance.m_frameStreamEnd = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
