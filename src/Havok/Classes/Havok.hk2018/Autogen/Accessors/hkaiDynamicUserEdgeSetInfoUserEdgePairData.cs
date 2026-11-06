// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiDynamicUserEdgeSetInfoUserEdgePairData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiDynamicUserEdgeSetInfo.UserEdgePair>
{
    public hkaiDynamicUserEdgeSetInfoUserEdgePairData(Havok.hk2018.hkaiDynamicUserEdgeSetInfo.UserEdgePair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aFaceKey":
            case "aFaceKey":
            {
                value = instance.m_aFaceKey;
                return true;
            }
            case "m_bFaceKey":
            case "bFaceKey":
            {
                value = instance.m_bFaceKey;
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
            case "m_aFaceKey":
            case "aFaceKey":
            {
                if (value is not uint castValue) return false;
                instance.m_aFaceKey = castValue;
                return true;
            }
            case "m_bFaceKey":
            case "bFaceKey":
            {
                if (value is not uint castValue) return false;
                instance.m_bFaceKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
