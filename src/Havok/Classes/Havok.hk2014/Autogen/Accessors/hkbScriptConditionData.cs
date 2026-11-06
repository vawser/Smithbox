// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbScriptConditionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbScriptCondition>
{
    public hkbScriptConditionData(Havok.hk2014.hkbScriptCondition instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_conditionScript":
            case "conditionScript":
            {
                value = instance.m_conditionScript;
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
            case "m_conditionScript":
            case "conditionScript":
            {
                if (value is null)
                {
                    instance.m_conditionScript = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_conditionScript = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
