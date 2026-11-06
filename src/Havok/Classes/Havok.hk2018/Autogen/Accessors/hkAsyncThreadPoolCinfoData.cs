// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkAsyncThreadPoolCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkAsyncThreadPool.Cinfo>
{
    public hkAsyncThreadPoolCinfoData(Havok.hk2018.hkAsyncThreadPool.Cinfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_hardwareThreadBinding":
            case "hardwareThreadBinding":
            {
                value = instance.m_hardwareThreadBinding;
                return true;
            }
            case "m_numThreads":
            case "numThreads":
            {
                value = instance.m_numThreads;
                return true;
            }
            case "m_hardwareThreadMasksOrIds":
            case "hardwareThreadMasksOrIds":
            {
                value = instance.m_hardwareThreadMasksOrIds;
                return true;
            }
            case "m_stackSize":
            case "stackSize":
            {
                value = instance.m_stackSize;
                return true;
            }
            case "m_threadName":
            case "threadName":
            {
                value = instance.m_threadName;
                return true;
            }
            case "m_normalPriority":
            case "normalPriority":
            {
                value = instance.m_normalPriority;
                return true;
            }
            case "m_lowPriority":
            case "lowPriority":
            {
                value = instance.m_lowPriority;
                return true;
            }
            case "m_threadBackgroundPriorities":
            case "threadBackgroundPriorities":
            {
                value = instance.m_threadBackgroundPriorities;
                return true;
            }
            case "m_timerBufferPerThreadAllocation":
            case "timerBufferPerThreadAllocation":
            {
                value = instance.m_timerBufferPerThreadAllocation;
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
            case "m_hardwareThreadBinding":
            case "hardwareThreadBinding":
            {
                if (value is Havok.hk2018.hkAsyncThreadPool.HardwareThreadBinding castValue)
                {
                    instance.m_hardwareThreadBinding = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_hardwareThreadBinding = (Havok.hk2018.hkAsyncThreadPool.HardwareThreadBinding)byteValue;
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
                if (value is not List<Havok.hk2018.hkAsyncThreadPool.ThreadPriority> castValue) return false;
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
