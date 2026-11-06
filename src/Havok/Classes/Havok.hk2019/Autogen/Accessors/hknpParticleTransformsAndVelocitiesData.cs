// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpParticleTransformsAndVelocitiesData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpParticleTransformsAndVelocities>
{
    public hknpParticleTransformsAndVelocitiesData(Havok.hk2019.hknpParticleTransformsAndVelocities instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_positions":
            case "positions":
            {
                value = instance.m_positions;
                return true;
            }
            case "m_orientations":
            case "orientations":
            {
                value = instance.m_orientations;
                return true;
            }
            case "m_linearVelocities":
            case "linearVelocities":
            {
                value = instance.m_linearVelocities;
                return true;
            }
            case "m_angularVelocities":
            case "angularVelocities":
            {
                value = instance.m_angularVelocities;
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
            case "m_positions":
            case "positions":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_positions = castValue;
                return true;
            }
            case "m_orientations":
            case "orientations":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_orientations = castValue;
                return true;
            }
            case "m_linearVelocities":
            case "linearVelocities":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_linearVelocities = castValue;
                return true;
            }
            case "m_angularVelocities":
            case "angularVelocities":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_angularVelocities = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
