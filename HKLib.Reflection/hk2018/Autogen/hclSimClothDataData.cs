// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSimClothDataData : HavokData<hclSimClothData> 
{
    public hclSimClothDataData(HavokType type, hclSimClothData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_simulationInfo":
            case "simulationInfo":
            {
                if (instance.m_simulationInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleDatas":
            case "particleDatas":
            {
                if (instance.m_particleDatas is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fixedParticles":
            case "fixedParticles":
            {
                if (instance.m_fixedParticles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_doNormals":
            case "doNormals":
            {
                if (instance.m_doNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simOpIds":
            case "simOpIds":
            {
                if (instance.m_simOpIds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simClothPoses":
            case "simClothPoses":
            {
                if (instance.m_simClothPoses is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_staticConstraintSets":
            case "staticConstraintSets":
            {
                if (instance.m_staticConstraintSets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_antiPinchConstraintSets":
            case "antiPinchConstraintSets":
            {
                if (instance.m_antiPinchConstraintSets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collidableTransformMap":
            case "collidableTransformMap":
            {
                if (instance.m_collidableTransformMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_perInstanceCollidables":
            case "perInstanceCollidables":
            {
                if (instance.m_perInstanceCollidables is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxParticleRadius":
            case "maxParticleRadius":
            {
                if (instance.m_maxParticleRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_staticCollisionMasks":
            case "staticCollisionMasks":
            {
                if (instance.m_staticCollisionMasks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_actions":
            case "actions":
            {
                if (instance.m_actions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_totalMass":
            case "totalMass":
            {
                if (instance.m_totalMass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transferMotionData":
            case "transferMotionData":
            {
                if (instance.m_transferMotionData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transferMotionEnabled":
            case "transferMotionEnabled":
            {
                if (instance.m_transferMotionEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_landscapeCollisionEnabled":
            case "landscapeCollisionEnabled":
            {
                if (instance.m_landscapeCollisionEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_landscapeCollisionData":
            case "landscapeCollisionData":
            {
                if (instance.m_landscapeCollisionData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numLandscapeCollidableParticles":
            case "numLandscapeCollidableParticles":
            {
                if (instance.m_numLandscapeCollidableParticles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleIndices":
            case "triangleIndices":
            {
                if (instance.m_triangleIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleFlips":
            case "triangleFlips":
            {
                if (instance.m_triangleFlips is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pinchDetectionEnabled":
            case "pinchDetectionEnabled":
            {
                if (instance.m_pinchDetectionEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_perParticlePinchDetectionEnabledFlags":
            case "perParticlePinchDetectionEnabledFlags":
            {
                if (instance.m_perParticlePinchDetectionEnabledFlags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collidablePinchingDatas":
            case "collidablePinchingDatas":
            {
                if (instance.m_collidablePinchingDatas is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minPinchedParticleIndex":
            case "minPinchedParticleIndex":
            {
                if (instance.m_minPinchedParticleIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxPinchedParticleIndex":
            case "maxPinchedParticleIndex":
            {
                if (instance.m_maxPinchedParticleIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxCollisionPairs":
            case "maxCollisionPairs":
            {
                if (instance.m_maxCollisionPairs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_virtualCollisionPointsData":
            case "virtualCollisionPointsData":
            {
                if (instance.m_virtualCollisionPointsData is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
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
            case "m_simulationInfo":
            case "simulationInfo":
            {
                if (value is not hclSimClothData.OverridableSimulationInfo castValue) return false;
                instance.m_simulationInfo = castValue;
                return true;
            }
            case "m_particleDatas":
            case "particleDatas":
            {
                if (value is not List<hclSimClothData.ParticleData> castValue) return false;
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
            case "m_doNormals":
            case "doNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_doNormals = castValue;
                return true;
            }
            case "m_simOpIds":
            case "simOpIds":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_simOpIds = castValue;
                return true;
            }
            case "m_simClothPoses":
            case "simClothPoses":
            {
                if (value is not List<hclSimClothPose?> castValue) return false;
                instance.m_simClothPoses = castValue;
                return true;
            }
            case "m_staticConstraintSets":
            case "staticConstraintSets":
            {
                if (value is not List<hclConstraintSet?> castValue) return false;
                instance.m_staticConstraintSets = castValue;
                return true;
            }
            case "m_antiPinchConstraintSets":
            case "antiPinchConstraintSets":
            {
                if (value is not List<hclConstraintSet?> castValue) return false;
                instance.m_antiPinchConstraintSets = castValue;
                return true;
            }
            case "m_collidableTransformMap":
            case "collidableTransformMap":
            {
                if (value is not hclSimClothData.CollidableTransformMap castValue) return false;
                instance.m_collidableTransformMap = castValue;
                return true;
            }
            case "m_perInstanceCollidables":
            case "perInstanceCollidables":
            {
                if (value is not List<hclCollidable?> castValue) return false;
                instance.m_perInstanceCollidables = castValue;
                return true;
            }
            case "m_maxParticleRadius":
            case "maxParticleRadius":
            {
                if (value is not float castValue) return false;
                instance.m_maxParticleRadius = castValue;
                return true;
            }
            case "m_staticCollisionMasks":
            case "staticCollisionMasks":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_staticCollisionMasks = castValue;
                return true;
            }
            case "m_actions":
            case "actions":
            {
                if (value is not List<hclAction?> castValue) return false;
                instance.m_actions = castValue;
                return true;
            }
            case "m_totalMass":
            case "totalMass":
            {
                if (value is not float castValue) return false;
                instance.m_totalMass = castValue;
                return true;
            }
            case "m_transferMotionData":
            case "transferMotionData":
            {
                if (value is not hclSimClothData.TransferMotionData castValue) return false;
                instance.m_transferMotionData = castValue;
                return true;
            }
            case "m_transferMotionEnabled":
            case "transferMotionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_transferMotionEnabled = castValue;
                return true;
            }
            case "m_landscapeCollisionEnabled":
            case "landscapeCollisionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_landscapeCollisionEnabled = castValue;
                return true;
            }
            case "m_landscapeCollisionData":
            case "landscapeCollisionData":
            {
                if (value is not hclSimClothData.LandscapeCollisionData castValue) return false;
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
            case "m_pinchDetectionEnabled":
            case "pinchDetectionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_pinchDetectionEnabled = castValue;
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
                if (value is not List<hclSimClothData.CollidablePinchingData> castValue) return false;
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
            case "m_virtualCollisionPointsData":
            case "virtualCollisionPointsData":
            {
                if (value is not hclVirtualCollisionPointsData castValue) return false;
                instance.m_virtualCollisionPointsData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
