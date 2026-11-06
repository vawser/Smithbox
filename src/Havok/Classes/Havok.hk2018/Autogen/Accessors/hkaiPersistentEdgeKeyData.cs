// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiPersistentEdgeKeyData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiPersistentEdgeKey>
{
    public hkaiPersistentEdgeKeyData(Havok.hk2018.hkaiPersistentEdgeKey instance) : base(instance) { }

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
            case "m_edgeOffset":
            case "edgeOffset":
            {
                value = instance.m_edgeOffset;
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
                if (value is not Havok.hk2018.hkaiPersistentFaceKey castValue) return false;
                instance.m_faceKey = castValue;
                return true;
            }
            case "m_edgeOffset":
            case "edgeOffset":
            {
                if (value is not short castValue) return false;
                instance.m_edgeOffset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
