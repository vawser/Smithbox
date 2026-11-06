// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclStateTransitionSimClothTransitionDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclStateTransition.SimClothTransitionData>
{
    public hclStateTransitionSimClothTransitionDataData(Havok.hk2018.hclStateTransition.SimClothTransitionData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_isSimulated":
            case "isSimulated":
            {
                value = instance.m_isSimulated;
                return true;
            }
            case "m_transitionConstraints":
            case "transitionConstraints":
            {
                value = instance.m_transitionConstraints;
                return true;
            }
            case "m_transitionType":
            case "transitionType":
            {
                value = instance.m_transitionType;
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
            case "m_isSimulated":
            case "isSimulated":
            {
                if (value is not bool castValue) return false;
                instance.m_isSimulated = castValue;
                return true;
            }
            case "m_transitionConstraints":
            case "transitionConstraints":
            {
                if (value is not List<Havok.hk2018.hkHandle<uint>> castValue) return false;
                instance.m_transitionConstraints = castValue;
                return true;
            }
            case "m_transitionType":
            case "transitionType":
            {
                if (value is not uint castValue) return false;
                instance.m_transitionType = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
