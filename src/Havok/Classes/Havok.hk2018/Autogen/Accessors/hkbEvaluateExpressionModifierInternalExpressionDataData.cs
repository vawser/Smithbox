// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbEvaluateExpressionModifierInternalExpressionDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbEvaluateExpressionModifier.InternalExpressionData>
{
    public hkbEvaluateExpressionModifierInternalExpressionDataData(Havok.hk2018.hkbEvaluateExpressionModifier.InternalExpressionData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_raisedEvent":
            case "raisedEvent":
            {
                value = instance.m_raisedEvent;
                return true;
            }
            case "m_wasTrueInPreviousFrame":
            case "wasTrueInPreviousFrame":
            {
                value = instance.m_wasTrueInPreviousFrame;
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
