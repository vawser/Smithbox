// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpParticleFaceVerticesWithEffMassData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpParticleFaceVerticesWithEffMass>
{
    public hknpParticleFaceVerticesWithEffMassData(Havok.hk2018.hknpParticleFaceVerticesWithEffMass instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceVertices":
            case "faceVertices":
            {
                value = instance.m_faceVertices;
                return true;
            }
            case "m_effectiveMasses":
            case "effectiveMasses":
            {
                value = instance.m_effectiveMasses;
                return true;
            }
            case "m_contactRadius":
            case "contactRadius":
            {
                value = instance.m_contactRadius;
                return true;
            }
            case "m_distance":
            case "distance":
            {
                value = instance.m_distance;
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
            case "m_faceVertices":
            case "faceVertices":
            {
                if (value is not Havok.hk2018.hkFourTransposedPointsf castValue) return false;
                instance.m_faceVertices = castValue;
                return true;
            }
            case "m_effectiveMasses":
            case "effectiveMasses":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_effectiveMasses = castValue;
                return true;
            }
            case "m_contactRadius":
            case "contactRadius":
            {
                if (value is not float castValue) return false;
                instance.m_contactRadius = castValue;
                return true;
            }
            case "m_distance":
            case "distance":
            {
                if (value is not float castValue) return false;
                instance.m_distance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
