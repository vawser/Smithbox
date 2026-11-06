// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclStaticDisplayBufferSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclStaticDisplayBufferSetupObject>
{
    public hclStaticDisplayBufferSetupObjectData(Havok.hk2014.hclStaticDisplayBufferSetupObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_setupMesh":
            case "setupMesh":
            {
                value = instance.m_setupMesh;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
            case "m_setupMesh":
            case "setupMesh":
            {
                if (value is null)
                {
                    instance.m_setupMesh = default;
                    return true;
                }
                if (value is Havok.hk2014.hclSetupMesh castValue)
                {
                    instance.m_setupMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
