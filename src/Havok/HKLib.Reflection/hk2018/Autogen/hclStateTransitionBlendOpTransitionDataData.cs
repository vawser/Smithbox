// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStateTransitionBlendOpTransitionDataData : HavokData<hclStateTransition.BlendOpTransitionData> 
{
    public hclStateTransitionBlendOpTransitionDataData(HavokType type, hclStateTransition.BlendOpTransitionData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bufferASimCloths":
            case "bufferASimCloths":
            {
                if (instance.m_bufferASimCloths is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bufferBSimCloths":
            case "bufferBSimCloths":
            {
                if (instance.m_bufferBSimCloths is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transitionType":
            case "transitionType":
            {
                if (instance.m_transitionType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_transitionType is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_blendWeightType":
            case "blendWeightType":
            {
                if (instance.m_blendWeightType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_blendWeightType is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_blendOperatorId":
            case "blendOperatorId":
            {
                if (instance.m_blendOperatorId is not TGet castValue) return false;
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
            case "m_bufferASimCloths":
            case "bufferASimCloths":
            {
                if (value is not List<int> castValue) return false;
                instance.m_bufferASimCloths = castValue;
                return true;
            }
            case "m_bufferBSimCloths":
            case "bufferBSimCloths":
            {
                if (value is not List<int> castValue) return false;
                instance.m_bufferBSimCloths = castValue;
                return true;
            }
            case "m_transitionType":
            case "transitionType":
            {
                if (value is hclStateTransition.TransitionType castValue)
                {
                    instance.m_transitionType = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_transitionType = (hclStateTransition.TransitionType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_blendWeightType":
            case "blendWeightType":
            {
                if (value is hclBlendSomeVerticesOperator.BlendWeightType castValue)
                {
                    instance.m_blendWeightType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_blendWeightType = (hclBlendSomeVerticesOperator.BlendWeightType)intValue;
                    return true;
                }
                return false;
            }
            case "m_blendOperatorId":
            case "blendOperatorId":
            {
                if (value is not uint castValue) return false;
                instance.m_blendOperatorId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
