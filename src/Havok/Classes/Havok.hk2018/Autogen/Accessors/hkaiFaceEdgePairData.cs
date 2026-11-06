// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiFaceEdgePairData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiFaceEdgePair>
{
    public hkaiFaceEdgePairData(Havok.hk2018.hkaiFaceEdgePair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceKey":
            case "faceKey":
            {
                value = instance.m_faceKey;
                return true;
            }
            case "m_edgeKey":
            case "edgeKey":
            {
                value = instance.m_edgeKey;
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
            case "m_faceKey":
            case "faceKey":
            {
                if (value is not uint castValue) return false;
                instance.m_faceKey = castValue;
                return true;
            }
            case "m_edgeKey":
            case "edgeKey":
            {
                if (value is not uint castValue) return false;
                instance.m_edgeKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
