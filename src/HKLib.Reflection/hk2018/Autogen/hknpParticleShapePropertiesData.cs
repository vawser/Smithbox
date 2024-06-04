// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpParticleShapePropertiesData : HavokData<hknpParticleShapeProperties> 
{
    public hknpParticleShapePropertiesData(HavokType type, hknpParticleShapeProperties instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_particleParticleCollisionRadius":
            case "particleParticleCollisionRadius":
            {
                if (instance.m_particleParticleCollisionRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_innerRadius":
            case "innerRadius":
            {
                if (instance.m_innerRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outerRadius":
            case "outerRadius":
            {
                if (instance.m_outerRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inverseInnerRadius":
            case "inverseInnerRadius":
            {
                if (instance.m_inverseInnerRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_radiiRatio":
            case "radiiRatio":
            {
                if (instance.m_radiiRatio is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_radiiDifference":
            case "radiiDifference":
            {
                if (instance.m_radiiDifference is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_innerOuterRadius":
            case "innerOuterRadius":
            {
                if (instance.m_innerOuterRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleFaces":
            case "particleFaces":
            {
                if (instance.m_particleFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceVertices":
            case "faceVertices":
            {
                if (instance.m_faceVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_invInertia":
            case "invInertia":
            {
                if (instance.m_invInertia is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_convexRadii":
            case "convexRadii":
            {
                if (instance.m_convexRadii is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_allowedPenetration":
            case "allowedPenetration":
            {
                if (instance.m_allowedPenetration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hitPenetrationThreshold":
            case "hitPenetrationThreshold":
            {
                if (instance.m_hitPenetrationThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hitDistanceThreshold":
            case "hitDistanceThreshold":
            {
                if (instance.m_hitDistanceThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_movementThreshold":
            case "movementThreshold":
            {
                if (instance.m_movementThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                if (instance.m_shape is null)
                {
                    return true;
                }
                if (instance.m_shape is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_particleParticleCollisionRadius":
            case "particleParticleCollisionRadius":
            {
                if (value is not Vector128<float> castValue) return false;
                instance.m_particleParticleCollisionRadius = castValue;
                return true;
            }
            case "m_innerRadius":
            case "innerRadius":
            {
                if (value is not Vector128<float> castValue) return false;
                instance.m_innerRadius = castValue;
                return true;
            }
            case "m_outerRadius":
            case "outerRadius":
            {
                if (value is not Vector128<float> castValue) return false;
                instance.m_outerRadius = castValue;
                return true;
            }
            case "m_inverseInnerRadius":
            case "inverseInnerRadius":
            {
                if (value is not Vector128<float> castValue) return false;
                instance.m_inverseInnerRadius = castValue;
                return true;
            }
            case "m_radiiRatio":
            case "radiiRatio":
            {
                if (value is not Vector128<float> castValue) return false;
                instance.m_radiiRatio = castValue;
                return true;
            }
            case "m_radiiDifference":
            case "radiiDifference":
            {
                if (value is not Vector128<float> castValue) return false;
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
                if (value is not List<hknpParticle4Faces> castValue) return false;
                instance.m_particleFaces = castValue;
                return true;
            }
            case "m_faceVertices":
            case "faceVertices":
            {
                if (value is not List<hknpParticleFaceVerticesWithEffMass> castValue) return false;
                instance.m_faceVertices = castValue;
                return true;
            }
            case "m_invInertia":
            case "invInertia":
            {
                if (value is not Vector128<float> castValue) return false;
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
                if (value is not Vector128<float> castValue) return false;
                instance.m_allowedPenetration = castValue;
                return true;
            }
            case "m_hitPenetrationThreshold":
            case "hitPenetrationThreshold":
            {
                if (value is not Vector128<float> castValue) return false;
                instance.m_hitPenetrationThreshold = castValue;
                return true;
            }
            case "m_hitDistanceThreshold":
            case "hitDistanceThreshold":
            {
                if (value is not Vector128<float> castValue) return false;
                instance.m_hitDistanceThreshold = castValue;
                return true;
            }
            case "m_movementThreshold":
            case "movementThreshold":
            {
                if (value is not Vector128<float> castValue) return false;
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
                if (value is hknpConvexShape castValue)
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
