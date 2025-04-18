// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbEvaluateExpressionModifierInternalExpressionDataData : HavokData<hkbEvaluateExpressionModifier.InternalExpressionData> 
{
    public hkbEvaluateExpressionModifierInternalExpressionDataData(HavokType type, hkbEvaluateExpressionModifier.InternalExpressionData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_raisedEvent":
            case "raisedEvent":
            {
                if (instance.m_raisedEvent is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wasTrueInPreviousFrame":
            case "wasTrueInPreviousFrame":
            {
                if (instance.m_wasTrueInPreviousFrame is not TGet castValue) return false;
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
            case "m_raisedEvent":
            case "raisedEvent":
            {
                if (value is not bool castValue) return false;
                instance.m_raisedEvent = castValue;
                return true;
            }
            case "m_wasTrueInPreviousFrame":
            case "wasTrueInPreviousFrame":
            {
                if (value is not bool castValue) return false;
                instance.m_wasTrueInPreviousFrame = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
