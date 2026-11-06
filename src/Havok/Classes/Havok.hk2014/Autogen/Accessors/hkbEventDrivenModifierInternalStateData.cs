// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbEventDrivenModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbEventDrivenModifierInternalState>
{
    public hkbEventDrivenModifierInternalStateData(Havok.hk2014.hkbEventDrivenModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_isActive":
            case "isActive":
            {
                value = instance.m_isActive;
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
            case "m_isActive":
            case "isActive":
            {
                if (value is not bool castValue) return false;
                instance.m_isActive = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
