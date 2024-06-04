// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbHandIkControlsModifierHandData : HavokData<hkbHandIkControlsModifier.Hand> 
{
    public hkbHandIkControlsModifierHandData(HavokType type, hkbHandIkControlsModifier.Hand instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_controlData":
            case "controlData":
            {
                if (instance.m_controlData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handIndex":
            case "handIndex":
            {
                if (instance.m_handIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                if (instance.m_enable is not TGet castValue) return false;
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
            case "m_controlData":
            case "controlData":
            {
                if (value is not hkbHandIkControlData castValue) return false;
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
