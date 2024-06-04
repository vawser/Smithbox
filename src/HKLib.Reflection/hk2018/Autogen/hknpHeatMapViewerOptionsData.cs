// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hknpHeatMapViewer;

namespace HKLib.Reflection.hk2018;

internal class hknpHeatMapViewerOptionsData : HavokData<Options> 
{
    public hknpHeatMapViewerOptionsData(HavokType type, Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collisionDetection":
            case "collisionDetection":
            {
                if (instance.m_collisionDetection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_contactSolving":
            case "contactSolving":
            {
                if (instance.m_contactSolving is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constraintBuilding":
            case "constraintBuilding":
            {
                if (instance.m_constraintBuilding is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constraintSolving":
            case "constraintSolving":
            {
                if (instance.m_constraintSolving is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particlesBodyCollision":
            case "particlesBodyCollision":
            {
                if (instance.m_particlesBodyCollision is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterProxies":
            case "characterProxies":
            {
                if (instance.m_characterProxies is not TGet castValue) return false;
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
