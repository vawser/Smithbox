// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxVertexBufferData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxVertexBuffer>
{
    public hkxVertexBufferData(Havok.hk55.hkxVertexBuffer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_format":
            case "format":
            {
                value = instance.m_format;
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
            case "m_format":
            case "format":
            {
                if (value is null)
                {
                    instance.m_format = default;
                    return true;
                }
                if (value is Havok.hk55.hkxVertexFormat castValue)
                {
                    instance.m_format = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
