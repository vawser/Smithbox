// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiPersistentEdgeKeyData : HavokData<HKLib.hk2019.hkaiPersistentEdgeKey> 
{
    public hkaiPersistentEdgeKeyData(HavokType type, HKLib.hk2019.hkaiPersistentEdgeKey instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceKey":
            case "faceKey":
            {
                if (instance.m_faceKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeOffset":
            case "edgeOffset":
            {
                if (instance.m_edgeOffset is not TGet castValue) return false;
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
            case "m_faceKey":
            case "faceKey":
            {
                if (value is not HKLib.hk2019.hkaiPersistentFaceKey castValue) return false;
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
