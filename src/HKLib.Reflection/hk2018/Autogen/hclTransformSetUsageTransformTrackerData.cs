// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclTransformSetUsageTransformTrackerData : HavokData<hclTransformSetUsage.TransformTracker> 
{
    public hclTransformSetUsageTransformTrackerData(HavokType type, hclTransformSetUsage.TransformTracker instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_read":
            case "read":
            {
                if (instance.m_read is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_readBeforeWrite":
            case "readBeforeWrite":
            {
                if (instance.m_readBeforeWrite is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_written":
            case "written":
            {
                if (instance.m_written is not TGet castValue) return false;
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
            case "m_read":
            case "read":
            {
                if (value is not hkBitField castValue) return false;
                instance.m_read = castValue;
                return true;
            }
            case "m_readBeforeWrite":
            case "readBeforeWrite":
            {
                if (value is not hkBitField castValue) return false;
                instance.m_readBeforeWrite = castValue;
                return true;
            }
            case "m_written":
            case "written":
            {
                if (value is not hkBitField castValue) return false;
                instance.m_written = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
