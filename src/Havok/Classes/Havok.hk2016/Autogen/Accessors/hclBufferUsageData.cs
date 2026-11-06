// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclBufferUsageData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclBufferUsage>
{
    private static readonly System.Reflection.FieldInfo _perComponentFlagsInfo = typeof(Havok.hk2016.hclBufferUsage).GetField("m_perComponentFlags")!;
    public hclBufferUsageData(Havok.hk2016.hclBufferUsage instance) : base(instance) { }

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
            case "m_trianglesRead":
            case "trianglesRead":
            {
                value = instance.m_trianglesRead;
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
                if (value is not byte[] castValue || castValue.Length != 4) return false;
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
            case "m_trianglesRead":
            case "trianglesRead":
            {
                if (value is not bool castValue) return false;
                instance.m_trianglesRead = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
