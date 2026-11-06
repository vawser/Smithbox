// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpMoppCodeReindexedTerminalData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpMoppCodeReindexedTerminal>
{
    public hkpMoppCodeReindexedTerminalData(Havok.hk2014.hkpMoppCodeReindexedTerminal instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_origShapeKey":
            case "origShapeKey":
            {
                value = instance.m_origShapeKey;
                return true;
            }
            case "m_reindexedShapeKey":
            case "reindexedShapeKey":
            {
                value = instance.m_reindexedShapeKey;
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
            case "m_origShapeKey":
            case "origShapeKey":
            {
                if (value is not uint castValue) return false;
                instance.m_origShapeKey = castValue;
                return true;
            }
            case "m_reindexedShapeKey":
            case "reindexedShapeKey":
            {
                if (value is not uint castValue) return false;
                instance.m_reindexedShapeKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
