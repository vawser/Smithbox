// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpParticleShapePropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpParticleShapeProperties>
{
    public hknpParticleShapePropertiesData(Havok.hk2018.hknpParticleShapeProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_particleParticleCollisionRadius":
            case "particleParticleCollisionRadius":
            {
                value = instance.m_particleParticleCollisionRadius;
                return true;
            }
            case "m_innerRadius":
            case "innerRadius":
            {
                value = instance.m_innerRadius;
                return true;
            }
            case "m_outerRadius":
            case "outerRadius":
            {
                value = instance.m_outerRadius;
                return true;
            }
            case "m_inverseInnerRadius":
            case "inverseInnerRadius":
            {
                value = instance.m_inverseInnerRadius;
                return true;
            }
            case "m_radiiRatio":
            case "radiiRatio":
            {
                value = instance.m_radiiRatio;
                return true;
            }
            case "m_radiiDifference":
            case "radiiDifference":
            {
                value = instance.m_radiiDifference;
                return true;
            }
            case "m_innerOuterRadius":
            case "innerOuterRadius":
            {
                value = instance.m_innerOuterRadius;
                return true;
            }
            case "m_particleFaces":
            case "particleFaces":
            {
                value = instance.m_particleFaces;
                return true;
            }
            case "m_faceVertices":
            case "faceVertices":
            {
                value = instance.m_faceVertices;
                return true;
            }
            case "m_invInertia":
            case "invInertia":
            {
                value = instance.m_invInertia;
                return true;
            }
            case "m_convexRadii":
            case "convexRadii":
            {
                value = instance.m_convexRadii;
                return true;
            }
            case "m_allowedPenetration":
            case "allowedPenetration":
            {
                value = instance.m_allowedPenetration;
                return true;
            }
            case "m_hitPenetrationThreshold":
            case "hitPenetrationThreshold":
            {
                value = instance.m_hitPenetrationThreshold;
                return true;
            }
            case "m_hitDistanceThreshold":
            case "hitDistanceThreshold":
            {
                value = instance.m_hitDistanceThreshold;
                return true;
            }
            case "m_movementThreshold":
            case "movementThreshold":
            {
                value = instance.m_movementThreshold;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
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
            case "m_particleParticleCollisionRadius":
            case "particleParticleCollisionRadius":
            {
                if (value is not float castValue) return false;
                instance.m_particleParticleCollisionRadius = castValue;
                return true;
            }
            case "m_innerRadius":
            case "innerRadius":
            {
                if (value is not float castValue) return false;
                instance.m_innerRadius = castValue;
                return true;
            }
            case "m_outerRadius":
            case "outerRadius":
            {
                if (value is not float castValue) return false;
                instance.m_outerRadius = castValue;
                return true;
            }
            case "m_inverseInnerRadius":
            case "inverseInnerRadius":
            {
                if (value is not float castValue) return false;
                instance.m_inverseInnerRadius = castValue;
                return true;
            }
            case "m_radiiRatio":
            case "radiiRatio":
            {
                if (value is not float castValue) return false;
                instance.m_radiiRatio = castValue;
                return true;
            }
            case "m_radiiDifference":
            case "radiiDifference":
            {
                if (value is not float castValue) return false;
                instance.m_radiiDifference = castValue;
                return true;
            }
            case "m_innerOuterRadius":
            case "innerOuterRadius":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_innerOuterRadius = castValue;
                return true;
            }
            case "m_particleFaces":
            case "particleFaces":
            {
                if (value is not List<Havok.hk2018.hknpParticle4Faces> castValue) return false;
                instance.m_particleFaces = castValue;
                return true;
            }
            case "m_faceVertices":
            case "faceVertices":
            {
                if (value is not List<Havok.hk2018.hknpParticleFaceVerticesWithEffMass> castValue) return false;
                instance.m_faceVertices = castValue;
                return true;
            }
            case "m_invInertia":
            case "invInertia":
            {
                if (value is not float castValue) return false;
                instance.m_invInertia = castValue;
                return true;
            }
            case "m_convexRadii":
            case "convexRadii":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_convexRadii = castValue;
                return true;
            }
            case "m_allowedPenetration":
            case "allowedPenetration":
            {
                if (value is not float castValue) return false;
                instance.m_allowedPenetration = castValue;
                return true;
            }
            case "m_hitPenetrationThreshold":
            case "hitPenetrationThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_hitPenetrationThreshold = castValue;
                return true;
            }
            case "m_hitDistanceThreshold":
            case "hitDistanceThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_hitDistanceThreshold = castValue;
                return true;
            }
            case "m_movementThreshold":
            case "movementThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_movementThreshold = castValue;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is Havok.hk2018.hknpConvexShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
