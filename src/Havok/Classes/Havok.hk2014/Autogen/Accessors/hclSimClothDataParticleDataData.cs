// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclSimClothDataParticleDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclSimClothData.ParticleData>
{
    public hclSimClothDataParticleDataData(Havok.hk2014.hclSimClothData.ParticleData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_mass":
            case "mass":
            {
                value = instance.m_mass;
                return true;
            }
            case "m_invMass":
            case "invMass":
            {
                value = instance.m_invMass;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                value = instance.m_friction;
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
            case "m_mass":
            case "mass":
            {
                if (value is not float castValue) return false;
                instance.m_mass = castValue;
                return true;
            }
            case "m_invMass":
            case "invMass":
            {
                if (value is not float castValue) return false;
                instance.m_invMass = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                if (value is not float castValue) return false;
                instance.m_friction = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
