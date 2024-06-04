// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBonePlanesSetupObjectData : HavokData<hclBonePlanesSetupObject> 
{
    public hclBonePlanesSetupObjectData(HavokType type, hclBonePlanesSetupObject instance) : base(type, instance) {}

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
            case "m_simulationMesh":
            case "simulationMesh":
            {
                if (instance.m_simulationMesh is null)
                {
                    return true;
                }
                if (instance.m_simulationMesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_transformSetSetup":
            case "transformSetSetup":
            {
                if (instance.m_transformSetSetup is null)
                {
                    return true;
                }
                if (instance.m_transformSetSetup is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_perParticlePlanes":
            case "perParticlePlanes":
            {
                if (instance.m_perParticlePlanes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_globalPlanes":
            case "globalPlanes":
            {
                if (instance.m_globalPlanes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_perParticleAngle":
            case "perParticleAngle":
            {
                if (instance.m_perParticleAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angleSpecifiedInDegrees":
            case "angleSpecifiedInDegrees":
            {
                if (instance.m_angleSpecifiedInDegrees is not TGet castValue) return false;
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
            case "m_simulationMesh":
            case "simulationMesh":
            {
                if (value is null)
                {
                    instance.m_simulationMesh = default;
                    return true;
                }
                if (value is hclSimulationSetupMesh castValue)
                {
                    instance.m_simulationMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_transformSetSetup":
            case "transformSetSetup":
            {
                if (value is null)
                {
                    instance.m_transformSetSetup = default;
                    return true;
                }
                if (value is hclTransformSetSetupObject castValue)
                {
                    instance.m_transformSetSetup = castValue;
                    return true;
                }
                return false;
            }
            case "m_perParticlePlanes":
            case "perParticlePlanes":
            {
                if (value is not List<hclBonePlanesSetupObject.PerParticlePlane> castValue) return false;
                instance.m_perParticlePlanes = castValue;
                return true;
            }
            case "m_globalPlanes":
            case "globalPlanes":
            {
                if (value is not List<hclBonePlanesSetupObject.GlobalPlane> castValue) return false;
                instance.m_globalPlanes = castValue;
                return true;
            }
            case "m_perParticleAngle":
            case "perParticleAngle":
            {
                if (value is not List<hclBonePlanesSetupObject.PerParticleAngle> castValue) return false;
                instance.m_perParticleAngle = castValue;
                return true;
            }
            case "m_angleSpecifiedInDegrees":
            case "angleSpecifiedInDegrees":
            {
                if (value is not bool castValue) return false;
                instance.m_angleSpecifiedInDegrees = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
