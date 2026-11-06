// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbHandIkControlsModifierHandData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbHandIkControlsModifier.Hand>
{
    public hkbHandIkControlsModifierHandData(Havok.hk2016.hkbHandIkControlsModifier.Hand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_controlData":
            case "controlData":
            {
                value = instance.m_controlData;
                return true;
            }
            case "m_handIndex":
            case "handIndex":
            {
                value = instance.m_handIndex;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
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
            case "m_controlData":
            case "controlData":
            {
                if (value is not Havok.hk2016.hkbHandIkControlData castValue) return false;
                instance.m_controlData = castValue;
                return true;
            }
            case "m_handIndex":
            case "handIndex":
            {
                if (value is not int castValue) return false;
                instance.m_handIndex = castValue;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
