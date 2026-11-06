// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class CustomManualSelectorGeneratorInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.CustomManualSelectorGeneratorInternalState>
{
    public CustomManualSelectorGeneratorInternalStateData(Havok.hk2014.CustomManualSelectorGeneratorInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_currentGeneratorIndex":
            case "currentGeneratorIndex":
            {
                value = instance.m_currentGeneratorIndex;
                return true;
            }
            case "m_generatorIndexAtActivate":
            case "generatorIndexAtActivate":
            {
                value = instance.m_generatorIndexAtActivate;
                return true;
            }
            case "m_activeTransitions":
            case "activeTransitions":
            {
                value = instance.m_activeTransitions;
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
            case "m_currentGeneratorIndex":
            case "currentGeneratorIndex":
            {
                if (value is not sbyte castValue) return false;
                instance.m_currentGeneratorIndex = castValue;
                return true;
            }
            case "m_generatorIndexAtActivate":
            case "generatorIndexAtActivate":
            {
                if (value is not sbyte castValue) return false;
                instance.m_generatorIndexAtActivate = castValue;
                return true;
            }
            case "m_activeTransitions":
            case "activeTransitions":
            {
                if (value is not List<Havok.hk2014.hkbStateMachine.ActiveTransitionInfo> castValue) return false;
                instance.m_activeTransitions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
