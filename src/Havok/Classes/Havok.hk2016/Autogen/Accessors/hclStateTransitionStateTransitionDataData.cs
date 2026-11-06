// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclStateTransitionStateTransitionDataData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclStateTransition.StateTransitionData>
{
    public hclStateTransitionStateTransitionDataData(Havok.hk2016.hclStateTransition.StateTransitionData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_simClothTransitionData":
            case "simClothTransitionData":
            {
                value = instance.m_simClothTransitionData;
                return true;
            }
            case "m_blendOpTransitionData":
            case "blendOpTransitionData":
            {
                value = instance.m_blendOpTransitionData;
                return true;
            }
            case "m_simulatedState":
            case "simulatedState":
            {
                value = instance.m_simulatedState;
                return true;
            }
            case "m_emptyState":
            case "emptyState":
            {
                value = instance.m_emptyState;
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
            case "m_simClothTransitionData":
            case "simClothTransitionData":
            {
                if (value is not List<Havok.hk2016.hclStateTransition.SimClothTransitionData> castValue) return false;
                instance.m_simClothTransitionData = castValue;
                return true;
            }
            case "m_blendOpTransitionData":
            case "blendOpTransitionData":
            {
                if (value is not List<Havok.hk2016.hclStateTransition.BlendOpTransitionData> castValue) return false;
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
