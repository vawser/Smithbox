// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbFootIkModifierInternalLegDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbFootIkModifier.InternalLegData>
{
    public hkbFootIkModifierInternalLegDataData(Havok.hk2018.hkbFootIkModifier.InternalLegData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_groundPosition":
            case "groundPosition":
            {
                value = instance.m_groundPosition;
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
            case "m_groundPosition":
            case "groundPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_groundPosition = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
