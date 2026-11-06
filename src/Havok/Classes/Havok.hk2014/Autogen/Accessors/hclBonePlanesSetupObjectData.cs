// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclBonePlanesSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclBonePlanesSetupObject>
{
    public hclBonePlanesSetupObjectData(Havok.hk2014.hclBonePlanesSetupObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_simulationMesh":
            case "simulationMesh":
            {
                value = instance.m_simulationMesh;
                return true;
            }
            case "m_transformSetSetup":
            case "transformSetSetup":
            {
                value = instance.m_transformSetSetup;
                return true;
            }
            case "m_perParticlePlanes":
            case "perParticlePlanes":
            {
                value = instance.m_perParticlePlanes;
                return true;
            }
            case "m_globalPlanes":
            case "globalPlanes":
            {
                value = instance.m_globalPlanes;
                return true;
            }
            case "m_perParticleAngle":
            case "perParticleAngle":
            {
                value = instance.m_perParticleAngle;
                return true;
            }
            case "m_angleSpecifiedInDegrees":
            case "angleSpecifiedInDegrees":
            {
                value = instance.m_angleSpecifiedInDegrees;
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
                if (value is Havok.hk2014.hclSimulationSetupMesh castValue)
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
                if (value is Havok.hk2014.hclTransformSetSetupObject castValue)
                {
                    instance.m_transformSetSetup = castValue;
                    return true;
                }
                return false;
            }
            case "m_perParticlePlanes":
            case "perParticlePlanes":
            {
                if (value is not List<Havok.hk2014.hclBonePlanesSetupObject.PerParticlePlane> castValue) return false;
                instance.m_perParticlePlanes = castValue;
                return true;
            }
            case "m_globalPlanes":
            case "globalPlanes":
            {
                if (value is not List<Havok.hk2014.hclBonePlanesSetupObject.GlobalPlane> castValue) return false;
                instance.m_globalPlanes = castValue;
                return true;
            }
            case "m_perParticleAngle":
            case "perParticleAngle":
            {
                if (value is not List<Havok.hk2014.hclBonePlanesSetupObject.PerParticleAngle> castValue) return false;
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
