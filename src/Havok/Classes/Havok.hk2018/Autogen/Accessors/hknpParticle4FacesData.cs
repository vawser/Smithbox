// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpParticle4FacesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpParticle4Faces>
{
    public hknpParticle4FacesData(Havok.hk2018.hknpParticle4Faces instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_directions":
            case "directions":
            {
                value = instance.m_directions;
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
            case "m_directions":
            case "directions":
            {
                if (value is not Havok.hk2018.hkFourTransposedPointsf castValue) return false;
                instance.m_directions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
