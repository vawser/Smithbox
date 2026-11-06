// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpHeatMapViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpHeatMapViewer.Options>
{
    public hknpHeatMapViewerOptionsData(Havok.hk2019.hknpHeatMapViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collisionDetection":
            case "collisionDetection":
            {
                value = instance.m_collisionDetection;
                return true;
            }
            case "m_contactSolving":
            case "contactSolving":
            {
                value = instance.m_contactSolving;
                return true;
            }
            case "m_constraintBuilding":
            case "constraintBuilding":
            {
                value = instance.m_constraintBuilding;
                return true;
            }
            case "m_constraintSolving":
            case "constraintSolving":
            {
                value = instance.m_constraintSolving;
                return true;
            }
            case "m_particlesBodyCollision":
            case "particlesBodyCollision":
            {
                value = instance.m_particlesBodyCollision;
                return true;
            }
            case "m_characterProxies":
            case "characterProxies":
            {
                value = instance.m_characterProxies;
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
            case "m_collisionDetection":
            case "collisionDetection":
            {
                if (value is not bool castValue) return false;
                instance.m_collisionDetection = castValue;
                return true;
            }
            case "m_contactSolving":
            case "contactSolving":
            {
                if (value is not bool castValue) return false;
                instance.m_contactSolving = castValue;
                return true;
            }
            case "m_constraintBuilding":
            case "constraintBuilding":
            {
                if (value is not bool castValue) return false;
                instance.m_constraintBuilding = castValue;
                return true;
            }
            case "m_constraintSolving":
            case "constraintSolving":
            {
                if (value is not bool castValue) return false;
                instance.m_constraintSolving = castValue;
                return true;
            }
            case "m_particlesBodyCollision":
            case "particlesBodyCollision":
            {
                if (value is not bool castValue) return false;
                instance.m_particlesBodyCollision = castValue;
                return true;
            }
            case "m_characterProxies":
            case "characterProxies":
            {
                if (value is not bool castValue) return false;
                instance.m_characterProxies = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
