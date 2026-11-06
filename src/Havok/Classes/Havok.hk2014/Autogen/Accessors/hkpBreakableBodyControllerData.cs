// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpBreakableBodyControllerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpBreakableBodyController>
{
    public hkpBreakableBodyControllerData(Havok.hk2014.hkpBreakableBodyController instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_breakingImpulse":
            case "breakingImpulse":
            {
                value = instance.m_breakingImpulse;
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
            case "m_breakingImpulse":
            case "breakingImpulse":
            {
                if (value is not float castValue) return false;
                instance.m_breakingImpulse = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
