// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStateTransitionStateTransitionDataData : HavokData<hclStateTransition.StateTransitionData> 
{
    public hclStateTransitionStateTransitionDataData(HavokType type, hclStateTransition.StateTransitionData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_simClothTransitionData":
            case "simClothTransitionData":
            {
                if (instance.m_simClothTransitionData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendOpTransitionData":
            case "blendOpTransitionData":
            {
                if (instance.m_blendOpTransitionData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simulatedState":
            case "simulatedState":
            {
                if (instance.m_simulatedState is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_emptyState":
            case "emptyState":
            {
                if (instance.m_emptyState is not TGet castValue) return false;
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
            case "m_simClothTransitionData":
            case "simClothTransitionData":
            {
                if (value is not List<hclStateTransition.SimClothTransitionData> castValue) return false;
                instance.m_simClothTransitionData = castValue;
                return true;
            }
            case "m_blendOpTransitionData":
            case "blendOpTransitionData":
            {
                if (value is not List<hclStateTransition.BlendOpTransitionData> castValue) return false;
                instance.m_blendOpTransitionData = castValue;
                return true;
            }
            case "m_simulatedState":
            case "simulatedState":
            {
                if (value is not bool castValue) return false;
                instance.m_simulatedState = castValue;
                return true;
            }
            case "m_emptyState":
            case "emptyState":
            {
                if (value is not bool castValue) return false;
                instance.m_emptyState = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
