// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiPersistentEdgeKeyData : HavokData<hkaiPersistentEdgeKey> 
{
    public hkaiPersistentEdgeKeyData(HavokType type, hkaiPersistentEdgeKey instance) : base(type, instance) {}

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
                if (value is not hkaiPersistentFaceKey castValue) return false;
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
