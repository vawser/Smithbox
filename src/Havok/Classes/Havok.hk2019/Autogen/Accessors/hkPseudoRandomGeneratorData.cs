// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkPseudoRandomGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkPseudoRandomGenerator>
{
    public hkPseudoRandomGeneratorData(Havok.hk2019.hkPseudoRandomGenerator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_seed":
            case "seed":
            {
                value = instance.m_seed;
                return true;
            }
            case "m_current":
            case "current":
            {
                value = instance.m_current;
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
            case "m_seed":
            case "seed":
            {
                if (value is not uint castValue) return false;
                instance.m_seed = castValue;
                return true;
            }
            case "m_current":
            case "current":
            {
                if (value is not uint castValue) return false;
                instance.m_current = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
