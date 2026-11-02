// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpParticleTransformsAndVelocitiesData : HavokData<HKLib.hk2019.hknpParticleTransformsAndVelocities> 
{
    public hknpParticleTransformsAndVelocitiesData(HavokType type, HKLib.hk2019.hknpParticleTransformsAndVelocities instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_positions":
            case "positions":
            {
                if (instance.m_positions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_orientations":
            case "orientations":
            {
                if (instance.m_orientations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearVelocities":
            case "linearVelocities":
            {
                if (instance.m_linearVelocities is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularVelocities":
            case "angularVelocities":
            {
                if (instance.m_angularVelocities is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
