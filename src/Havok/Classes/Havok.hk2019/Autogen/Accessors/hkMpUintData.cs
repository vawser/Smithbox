// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkMpUintData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkMpUint>
{
    public hkMpUintData(Havok.hk2019.hkMpUint instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_atoms":
            case "atoms":
            {
                value = instance.m_atoms;
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
            case "m_atoms":
            case "atoms":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_atoms = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
