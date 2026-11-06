// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclSimClothDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclSimClothData>
{
    public hclSimClothDataData(Havok.hk2014.hclSimClothData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_simulationInfo":
            case "simulationInfo":
            {
                value = instance.m_simulationInfo;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_particleDatas":
            case "particleDatas":
            {
                value = instance.m_particleDatas;
                return true;
            }
            case "m_fixedParticles":
            case "fixedParticles":
            {
                value = instance.m_fixedParticles;
                return true;
            }
            case "m_triangleIndices":
            case "triangleIndices":
            {
                value = instance.m_triangleIndices;
                return true;
            }
            case "m_triangleFlips":
            case "triangleFlips":
            {
                value = instance.m_triangleFlips;
                return true;
            }
            case "m_totalMass":
            case "totalMass":
            {
                value = instance.m_totalMass;
                return true;
            }
            case "m_collidableTransformMap":
            case "collidableTransformMap":
            {
                value = instance.m_collidableTransformMap;
                return true;
            }
            case "m_perInstanceCollidables":
            case "perInstanceCollidables":
            {
                value = instance.m_perInstanceCollidables;
                return true;
            }
            case "m_staticConstraintSets":
            case "staticConstraintSets":
            {
                value = instance.m_staticConstraintSets;
                return true;
            }
            case "m_antiPinchConstraintSets":
            case "antiPinchConstraintSets":
            {
                value = instance.m_antiPinchConstraintSets;
                return true;
            }
            case "m_simClothPoses":
            case "simClothPoses":
            {
                value = instance.m_simClothPoses;
                return true;
            }
            case "m_actions":
            case "actions":
            {
                value = instance.m_actions;
                return true;
            }
            case "m_staticCollisionMasks":
            case "staticCollisionMasks":
            {
                value = instance.m_staticCollisionMasks;
                return true;
            }
            case "m_perParticlePinchDetectionEnabledFlags":
            case "perParticlePinchDetectionEnabledFlags":
            {
                value = instance.m_perParticlePinchDetectionEnabledFlags;
                return true;
            }
            case "m_collidablePinchingDatas":
            case "collidablePinchingDatas":
            {
                value = instance.m_collidablePinchingDatas;
                return true;
            }
            case "m_minPinchedParticleIndex":
            case "minPinchedParticleIndex":
            {
                value = instance.m_minPinchedParticleIndex;
                return true;
            }
            case "m_maxPinchedParticleIndex":
            case "maxPinchedParticleIndex":
            {
                value = instance.m_maxPinchedParticleIndex;
                return true;
            }
            case "m_maxCollisionPairs":
            case "maxCollisionPairs":
            {
                value = instance.m_maxCollisionPairs;
                return true;
            }
            case "m_maxParticleRadius":
            case "maxParticleRadius":
            {
                value = instance.m_maxParticleRadius;
                return true;
            }
            case "m_landscapeCollisionData":
            case "landscapeCollisionData":
            {
                value = instance.m_landscapeCollisionData;
                return true;
            }
            case "m_numLandscapeCollidableParticles":
            case "numLandscapeCollidableParticles":
            {
                value = instance.m_numLandscapeCollidableParticles;
                return true;
            }
            case "m_doNormals":
            case "doNormals":
            {
                value = instance.m_doNormals;
                return true;
            }
            case "m_transferMotionData":
            case "transferMotionData":
            {
                value = instance.m_transferMotionData;
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
            case "m_simulationInfo":
            case "simulationInfo":
            {
                if (value is not Havok.hk2014.hclSimClothData.OverridableSimulationInfo castValue) return false;
                instance.m_simulationInfo = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_particleDatas":
            case "particleDatas":
            {
                if (value is not List<Havok.hk2014.hclSimClothData.ParticleData> castValue) return false;
                instance.m_particleDatas = castValue;
                return true;
            }
            case "m_fixedParticles":
            case "fixedParticles":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_fixedParticles = castValue;
                return true;
            }
            case "m_triangleIndices":
            case "triangleIndices":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_triangleIndices = castValue;
                return true;
            }
            case "m_triangleFlips":
            case "triangleFlips":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_triangleFlips = castValue;
                return true;
            }
            case "m_totalMass":
            case "totalMass":
            {
                if (value is not float castValue) return false;
                instance.m_totalMass = castValue;
                return true;
            }
            case "m_collidableTransformMap":
            case "collidableTransformMap":
            {
                if (value is not Havok.hk2014.hclSimClothData.CollidableTransformMap castValue) return false;
                instance.m_collidableTransformMap = castValue;
                return true;
            }
            case "m_perInstanceCollidables":
            case "perInstanceCollidables":
            {
                if (value is not List<Havok.hk2014.hclCollidable?> castValue) return false;
                instance.m_perInstanceCollidables = castValue;
                return true;
            }
            case "m_staticConstraintSets":
            case "staticConstraintSets":
            {
                if (value is not List<Havok.hk2014.hclConstraintSet?> castValue) return false;
                instance.m_staticConstraintSets = castValue;
                return true;
            }
            case "m_antiPinchConstraintSets":
            case "antiPinchConstraintSets":
            {
                if (value is not List<Havok.hk2014.hclConstraintSet?> castValue) return false;
                instance.m_antiPinchConstraintSets = castValue;
                return true;
            }
            case "m_simClothPoses":
            case "simClothPoses":
            {
                if (value is not List<Havok.hk2014.hclSimClothPose?> castValue) return false;
                instance.m_simClothPoses = castValue;
                return true;
            }
            case "m_actions":
            case "actions":
            {
                if (value is not List<Havok.hk2014.hclAction?> castValue) return false;
                instance.m_actions = castValue;
                return true;
            }
            case "m_staticCollisionMasks":
            case "staticCollisionMasks":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_staticCollisionMasks = castValue;
                return true;
            }
            case "m_perParticlePinchDetectionEnabledFlags":
            case "perParticlePinchDetectionEnabledFlags":
            {
                if (value is not List<bool> castValue) return false;
                instance.m_perParticlePinchDetectionEnabledFlags = castValue;
                return true;
            }
            case "m_collidablePinchingDatas":
            case "collidablePinchingDatas":
            {
                if (value is not List<Havok.hk2014.hclSimClothData.CollidablePinchingData> castValue) return false;
                instance.m_collidablePinchingDatas = castValue;
                return true;
            }
            case "m_minPinchedParticleIndex":
            case "minPinchedParticleIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_minPinchedParticleIndex = castValue;
                return true;
            }
            case "m_maxPinchedParticleIndex":
            case "maxPinchedParticleIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_maxPinchedParticleIndex = castValue;
                return true;
            }
            case "m_maxCollisionPairs":
            case "maxCollisionPairs":
            {
                if (value is not uint castValue) return false;
                instance.m_maxCollisionPairs = castValue;
                return true;
            }
            case "m_maxParticleRadius":
            case "maxParticleRadius":
            {
                if (value is not float castValue) return false;
                instance.m_maxParticleRadius = castValue;
                return true;
            }
            case "m_landscapeCollisionData":
            case "landscapeCollisionData":
            {
                if (value is not Havok.hk2014.hclSimClothData.LandscapeCollisionData castValue) return false;
                instance.m_landscapeCollisionData = castValue;
                return true;
            }
            case "m_numLandscapeCollidableParticles":
            case "numLandscapeCollidableParticles":
            {
                if (value is not uint castValue) return false;
                instance.m_numLandscapeCollidableParticles = castValue;
                return true;
            }
            case "m_doNormals":
            case "doNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_doNormals = castValue;
                return true;
            }
            case "m_transferMotionData":
            case "transferMotionData":
            {
                if (value is not Havok.hk2014.hclSimClothData.TransferMotionData castValue) return false;
                instance.m_transferMotionData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
