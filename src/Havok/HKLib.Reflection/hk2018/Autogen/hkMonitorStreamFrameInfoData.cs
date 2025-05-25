// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMonitorStreamFrameInfoData : HavokData<hkMonitorStreamFrameInfo> 
{
    public hkMonitorStreamFrameInfoData(HavokType type, hkMonitorStreamFrameInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_heading":
            case "heading":
            {
                if (instance.m_heading is null)
                {
                    return true;
                }
                if (instance.m_heading is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_indexOfTimer0":
            case "indexOfTimer0":
            {
                if (instance.m_indexOfTimer0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indexOfTimer1":
            case "indexOfTimer1":
            {
                if (instance.m_indexOfTimer1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_absoluteTimeCounter":
            case "absoluteTimeCounter":
            {
                if (instance.m_absoluteTimeCounter is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_absoluteTimeCounter is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_timerFactor0":
            case "timerFactor0":
            {
                if (instance.m_timerFactor0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timerFactor1":
            case "timerFactor1":
            {
                if (instance.m_timerFactor1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_threadId":
            case "threadId":
            {
                if (instance.m_threadId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frameStreamStart":
            case "frameStreamStart":
            {
                if (instance.m_frameStreamStart is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frameStreamEnd":
            case "frameStreamEnd":
            {
                if (instance.m_frameStreamEnd is not TGet castValue) return false;
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
                if (value is hkMonitorStreamFrameInfo.AbsoluteTimeCounter castValue)
                {
                    instance.m_absoluteTimeCounter = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_absoluteTimeCounter = (hkMonitorStreamFrameInfo.AbsoluteTimeCounter)uintValue;
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
