// Automatically Generated

#nullable enable

namespace Havok.hk2012.Accessors;

internal sealed class hclTransformSetUsageData : Havok.Formats.HavokFieldAccess<Havok.hk2012.hclTransformSetUsage>
{
    private static readonly System.Reflection.FieldInfo _perComponentFlagsInfo = typeof(Havok.hk2012.hclTransformSetUsage).GetField("m_perComponentFlags")!;
    public hclTransformSetUsageData(Havok.hk2012.hclTransformSetUsage instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
