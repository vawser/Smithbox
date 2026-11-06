// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclStateTransitionBlendOpTransitionDataData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclStateTransition.BlendOpTransitionData>
{
    public hclStateTransitionBlendOpTransitionDataData(Havok.hk2016.hclStateTransition.BlendOpTransitionData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bufferASimCloths":
            case "bufferASimCloths":
            {
                value = instance.m_bufferASimCloths;
                return true;
            }
            case "m_bufferBSimCloths":
            case "bufferBSimCloths":
            {
                value = instance.m_bufferBSimCloths;
                return true;
            }
            case "m_transitionType":
            case "transitionType":
            {
                value = instance.m_transitionType;
                return true;
            }
            case "m_blendWeightType":
            case "blendWeightType":
            {
                value = instance.m_blendWeightType;
                return true;
            }
            case "m_blendOperatorId":
            case "blendOperatorId":
            {
                value = instance.m_blendOperatorId;
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
                if (value is Havok.hk2016.hclStateTransition.TransitionType castValue)
                {
                    instance.m_transitionType = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_transitionType = (Havok.hk2016.hclStateTransition.TransitionType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_blendWeightType":
            case "blendWeightType":
            {
                if (value is Havok.hk2016.hclBlendSomeVerticesOperator.BlendWeightType castValue)
                {
                    instance.m_blendWeightType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_blendWeightType = (Havok.hk2016.hclBlendSomeVerticesOperator.BlendWeightType)intValue;
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
