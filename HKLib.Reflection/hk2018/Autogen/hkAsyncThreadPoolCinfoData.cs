// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkAsyncThreadPool;
using ThreadPriority = HKLib.hk2018.hkAsyncThreadPool.ThreadPriority;

namespace HKLib.Reflection.hk2018;

internal class hkAsyncThreadPoolCinfoData : HavokData<Cinfo> 
{
    public hkAsyncThreadPoolCinfoData(HavokType type, Cinfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_hardwareThreadBinding":
            case "hardwareThreadBinding":
            {
                if (instance.m_hardwareThreadBinding is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_hardwareThreadBinding is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_numThreads":
            case "numThreads":
            {
                if (instance.m_numThreads is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hardwareThreadMasksOrIds":
            case "hardwareThreadMasksOrIds":
            {
                if (instance.m_hardwareThreadMasksOrIds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stackSize":
            case "stackSize":
            {
                if (instance.m_stackSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_threadName":
            case "threadName":
            {
                if (instance.m_threadName is null)
                {
                    return true;
                }
                if (instance.m_threadName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_normalPriority":
            case "normalPriority":
            {
                if (instance.m_normalPriority is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lowPriority":
            case "lowPriority":
            {
                if (instance.m_lowPriority is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_threadBackgroundPriorities":
            case "threadBackgroundPriorities":
            {
                if (instance.m_threadBackgroundPriorities is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timerBufferPerThreadAllocation":
            case "timerBufferPerThreadAllocation":
            {
                if (instance.m_timerBufferPerThreadAllocation is not TGet castValue) return false;
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
            case "m_hardwareThreadBinding":
            case "hardwareThreadBinding":
            {
                if (value is HardwareThreadBinding castValue)
                {
                    instance.m_hardwareThreadBinding = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_hardwareThreadBinding = (HardwareThreadBinding)byteValue;
                    return true;
                }
                return false;
            }
            case "m_numThreads":
            case "numThreads":
            {
                if (value is not int castValue) return false;
                instance.m_numThreads = castValue;
                return true;
            }
            case "m_hardwareThreadMasksOrIds":
            case "hardwareThreadMasksOrIds":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_hardwareThreadMasksOrIds = castValue;
                return true;
            }
            case "m_stackSize":
            case "stackSize":
            {
                if (value is not int castValue) return false;
                instance.m_stackSize = castValue;
                return true;
            }
            case "m_threadName":
            case "threadName":
            {
                if (value is null)
                {
                    instance.m_threadName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_threadName = castValue;
                    return true;
                }
                return false;
            }
            case "m_normalPriority":
            case "normalPriority":
            {
                if (value is not int castValue) return false;
                instance.m_normalPriority = castValue;
                return true;
            }
            case "m_lowPriority":
            case "lowPriority":
            {
                if (value is not int castValue) return false;
                instance.m_lowPriority = castValue;
                return true;
            }
            case "m_threadBackgroundPriorities":
            case "threadBackgroundPriorities":
            {
                if (value is not List<ThreadPriority> castValue) return false;
                instance.m_threadBackgroundPriorities = castValue;
                return true;
            }
            case "m_timerBufferPerThreadAllocation":
            case "timerBufferPerThreadAllocation":
            {
                if (value is not int castValue) return false;
                instance.m_timerBufferPerThreadAllocation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
