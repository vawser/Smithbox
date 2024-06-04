// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStateTransitionSimClothTransitionDataData : HavokData<hclStateTransition.SimClothTransitionData> 
{
    public hclStateTransitionSimClothTransitionDataData(HavokType type, hclStateTransition.SimClothTransitionData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_isSimulated":
            case "isSimulated":
            {
                if (instance.m_isSimulated is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transitionConstraints":
            case "transitionConstraints":
            {
                if (instance.m_transitionConstraints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transitionType":
            case "transitionType":
            {
                if (instance.m_transitionType is not TGet castValue) return false;
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
                if (value is not List<hkHandle<uint>> castValue) return false;
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
