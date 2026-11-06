// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclTransformSetUsageTransformTrackerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclTransformSetUsage.TransformTracker>
{
    public hclTransformSetUsageTransformTrackerData(Havok.hk2014.hclTransformSetUsage.TransformTracker instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_read":
            case "read":
            {
                value = instance.m_read;
                return true;
            }
            case "m_readBeforeWrite":
            case "readBeforeWrite":
            {
                value = instance.m_readBeforeWrite;
                return true;
            }
            case "m_written":
            case "written":
            {
                value = instance.m_written;
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
            case "m_read":
            case "read":
            {
                if (value is not Havok.hk2014.hkBitField castValue) return false;
                instance.m_read = castValue;
                return true;
            }
            case "m_readBeforeWrite":
            case "readBeforeWrite":
            {
                if (value is not Havok.hk2014.hkBitField castValue) return false;
                instance.m_readBeforeWrite = castValue;
                return true;
            }
            case "m_written":
            case "written":
            {
                if (value is not Havok.hk2014.hkBitField castValue) return false;
                instance.m_written = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
