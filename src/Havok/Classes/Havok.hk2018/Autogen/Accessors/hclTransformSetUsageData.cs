// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclTransformSetUsageData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclTransformSetUsage>
{
    private static readonly System.Reflection.FieldInfo _perComponentFlagsInfo = typeof(Havok.hk2018.hclTransformSetUsage).GetField("m_perComponentFlags")!;
    public hclTransformSetUsageData(Havok.hk2018.hclTransformSetUsage instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_perComponentFlags":
            case "perComponentFlags":
            {
                value = instance.m_perComponentFlags;
                return true;
            }
            case "m_perComponentTransformTrackers":
            case "perComponentTransformTrackers":
            {
                value = instance.m_perComponentTransformTrackers;
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
            case "m_perComponentFlags":
            case "perComponentFlags":
            {
                if (value is not byte[] castValue || castValue.Length != 2) return false;
                try
                {
                    _perComponentFlagsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_perComponentTransformTrackers":
            case "perComponentTransformTrackers":
            {
                if (value is not List<Havok.hk2018.hclTransformSetUsage.TransformTracker> castValue) return false;
                instance.m_perComponentTransformTrackers = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
